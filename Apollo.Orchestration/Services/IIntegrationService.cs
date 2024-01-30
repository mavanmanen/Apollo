namespace Apollo.Orchestration.Services;

internal interface IIntegrationService
{
    public void SetupIntegrations();
    public void SetupInternal();
    public Integration[] Integrations { get; }
    public Handler[] Handlers { get; }
}