using System.Text.Json.Serialization;

namespace AvMusic.Synology.Json;

/// <summary>
/// 群晖 Web API 通用响应包装。
/// </summary>
public sealed class SynologyResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("data")]
    public T? Data { get; init; }

    [JsonPropertyName("error")]
    public SynologyError? Error { get; init; }
}

public sealed class SynologyError
{
    [JsonPropertyName("code")]
    public int Code { get; init; }
}
