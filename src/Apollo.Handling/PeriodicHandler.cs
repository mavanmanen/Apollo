using Apollo.Core.Messages;
using Cronos;
using Microsoft.Extensions.DependencyInjection;

namespace Apollo.Handling;

public abstract class PeriodicHandler
{
    internal event EventHandler<ResultMessage>? Handled;

    private string _sourceId = null!;
    private IServiceProvider _services = null!;
    private CronExpression _cron = null!;
    private Timer _timer = null!;

    internal void Init(string sourceId, IServiceProvider services, PeriodicHandlerParameters parameters)
    {
        _sourceId = sourceId;
        _services = services;
        _cron = CronExpression.Parse(parameters.Period);
        _timer = new Timer(InternalHandleAsync, null, Timeout.Infinite, Timeout.Infinite);
    }

    internal void Start()
    {
        _timer.Change(GetNextOccurrence(), Timeout.InfiniteTimeSpan);
    }

    private TimeSpan GetNextOccurrence() =>
        (TimeSpan)(_cron.GetNextOccurrence(DateTime.UtcNow) - DateTime.UtcNow)!;

    private async void InternalHandleAsync(object? _)
    {
        await using var scope = _services.CreateAsyncScope();
        var result = await HandleAsync(_sourceId, scope.ServiceProvider);
        Handled?.Invoke(this, result);
        Start();
    }

    protected abstract Task<ResultMessage> HandleAsync(string sourceId, IServiceProvider services);
}