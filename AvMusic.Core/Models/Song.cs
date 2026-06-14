namespace AvMusic.Core.Models;

/// <summary>
/// 领域层歌曲模型（与群晖 API DTO 解耦）。
/// </summary>
public sealed class Song
{
    public required string Id { get; init; }

    public required string Title { get; init; }

    public string? Artist { get; init; }

    public string? Album { get; init; }

    public string? AlbumArtist { get; init; }

    public int DurationSeconds { get; init; }

    public int Rating { get; init; }

    public string? Path { get; init; }

    public string? Codec { get; init; }
}
