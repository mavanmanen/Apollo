using Apollo.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Apollo.Orchestration.Services;

internal sealed class IntegrationStep(HandlerTypes triggerType, string trigger, JToken? triggerParameters, string output, JToken? transformSpec)
{
    [JsonConverter(typeof(StringEnumConverter))]
    public HandlerTypes TriggerType { get; } = triggerType;
    public string Trigger { get; } = trigger;
    public JToken? TriggerParameters { get; } = triggerParameters;
    public string Output { get; } = output;
    public JToken? TransformSpec { get; } = transformSpec;
}