using Apollo.Orchestration.Services;
using Microsoft.AspNetCore.Builder;
using Newtonsoft.Json;

namespace Apollo.Orchestration.API.Routes;

internal sealed class IntegrationRoutes : RoutesBase
{
    protected override void MapGet(WebApplication app)
    {
        app.MapGet("/integrations", (IIntegrationService integrationService) =>
            integrationService.Integrations.Select(i => new
            {
                i.Id,
                i.Name,
                i.Description,
                Steps = i.Steps.Select(s => new
                {
                    s.TriggerType,
                    s.Trigger,
                    s.Output,
                    TransformSpec = JsonConvert.SerializeObject(s.TransformSpec)
                })
            })
        );
    }
}