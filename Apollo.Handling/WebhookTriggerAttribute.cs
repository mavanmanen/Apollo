namespace Apollo.Handling;

[AttributeUsage(AttributeTargets.Class)]
public class WebhookTriggerAttribute(string routeName, Type messageType) : Attribute
{
    public string RouteName { get; } = routeName;
    public Type MessageType { get; } = messageType;
}