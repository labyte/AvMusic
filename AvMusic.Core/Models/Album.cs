namespace AvMusic.Core.Models;

public sealed class Album
{
    public required string Name { get; init; }

    public string? AlbumArtist { get; init; }

    public string? DisplayArtist { get; init; }

    public int Year { get; init; }

    public double AvgRating { get; init; }
}
