namespace Apollo.Core.Services;

public interface IServiceBusSender
{
    public void SendMessage(string queueName, object message);
    public void SendMessage(string queueName, string routingKey, object message);
    public void SendMessageBatch(string queueName, IEnumerable<object> messages);
    public void SendMessageBatch(string queueName, string routingKey, IEnumerable<object> messages);
}