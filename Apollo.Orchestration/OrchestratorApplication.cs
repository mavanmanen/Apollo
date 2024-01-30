using Apollo.Orchestration.API.Routes;
using Apollo.Orchestration.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Apollo.Orchestration;

public sealed class OrchestratorApplication
{
    private readonly WebApplication _app;

    internal OrchestratorApplication(WebApplication app)
    {
        _app = app;
        
        var integrationService = app.Services.GetRequiredService<IIntegrationService>();
        integrationService.SetupIntegrations();
        integrationService.SetupInternal();

        var routes = typeof(OrchestratorApplication).Assembly.GetTypes()
            .Where(t => t.IsAssignableTo(typeof(RoutesBase)))
            .Where(t => !t.IsAbstract);

        foreach (var route in routes)
        {
            var instance = (RoutesBase)Activator.CreateInstance(route)!;
            instance.Map(app);
        }
    }

    public static OrchestratorApplicationBuilder CreateBuilder() => new();

    public Task RunAsync() => _app.RunAsync();
}