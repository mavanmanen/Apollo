using Apollo.Core;

namespace Apollo.Handling;

[AttributeUsage(AttributeTargets.Class)]
public class HandlerAttribute(HandlerTypes type, string system, string name, string description = "") : Attribute
{
    public HandlerTypes Type { get; } = type;
    public string System { get; } = system;
    public string Name { get; } = name;
    public string Description { get; } = description;
}