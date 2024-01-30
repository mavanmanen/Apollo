using Apollo.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Apollo.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static void RegisterServiceBus(this IServiceCollection services)
    {
        services.AddSingleton<IServiceBus, ServiceBus>();
        services.AddSingleton<IServiceBusSender>(s => s.GetRequiredService<IServiceBus>());
    }
}