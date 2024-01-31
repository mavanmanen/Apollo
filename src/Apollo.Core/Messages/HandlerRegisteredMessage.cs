using System.Text.Json.Serialization;

namespace Apollo.Core.Messages;

[method: JsonConstructor]
public class HandlerRegisteredMessage(HandlerTypes type, string system, string name, string description)
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public HandlerTypes Type { get; } = type;
    public string System { get; } = system;
    public string Name { get; } = name;
    public string Description { get; } = description;
}