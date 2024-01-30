using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Apollo.Handling.Services.Smee;

internal class SmeeInstance(string url, ILogger logger) : IDisposable
{
    private Process? _process;
    private readonly ProcessStartInfo _processStartInfo = new("cmd.exe")
    {
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        RedirectStandardInput = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };
    
    public void Start()
    {
        _process = new Process
        {
            StartInfo = _processStartInfo
        };
        _process.OutputDataReceived += (_, e) =>
        {
            if (e.Data?.StartsWith("Forwarding", StringComparison.Ordinal) ?? false)
            {
                logger.LogInformation("{Data}", e.Data);
            }
        };
        _process.ErrorDataReceived += (_, e) => logger.LogError("{Data}", e.Data);
        _process.Start();
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();
        _process.StandardInput.WriteLine($"smee -t {url}");
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _process?.Close();
        _process?.Dispose();
    }
}