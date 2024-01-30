using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Apollo.Core.Services;

public class ServiceBus : IServiceBus
{
    private readonly IServiceProvider _services;
    private readonly ILogger _logger;
    private readonly IModel _channel;

    public ServiceBus(IConfiguration configuration, IServiceProvider services, ILoggerFactory loggerFactory)
    {
        _services = services;
        _logger = loggerFactory.CreateLogger(nameof(ServiceBus));
        var connectionFactory = new ConnectionFactory
        {
            Uri = new Uri(configuration.GetConnectionString(nameof(ServiceBus))!)
        };

        var connection = connectionFactory.CreateConnection();
        _channel = connection.CreateModel();
    }

    public void DeclareExchange(string exchangeName, string exchangeType)
    {
        _logger.LogInformation("Creating exchange: {Exchange}", exchangeName);
        _channel.ExchangeDeclare(exchangeName, exchangeType);
    }

    public void DeclareQueue(string queueName)
    {
        _logger.LogInformation("Creating queue: {Queue}", queueName);
        _channel.QueueDeclare(queueName, false, false, false);
    }
    
    public void BindQueue(string exchangeName, string queueName, string routingKey)
    {
        _logger.LogInformation("Binding: {Queue} -> {Exchange}", queueName, exchangeName);
        _channel.QueueBind(queueName, exchangeName, routingKey);
    }

    public void ConsumeQueue(string queueName, Func<string, IServiceProvider, IServiceBusSender, Task> handler)
    {
        _logger.LogInformation("Adding consumer for queue: {Queue}", queueName);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (_, e) =>
        {
            await using var scope = _services.CreateAsyncScope();
            var serviceBus = scope.ServiceProvider.GetRequiredService<IServiceBus>();
            var message = Encoding.UTF8.GetString(e.Body.ToArray());
            _logger.LogInformation("Handling message: {Message}", message);
            await handler(message, scope.ServiceProvider, serviceBus);
        };
        _channel.BasicConsume(queueName, true, consumer);
    }

    public void SendMessage(string exchangeName, object message) => 
        _channel.BasicPublish(exchangeName, string.Empty, null, GetMessageBytes(message));

    public void SendMessage(string exchangeName, string routingKey, object message) =>
        _channel.BasicPublish(exchangeName, routingKey, null, GetMessageBytes(message));

    public void SendMessageBatch(string queueName, IEnumerable<object> messages)
    {
        var batch = _channel.CreateBasicPublishBatch();
        foreach (var message in messages.Select(GetMessageBytes))
        {
            batch.Add(string.Empty, queueName, false, null, message);
        }
        batch.Publish();
    }

    public void SendMessageBatch(string queueName, string routingKey, IEnumerable<object> messages)
    {
        var batch = _channel.CreateBasicPublishBatch();
        foreach (var message in messages.Select(GetMessageBytes))
        {
            batch.Add(queueName, routingKey, false, null, message);
        }
        batch.Publish();
    }

    private static ReadOnlyMemory<byte> GetMessageBytes(object message)
    {
        var json = JsonConvert.SerializeObject(message);
        return  new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(json));
    }
}