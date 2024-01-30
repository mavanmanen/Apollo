using Newtonsoft.Json;

namespace Apollo.Core.Messages;

public class ResultMessage : MessageBase
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public IEnumerable<object>? ResultData { get; set; }
    public string? TargetQueue { get; set; }

    [JsonConstructor]
    public ResultMessage()
    {
    }

    private ResultMessage(string sourceId, bool success = true, string? message = null, string? targetQueue = null, IEnumerable<object>? resultData = null) : base(sourceId)
    {
        Success = success;
        Message = message;
        TargetQueue = targetQueue;
        ResultData = resultData;
    }

    public static ResultMessage Ok(string sourceId) => new(sourceId);
    public static ResultMessage Cancel(string sourceId, string message) => new(sourceId, message: message);
    public static ResultMessage Fail(string sourceId, string errorMessage) => new(sourceId, false, errorMessage);
    public static ResultMessage Reply(string targetQueue, object resultData) => new(string.Empty, targetQueue: targetQueue, resultData: [resultData]);
    public static ResultMessage Reply(string sourceId, string targetQueue, object resultData) => new(sourceId, targetQueue: targetQueue, resultData: [resultData]);
    public static ResultMessage Reply(string sourceId, string targetQueue, IEnumerable<object> resultData) => new(sourceId, targetQueue: targetQueue, resultData: resultData);
}