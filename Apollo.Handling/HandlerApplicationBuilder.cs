using System.Reflection;
using System.Security.Claims;
using Apollo.Core.Extensions;
using Apollo.Database.Extensions;
using Apollo.Handling.Services;
using Apollo.Handling.Services.Smee;
using Apollo.Handling.Services.WebhookBasicAuthentication;
using idunno.Authentication.Basic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Apollo.Handling;

public sealed class HandlerApplicationBuilder
{
    public IServiceCollection Services => _builder.Services;
    public ConfigurationManager Configuration => _builder.Configuration;
    public IWebHostEnvironment Environment => _builder.Environment;
    
    private readonly WebApplicationBuilder _builder;

    internal HandlerApplicationBuilder()
    {
        _builder = WebApplication.CreateBuilder();
    }

    public void AddWebhookBasicAuthentication(Action<WebhookBasicAuthenticationConfiguration> optionsBuilder)
    {
        var configuration = new WebhookBasicAuthenticationConfiguration();
        optionsBuilder(configuration);
        AddWebhookBasicAuthentication(configuration);
    }

    public void AddWebhookBasicAuthentication(IConfiguration config)
    {
        var configuration = new WebhookBasicAuthenticationConfiguration();
        config.Bind(configuration);
        AddWebhookBasicAuthentication(configuration);
    }

    public void AddWebhookBasicAuthentication(WebhookBasicAuthenticationConfiguration configuration)
    {
        Services.Configure<WebhookBasicAuthenticationConfiguration>(options =>
        {
            options.Username = configuration.Username;
            options.Password = configuration.Password;
            options.Salt = configuration.Salt;
        });
        
        Services.AddScoped<IWebhookBasicAuthenticationService, WebhookBasicAuthenticationService>();
        Services.AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme).AddBasic(options =>
        {
            options.Realm = configuration.Realm;
            options.Events = new BasicAuthenticationEvents
            {
                OnValidateCredentials = context =>
                {
                    var userService = context.HttpContext.RequestServices.GetRequiredService<IWebhookBasicAuthenticationService>();
                    if (userService.ValidateUser(context.Username, context.Password))
                    {
                        var claims = new[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, context.Username, ClaimValueTypes.String, context.Options.ClaimsIssuer),
                            new Claim(ClaimTypes.Name, context.Username, ClaimValueTypes.String, context.Options.ClaimsIssuer)
                        };
                        context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, context.Scheme.Name));
                        context.Success();
                    }

                    return Task.CompletedTask;
                }
            };

            if (Environment.IsDevelopment())
            {
                options.AllowInsecureProtocol = true;
            }
        });
        
        Services.AddAuthorization();
    }

    public void AddSmeeService()
    {
        Services.AddSingleton<ISmeeService, SmeeService>(s =>
        {
            var loggerFactory = s.GetRequiredService<ILoggerFactory>();
            return new SmeeService(Configuration.GetValue<string>("URLS")!, loggerFactory);
        });
    }
    
    public HandlerApplication Build()
    {
        Services.RegisterServiceBus();
        Services.AddSingleton<IHandlerService, HandlerService>();
        Services.AddSingleton<IPeriodicHandlerService, PeriodicHandlerService>();
        foreach (var handler in Assembly.GetExecutingAssembly().GetExportedTypes().Where(t => t.IsAssignableTo(typeof(IAsyncHandler))))
        {
            Services.AddScoped(handler);
            Services.AddScoped(typeof(IAsyncHandler), s => s.GetRequiredService(handler));
        }

        return new HandlerApplication(_builder.Build());
    }
}