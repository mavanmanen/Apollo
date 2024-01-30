using Apollo.Core.Messages;

namespace Apollo.Handling;

public interface IAsyncHandler
{
    
}

public interface IAsyncHandler<in TMessage> : IAsyncHandler
{
    public Task<ResultMessage> HandleAsync(TMessage message);
}