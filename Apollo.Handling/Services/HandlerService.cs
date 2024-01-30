using System.Reflection;
using System.Text.Json;
using Apollo.Core.Messages;
using Apollo.Core.Services;
using Apollo.Handling.Services.Smee;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Apollo.Handling.Services;

internal class HandlerService(
    IServiceBus serviceBus,
    ILoggerFactory loggerFactory,
    IPeriodicHandlerService periodicHandlerService)
    : IHandlerService
{
    private const string SetupRoutingKey = "setup-integration";

    public void SetupHandlers(Assembly assembly, WebApplication app)
    {
        var handlers = new List<HandlerAttribute>();
        handlers.AddRange(SetupServiceBusHandlers(assembly));
        handlers.AddRange(SetupWebhookHandlers(assembly, app));
        handlers.AddRange(SetupPeriodicHandlers(assembly));
        ReportHandlers(handlers);
    }

    private IEnumerable<HandlerAttribute> SetupServiceBusHandlers(Assembly assembly)
    {
        var handlerDefinitions = assembly.DefinedTypes
            .Where(t => t.GetCustomAttribute<ServiceBusTriggerAttribute>() is not null)
            .Select(t => new
            {
                Type = t,
                HandlerAttribute = t.GetCustomAttribute<HandlerAttribute>()!,
                TriggerAttribute = t.GetCustomAttribute<ServiceBusTriggerAttribute>()!,
                OutputExchangeAttribute = t.GetCustomAttribute<OutputExchangeAttribute>()
            }).ToArray();

        foreach (var handlerDefinition in handlerDefinitions)
        {
            var logger = loggerFactory.CreateLogger(handlerDefinition.TriggerAttribute.ExchangeName);
            var setupQueueName = $"{handlerDefinition.TriggerAttribute.ExchangeName}/{SetupRoutingKey}";
            
            serviceBus.DeclareExchange(handlerDefinition.TriggerAttribute.ExchangeName, ExchangeType.Topic);
            SetupQueueConsumer(handlerDefinition.TriggerAttribute.ExchangeName, setupQueueName, SetupRoutingKey, (setupRaw, _, _) =>
            {
                var message = DeserializeMessage<SetupIntegrationMessage>(setupRaw);
                if (message is null)
                {
                    return Task.CompletedTask;
                }
                
                SetupQueueConsumer(handlerDefinition.TriggerAttribute.ExchangeName, message.QueueName, message.RoutingKey, async (raw, s, sbs) =>
                    await HandleHandler(raw, handlerDefinition.Type, handlerDefinition.TriggerAttribute.MessageType, s, sbs, logger));
                
                return Task.CompletedTask;
            });
            
            if (handlerDefinition.OutputExchangeAttribute is not null)
            {
                serviceBus.DeclareExchange(handlerDefinition.OutputExchangeAttribute.ExchangeName, ExchangeType.Topic);
                var outputSetupQueueName = $"{handlerDefinition.OutputExchangeAttribute.ExchangeName}/{SetupRoutingKey}";
                SetupQueueConsumer(handlerDefinition.OutputExchangeAttribute.ExchangeName, outputSetupQueueName, SetupRoutingKey, (setupRaw, _, _) =>
                {
                    var message = (SetupIntegrationMessage)DeserializeMessage(setupRaw, typeof(SetupIntegrationMessage))!;
                    SetupQueueConsumer(handlerDefinition.OutputExchangeAttribute.ExchangeName, message.QueueName, message.RoutingKey);
                    return Task.CompletedTask;
                });
            }
        }

        return handlerDefinitions.Select(h => h.HandlerAttribute);
    }

    private IEnumerable<HandlerAttribute> SetupWebhookHandlers(Assembly assembly, WebApplication app)
    {
        var handlerDefinitions = assembly.DefinedTypes
            .Where(t => t.GetCustomAttribute<WebhookTriggerAttribute>() is not null)
            .Select(t => new
            {
                Type = t,
                HandlerAttribute = t.GetCustomAttribute<HandlerAttribute>()!,
                TriggerAttribute = t.GetCustomAttribute<WebhookTriggerAttribute>()!,
                OutputExchangeAttributes = t.GetCustomAttributes<OutputExchangeAttribute>()
            }).ToArray();

        foreach (var handlerDefinition in handlerDefinitions)
        {
            var logger = loggerFactory.CreateLogger(handlerDefinition.TriggerAttribute.RouteName);
            app.MapPost(handlerDefinition.TriggerAttribute.RouteName, async (IServiceBusSender sbs, IServiceProvider s, JsonElement body) =>
            {
                var message = body.GetRawText();
                await HandleHandler(message, handlerDefinition.Type, handlerDefinition.TriggerAttribute.MessageType, s, sbs, logger);
            });
            
            foreach (var outputExchangeAttribute in handlerDefinition.OutputExchangeAttributes)
            {
                serviceBus.DeclareExchange(outputExchangeAttribute.ExchangeName, ExchangeType.Fanout);
            }

            if (app.Environment.IsDevelopment())
            {
                var smeeService = app.Services.GetRequiredService<ISmeeService>();
                smeeService.CreateInstance(handlerDefinition.TriggerAttribute.RouteName);
            }
        }

        return handlerDefinitions.Select(h => h.HandlerAttribute);
    }
    
    private IEnumerable<HandlerAttribute> SetupPeriodicHandlers(Assembly assembly)
    {
        var handlerDefinitions = assembly.DefinedTypes
            .Where(t => t.IsAssignableTo(typeof(PeriodicHandler)))
            .Select(t => new
            {
                Type = t,
                HandlerAttribute = t.GetCustomAttribute<HandlerAttribute>()!,
                OutputExchangeAttributes = t.GetCustomAttributes<OutputExchangeAttribute>()
            }).ToArray();

        foreach (var handlerDefinition in handlerDefinitions)
        {
            foreach (var outputExchangeAttribute in handlerDefinition.OutputExchangeAttributes)
            {
                serviceBus.DeclareExchange(outputExchangeAttribute.ExchangeName, ExchangeType.Topic);
                var outputSetupQueueName = $"{outputExchangeAttribute.ExchangeName}/{SetupRoutingKey}";
                SetupQueueConsumer(outputExchangeAttribute.ExchangeName, outputSetupQueueName, SetupRoutingKey, (setupRaw, _, _) =>
                {
                    var message = DeserializeMessage<SetupIntegrationMessage>(setupRaw);
                    if (message?.Parameters is null)
                    {
                        return Task.CompletedTask;
                    }

                    SetupQueueConsumer(outputExchangeAttribute.ExchangeName, message.QueueName, message.RoutingKey);

                    var parameters = message.Parameters.ToObject<PeriodicHandlerParameters>();
                    if (parameters is null)
                    {
                        return Task.CompletedTask;
                    }

                    var logger = loggerFactory.CreateLogger(handlerDefinition.HandlerAttribute.Name);
                    periodicHandlerService.StartNew(handlerDefinition.Type, message.RoutingKey, parameters, (_, resultMessage) =>
                    {
                        HandleResult(resultMessage, serviceBus, logger);
                    });
                    return Task.CompletedTask;
                });
            }
        }

        return handlerDefinitions.Select(h => h.HandlerAttribute);
    }

    private void ReportHandlers(IEnumerable<HandlerAttribute> handlers)
    {
        const string reportQueue = "orchestrator/handlers";
        serviceBus.DeclareQueue(reportQueue);
        var messages = handlers.Select(h => new HandlerRegisteredMessage(h.Type, h.System, h.Name, h.Description));
        serviceBus.SendMessageBatch(reportQueue, messages);
    }
    
    private void SetupQueueConsumer(string exchange, string queue, string routingKey, Func<string, IServiceProvider, IServiceBusSender, Task>? handler = null)
    {
        serviceBus.DeclareQueue(queue);
        serviceBus.BindQueue(exchange, queue, routingKey);
        if (handler is not null)
        {
            serviceBus.ConsumeQueue(queue, handler);
        }
    }

    private static async Task HandleHandler(string raw, Type handlerType, Type messageType, IServiceProvider services, IServiceBusSender serviceBus, ILogger logger)
    {
        var message = DeserializeMessage(raw, messageType);
        if (message is null)
        {
            return;
        }

        var handler = services.GetRequiredService(handlerType);
        var result = await (Task<ResultMessage>)handlerType.GetMethod("HandleAsync")!.Invoke(handler, [message])!;
        HandleResult(result, serviceBus, logger);
    }

    private static TMessage? DeserializeMessage<TMessage>(string raw) =>
        (TMessage?)DeserializeMessage(raw, typeof(TMessage));

    private static object? DeserializeMessage(string raw, Type messageType)
    {
        if (messageType == typeof(string))
        {
            return raw;
        }
        
        raw = raw.Replace("\\\"", "\"");
        if (raw.StartsWith('"') && raw.EndsWith('"'))
        {
            raw = raw[1..^1];
        }

        return JsonConvert.DeserializeObject(raw, messageType);
    }

    private static void HandleResult(ResultMessage? result, IServiceBusSender serviceBus, ILogger logger)
    {
        if (result is null)
        {
            return;
        }

        if (!result.Success)
        {
            if (result.Message is not null)
            {
                logger.LogError("Handler failed: {Message}", result.Message);
            }

            return;
        }

        if (result.TargetQueue is null || result.ResultData is null || !result.ResultData.Any())
        {
            return;
        }
        
        if (result.ResultData.Count() > 1)
        {
            if (string.IsNullOrEmpty(result.SourceId))
            {
                serviceBus.SendMessageBatch(result.TargetQueue, result.ResultData);
            }
            else
            {
                serviceBus.SendMessageBatch(result.TargetQueue, result.SourceId, result.ResultData);
            }
        }
        else
        {
            if (string.IsNullOrEmpty(result.SourceId))
            {
                serviceBus.SendMessage(result.TargetQueue, result.ResultData.Single());
            }
            else
            {
                serviceBus.SendMessage(result.TargetQueue, result.SourceId, result.ResultData.Single());
            }
        }
    }
}