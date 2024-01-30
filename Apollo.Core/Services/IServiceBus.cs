namespace Apollo.Core.Services;

public interface IServiceBus : IServiceBusSender
{
    public void DeclareQueue(string queueName);
    public void DeclareExchange(string exchangeName, string exchangeType);
    public void BindQueue(string exchangeName, string queueName, string routingKey);
    public void ConsumeQueue(string queueName, Func<string, IServiceProvider, IServiceBusSender, Task> handler);
}