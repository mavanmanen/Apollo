using System.Reflection;
using Apollo.Handling.Services;
using Apollo.Handling.Services.WebhookBasicAuthentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Apollo.Handling;

public sealed class HandlerApplication
{
    public IServiceProvider Services => _app.Services;
    
    private readonly WebApplication _app;

    internal HandlerApplication(WebApplication app)
    {
        _app = app;
        
        var handlerService = _app.Services.GetRequiredService<IHandlerService>();
        handlerService.SetupHandlers(Assembly.GetExecutingAssembly(), _app);

        _app.MapGet("/", () => Results.Ok()).AllowAnonymous();
    }
    
    public static HandlerApplicationBuilder CreateBuilder() => new();

    public Task RunAsync() => _app.RunAsync();

    public void UseWebhookBasicAuthentication()
    {
        _app.UseAuthentication();
        _app.UseAuthorization();
            
        using var scope = _app.Services.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IWebhookBasicAuthenticationService>();
        userService.InitUser();
    }
}