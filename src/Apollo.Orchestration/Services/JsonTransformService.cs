using Jolt.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Apollo.Orchestration.Services;

public class JsonTransformService : IJsonTransformService
{
    public string Transform(string input, string transformSpec) =>
        Chainr
            .FromSpec(JToken.Parse(transformSpec))
            .Transform(JToken.Parse(input))
            .ToString(Formatting.None);
}