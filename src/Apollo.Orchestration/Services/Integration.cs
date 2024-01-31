namespace Apollo.Orchestration.Services;

internal sealed class Integration(Guid id, string name, string description, IntegrationStep[] steps)
{
    public Guid Id { get; } = id;
    public string Name { get; } = name;
    public string Description { get; } = description;
    public IntegrationStep[] Steps { get; } = steps;
}