using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Apollo.Core.Messages;

[method: JsonConstructor]
public class SetupIntegrationMessage(string queueName, string routingKey, JToken? parameters = null)
{
    public string QueueName { get; set; } = queueName;
    public string RoutingKey { get; set; } = routingKey;
    public JToken? Parameters { get; } = parameters;
}