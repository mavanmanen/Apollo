using System.Text.Json;
using Apollo.Core;
using Apollo.Core.Messages;
using Apollo.Core.Services;

namespace Apollo.Orchestration.Services;

internal sealed class IntegrationService(IServiceBus serviceBus, IJsonTransformService transformService) : IIntegrationService
{
    public Integration[] Integrations { get; private set; } = [];
    public Handler[] Handlers => _handlers.ToArray();
    private readonly List<Handler> _handlers = [];

    public void SetupIntegrations()
    {
        Integrations = Directory.EnumerateFiles("./Integrations", "*.json")
                .Select(f => new FileInfo(f))
                .Select(f => File.ReadAllText(f.FullName))
                .Select(s => JsonSerializer.Deserialize<Integration>(s))
                .OfType<Integration>()
                .ToArray();
        
        foreach (var integration in Integrations)
        {
            foreach (var step in integration.Steps)
            {
                var id = integration.Id.ToString("N");
                var triggerQueueName = $"{step.Trigger}-{id}";
                var outputQueueName = $"{step.Output}-{id}";

                const string setupRoutingKey = "setup-integration";
                if (step.TriggerType is HandlerTypes.ServiceBus or HandlerTypes.Periodic)
                {
                    serviceBus.SendMessage(step.Trigger, setupRoutingKey, new SetupIntegrationMessage(triggerQueueName, id, step.TriggerParameters));
                }
                
                serviceBus.SendMessage(step.Output, setupRoutingKey, new SetupIntegrationMessage(outputQueueName, id));
                
                serviceBus.DeclareQueue(triggerQueueName);
                var routingKey = step.TriggerType == HandlerTypes.ServiceBus ? id : string.Empty;
                serviceBus.BindQueue(step.Trigger, triggerQueueName, routingKey);
                serviceBus.ConsumeQueue(triggerQueueName, (message, _, sbs) =>
                    HandleIntegrationStep(id, step, message, sbs));
            }
        }
    }

    public void SetupInternal()
    {
        serviceBus.ConsumeQueue("orchestrator/handlers", (messageRaw, _, _) =>
        {
            var message = JsonSerializer.Deserialize<HandlerRegisteredMessage>(messageRaw);
            if (message is null)
            {
                return Task.CompletedTask;
            }
            
            var handler = new Handler(message.Type, message.System, message.Name, message.Description);
            if (!_handlers.Contains(handler))
            {
                _handlers.Add(handler);
            }
            
            return Task.CompletedTask;
        });
    }

    private Task HandleIntegrationStep(string id, IntegrationStep step, string message, IServiceBusSender serviceBus)
    {
        var messageJson = step.TransformSpec is not null
            ? transformService.Transform(message, step.TransformSpec.ToString()!)
            : message;
                    
        serviceBus.SendMessage(step.Output, id, messageJson);
        return Task.CompletedTask;
    }
}