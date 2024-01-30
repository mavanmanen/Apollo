namespace Apollo.Handling.Services.Smee;

internal interface ISmeeService : IDisposable
{
    public void CreateInstance(string endpoint);
}