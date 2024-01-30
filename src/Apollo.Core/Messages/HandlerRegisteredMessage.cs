using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Apollo.Core.Messages;

[method: System.Text.Json.Serialization.JsonConstructor]
public class HandlerRegisteredMessage(HandlerTypes type, string system, string name, string description)
{
    [JsonConverter(typeof(StringEnumConverter))]
    public HandlerTypes Type { get; } = type;
    public string System { get; } = system;
    public string Name { get; } = name;
    public string Description { get; } = description;
}