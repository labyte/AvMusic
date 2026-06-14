using AvMusic.Core.Models;

namespace AvMusic.Synology.Services;

/// <summary>
/// 群晖 Audio Station 音乐库访问服务。
/// </summary>
public interface IAudioStationService
{
    Task<PagedResult<Song>> GetSongsAsync(
        int offset = 0,
        int limit = 100,
        string library = "all",
        string sortBy = "title",
        string sortDirection = "ASC",
        CancellationToken cancellationToken = default);

    Task<PagedResult<Song>> GetSongsByAlbumAsync(
        string album,
        string albumArtist,
        int limit = 10000,
        CancellationToken cancellationToken = default);

    Task<PagedResult<Song>> GetSongsByArtistAsync(
        string artist,
        int offset = 0,
        int limit = 1000,
        CancellationToken cancellationToken = default);

    Task<PagedResult<Song>> GetSongsByRatingAsync(
        int minRating,
        int offset = 0,
        int limit = 1000,
        CancellationToken cancellationToken = default);

    Task<Song?> GetSongByIdAsync(string songId, CancellationToken cancellationToken = default);

    Task<PagedResult<Song>> GetRandomSongsAsync(
        int limit = 100,
        CancellationToken cancellationToken = default);

    Task SetSongRatingAsync(string songId, int rating, CancellationToken cancellationToken = default);

    Task<PagedResult<Album>> GetAlbumsAsync(
        int offset = 0,
        int limit = 1000,
        string sortBy = "year",
        string sortDirection = "ASC",
        CancellationToken cancellationToken = default);

    Task<PagedResult<Album>> GetAlbumsByArtistAsync(
        string artist,
        int limit = 1000,
        CancellationToken cancellationToken = default);

    Task<PagedResult<Album>> GetRecentAlbumsAsync(
        int limit = 50,
        CancellationToken cancellationToken = default);

    Task<PagedResult<Album>> GetAlbumsByGenreAsync(
        string genre,
        int limit = 1000,
        CancellationToken cancellationToken = default);

    Task<PagedResult<Artist>> GetArtistsAsync(
        int offset = 0,
        int limit = 1000,
        string sortBy = "name",
        string sortDirection = "ASC",
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Genre>> GetGenresAsync(CancellationToken cancellationToken = default);

    Task<FolderBrowseResult> BrowseFolderAsync(
        string? folderId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Playlist>> GetPlaylistsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Song>> GetPlaylistSongsAsync(
        string playlistId,
        CancellationToken cancellationToken = default);

    Task<SearchResult> SearchAsync(
        string keyword,
        int limit = 50,
        int offset = 0,
        CancellationToken cancellationToken = default);

    Task<string?> GetLyricsAsync(string songId, CancellationToken cancellationToken = default);
}
