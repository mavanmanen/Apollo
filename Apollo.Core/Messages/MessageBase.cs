namespace Apollo.Core.Messages;

public abstract class MessageBase
{
    public string SourceId { get; set; } = null!;

    protected MessageBase() 
    {
    }

    protected MessageBase(string sourceId)
    {
        SourceId = sourceId;
    }
}