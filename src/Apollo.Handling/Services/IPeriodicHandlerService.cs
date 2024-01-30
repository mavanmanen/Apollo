using Apollo.Core.Messages;

namespace Apollo.Handling.Services;

internal interface IPeriodicHandlerService
{
    public void StartNew(Type handlerType, string sourceId, PeriodicHandlerParameters parameters, EventHandler<ResultMessage> resultHandler);
}