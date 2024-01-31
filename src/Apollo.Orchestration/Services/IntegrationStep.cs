using System.Text.Json;
using System.Text.Json.Serialization;
using Apollo.Core;

namespace Apollo.Orchestration.Services;

internal sealed class IntegrationStep(HandlerTypes triggerType, string trigger, JsonElement? triggerParameters, string output, JsonElement? transformSpec)
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public HandlerTypes TriggerType { get; } = triggerType;
    public string Trigger { get; } = trigger;
    public JsonElement? TriggerParameters { get; } = triggerParameters;
    public string Output { get; } = output;
    public JsonElement? TransformSpec { get; } = transformSpec;
}