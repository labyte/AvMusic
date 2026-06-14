namespace AvMusic.Synology;

/// <summary>
/// 群晖 Web API 错误。
/// </summary>
public sealed class SynologyApiException : Exception
{
    public SynologyApiException(string message) : base(message)
    {
    }

    public SynologyApiException(string message, int errorCode)
        : base($"{message} (错误码: {errorCode})")
    {
        ErrorCode = errorCode;
    }

    public int? ErrorCode { get; }
}
