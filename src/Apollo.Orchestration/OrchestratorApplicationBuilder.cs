using Apollo.Core.Extensions;
using Apollo.Orchestration.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Apollo.Orchestration;

public sealed class OrchestratorApplicationBuilder
{
    public IServiceCollection Services => _builder.Services;
    public ConfigurationManager Configuration => _builder.Configuration;
    public IWebHostEnvironment Environment => _builder.Environment;
    
    private readonly WebApplicationBuilder _builder;

    internal OrchestratorApplicationBuilder()
    {
        _builder = WebApplication.CreateBuilder();
    }

    public OrchestratorApplication Build()
    {
        Configuration.AddEnvironmentVariables();
        Services.RegisterServiceBus();
        Services.AddSingleton<IJsonTransformService, JsonTransformService>();
        Services.AddSingleton<IIntegrationService, IntegrationService>();

        return new OrchestratorApplication(_builder.Build());
    }
}