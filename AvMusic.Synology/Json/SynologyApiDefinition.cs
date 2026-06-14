using System.Text.Json.Serialization;

namespace AvMusic.Synology.Json;

public sealed class SynologyApiDefinition
{
    [JsonPropertyName("path")]
    public string Path { get; init; } = string.Empty;

    [JsonPropertyName("minVersion")]
    public int MinVersion { get; init; }

    [JsonPropertyName("maxVersion")]
    public int MaxVersion { get; init; }

    [JsonPropertyName("requestFormat")]
    public string? RequestFormat { get; init; }
}
