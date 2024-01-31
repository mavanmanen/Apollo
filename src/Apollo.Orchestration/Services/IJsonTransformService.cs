namespace Apollo.Orchestration.Services;

public interface IJsonTransformService
{
    public string Transform(string input, string transformSpec);
}