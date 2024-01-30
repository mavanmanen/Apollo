using Apollo.Core.Messages;

namespace Apollo.Handling.Services;

internal class PeriodicHandlerService(IServiceProvider services) : IPeriodicHandlerService
{
    private readonly List<PeriodicHandler> _handlers = [];
    
    public void StartNew(Type handlerType, string sourceId, PeriodicHandlerParameters parameters, EventHandler<ResultMessage> resultHandler)
    {
        var instance = (PeriodicHandler)Activator.CreateInstance(handlerType)!;
        instance.Handled += resultHandler;
        instance.Init(sourceId, services, parameters);
        instance.Start();
        _handlers.Add(instance);
    }
}