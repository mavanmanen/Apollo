using Apollo.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Apollo.Orchestration.Services;

internal sealed class Handler(HandlerTypes type, string system, string name, string description)
{
    [JsonConverter(typeof(StringEnumConverter))]
    public HandlerTypes Type { get; } = type;
    public string System { get; } = system;
    public string Name { get; } = name;
    public string Description { get; } = description;
}