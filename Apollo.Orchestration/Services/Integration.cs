namespace Apollo.Orchestration.Services;

internal sealed class Integration(string name, string description, IntegrationStep[] steps)
{
    public Guid Id { get; set; }
    public string Name { get; } = name;
    public string Description { get; } = description;
    public IntegrationStep[] Steps { get; } = steps;
}