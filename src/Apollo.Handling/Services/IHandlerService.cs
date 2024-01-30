using System.Reflection;
using Microsoft.AspNetCore.Builder;

namespace Apollo.Handling.Services;

internal interface IHandlerService
{
    public void SetupHandlers(Assembly assembly, WebApplication app);
}