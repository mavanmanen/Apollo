using Microsoft.Extensions.Logging;

namespace Apollo.Handling.Services.Smee;

internal class SmeeService(string baseUrl, ILoggerFactory loggerFactory) : ISmeeService
{
    private readonly List<SmeeInstance> _instances = [];
    private readonly ILogger _logger = loggerFactory.CreateLogger(nameof(SmeeService));
    
    public void CreateInstance(string endpoint)
    {
        var instance = new SmeeInstance($"{baseUrl}/{endpoint}", _logger);
        instance.Start();
        _instances.Add(instance);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        foreach (var instance in _instances)
        {
            instance.Dispose();
        }
    }
}