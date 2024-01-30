namespace Apollo.Handling;

[AttributeUsage(AttributeTargets.Class)]
public class ServiceBusTriggerAttribute(string exchangeName, Type messageType) : Attribute
{
    public string ExchangeName { get; } = exchangeName;
    public Type MessageType { get; } = messageType;
}