using Apollo.Orchestration.Services;
using Microsoft.AspNetCore.Builder;

namespace Apollo.Orchestration.API.Routes;

internal sealed class HandlerRoutes : RoutesBase
{
    protected override void MapGet(WebApplication app)
    {
        app.MapGet("/handlers", (IIntegrationService integrationService) =>
            integrationService.Handlers.Select(h => new
            {
                Type = h.Type.ToString(),
                h.System,
                h.Name,
                h.Description
            })
        );
    }
}