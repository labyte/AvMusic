namespace AvMusic.Synology.Streaming;

public sealed record StreamDownloadResult(
    string? FilePath,
    int? HttpStatusCode,
    string? ErrorDetail);
