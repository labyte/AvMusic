namespace AvMusic.Core.Models;

public sealed class Playlist
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public string? Library { get; init; }

    public string? Type { get; init; }
}
