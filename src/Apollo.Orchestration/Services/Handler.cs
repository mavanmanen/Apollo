using System.Text.Json.Serialization;
using Apollo.Core;

namespace Apollo.Orchestration.Services;

internal sealed class Handler(HandlerTypes type, string system, string name, string description)
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public HandlerTypes Type { get; } = type;
    public string System { get; } = system;
    public string Name { get; } = name;
    public string Description { get; } = description;
}