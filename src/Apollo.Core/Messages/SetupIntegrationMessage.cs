using System.Text.Json;
using System.Text.Json.Serialization;

namespace Apollo.Core.Messages;

[method: JsonConstructor]
public class SetupIntegrationMessage(string queueName, string routingKey, JsonElement? parameters = null)
{
    public string QueueName { get; set; } = queueName;
    public string RoutingKey { get; set; } = routingKey;
    public JsonElement? Parameters { get; } = parameters;
}