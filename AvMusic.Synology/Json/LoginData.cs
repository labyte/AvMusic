using System.Text.Json.Serialization;

namespace AvMusic.Synology.Json;

public sealed class LoginData
{
    [JsonPropertyName("sid")]
    public string Sid { get; init; } = string.Empty;

    [JsonPropertyName("device_id")]
    public string? DeviceId { get; init; }

    [JsonPropertyName("is_portal_port")]
    public bool IsPortalPort { get; init; }
}
