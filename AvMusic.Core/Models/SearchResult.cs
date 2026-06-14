namespace AvMusic.Core.Models;

public sealed class SearchResult
{
    public required IReadOnlyList<Song> Songs { get; init; }

    public int SongTotal { get; init; }

    public required IReadOnlyList<Album> Albums { get; init; }

    public int AlbumTotal { get; init; }

    public required IReadOnlyList<Artist> Artists { get; init; }

    public int ArtistTotal { get; init; }
}
