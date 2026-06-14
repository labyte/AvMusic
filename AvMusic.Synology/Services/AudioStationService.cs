using AvMusic.Core.Models;
using AvMusic.Core.Session;
using AvMusic.Synology.Client;
using AvMusic.Synology.Dto;
using AvMusic.Synology.Mapping;
using Microsoft.Extensions.Logging;

namespace AvMusic.Synology.Services;

public sealed class AudioStationService : AudioStationServiceBase, IAudioStationService
{
    public AudioStationService(
        ISynologyApiClient client,
        IApiInfoService apiInfo,
        ISessionState session,
        ILogger<AudioStationService> logger)
        : base(client, apiInfo, session, logger)
    {
    }

    public async Task<PagedResult<Song>> GetSongsAsync(
        int offset = 0,
        int limit = 100,
        string library = AudioStationApiNames.DefaultLibrary,
        string sortBy = "title",
        string sortDirection = "ASC",
        CancellationToken cancellationToken = default)
    {
        var data = await PostApiAsync<SongListDataDto>(
            AudioStationApiNames.Song,
            new Dictionary<string, string>
            {
                ["method"] = "list",
                ["library"] = library,
                ["offset"] = offset.ToString(),
                ["limit"] = limit.ToString(),
                ["additional"] = AudioStationApiNames.SongAdditional,
                ["sort_by"] = sortBy,
                ["sort_direction"] = sortDirection
            },
            cancellationToken).ConfigureAwait(false);

        return EntityMapper.ToPagedSongs(data);
    }

    public async Task<PagedResult<Song>> GetSongsByAlbumAsync(
        string album,
        string albumArtist,
        int limit = 10000,
        CancellationToken cancellationToken = default)
    {
        var data = await PostApiAsync<SongListDataDto>(
            AudioStationApiNames.Song,
            new Dictionary<string, string>
            {
                ["method"] = "list",
                ["library"] = AudioStationApiNames.DefaultLibrary,
                ["limit"] = limit.ToString(),
                ["album"] = Uri.EscapeDataString(album),
                ["album_artist"] = Uri.EscapeDataString(albumArtist),
                ["additional"] = AudioStationApiNames.SongAdditional,
                ["sort_by"] = "album",
                ["sort_direction"] = "DESC"
            },
            cancellationToken).ConfigureAwait(false);

        return EntityMapper.ToPagedSongs(data);
    }

    public async Task<PagedResult<Song>> GetSongsByArtistAsync(
        string artist,
        int offset = 0,
        int limit = 1000,
        CancellationToken cancellationToken = default)
    {
        var data = await PostApiAsync<SongListDataDto>(
            AudioStationApiNames.Song,
            new Dictionary<string, string>
            {
                ["method"] = "list",
                ["library"] = AudioStationApiNames.DefaultLibrary,
                ["offset"] = offset.ToString(),
                ["limit"] = limit.ToString(),
                ["artist"] = Uri.EscapeDataString(artist),
                ["additional"] = AudioStationApiNames.SongAdditional,
                ["sort_by"] = "album",
                ["sort_direction"] = "DESC"
            },
            cancellationToken).ConfigureAwait(false);

        return EntityMapper.ToPagedSongs(data);
    }

    public async Task<PagedResult<Song>> GetSongsByRatingAsync(
        int minRating,
        int offset = 0,
        int limit = 1000,
        CancellationToken cancellationToken = default)
    {
        var data = await PostApiAsync<SongListDataDto>(
            AudioStationApiNames.Song,
            new Dictionary<string, string>
            {
                ["method"] = "list",
                ["library"] = AudioStationApiNames.DefaultLibrary,
                ["offset"] = offset.ToString(),
                ["limit"] = limit.ToString(),
                ["songs_offset"] = offset.ToString(),
                ["songs_limit"] = limit.ToString(),
                ["additional"] = AudioStationApiNames.SongAdditional,
                ["song_rating_meq"] = minRating.ToString(),
                ["sort_by"] = "song_rating",
                ["sort_direction"] = "DESC"
            },
            cancellationToken).ConfigureAwait(false);

        return EntityMapper.ToPagedSongs(data);
    }

    public async Task<Song?> GetSongByIdAsync(string songId, CancellationToken cancellationToken = default)
    {
        var data = await PostApiAsync<SongListDataDto>(
            AudioStationApiNames.Song,
            new Dictionary<string, string>
            {
                ["method"] = "getinfo",
                ["library"] = AudioStationApiNames.DefaultLibrary,
                ["id"] = songId,
                ["additional"] = AudioStationApiNames.SongAdditional,
                ["songs_limit"] = "1",
                ["songs_offset"] = "0",
                ["sort_by"] = "album",
                ["sort_direction"] = "DESC"
            },
            cancellationToken).ConfigureAwait(false);

        return data.Songs.Count > 0 ? EntityMapper.ToSong(data.Songs[0]) : null;
    }

    public async Task<PagedResult<Song>> GetRandomSongsAsync(
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var data = await PostApiAsync<SongListDataDto>(
            AudioStationApiNames.Song,
            new Dictionary<string, string>
            {
                ["method"] = "list",
                ["library"] = AudioStationApiNames.DefaultLibrary,
                ["limit"] = limit.ToString(),
                ["additional"] = AudioStationApiNames.SongAdditional,
                ["sort_by"] = "random"
            },
            cancellationToken).ConfigureAwait(false);

        return EntityMapper.ToPagedSongs(data);
    }

    public Task SetSongRatingAsync(string songId, int rating, CancellationToken cancellationToken = default) =>
        PostApiWithoutDataAsync(
            AudioStationApiNames.Song,
            new Dictionary<string, string>
            {
                ["method"] = "setrating",
                ["id"] = songId,
                ["rating"] = rating.ToString()
            },
            cancellationToken);

    public async Task<PagedResult<Album>> GetAlbumsAsync(
        int offset = 0,
        int limit = 1000,
        string sortBy = "year",
        string sortDirection = "ASC",
        CancellationToken cancellationToken = default)
    {
        var data = await PostApiAsync<AlbumListDataDto>(
            AudioStationApiNames.Album,
            new Dictionary<string, string>
            {
                ["method"] = "list",
                ["library"] = AudioStationApiNames.DefaultLibrary,
                ["offset"] = offset.ToString(),
                ["limit"] = limit.ToString(),
                ["additional"] = "avg_rating",
                ["sort_by"] = sortBy,
                ["sort_direction"] = sortDirection
            },
            cancellationToken).ConfigureAwait(false);

        return EntityMapper.ToPagedAlbums(data);
    }

    public async Task<PagedResult<Album>> GetAlbumsByArtistAsync(
        string artist,
        int limit = 1000,
        CancellationToken cancellationToken = default)
    {
        var data = await PostApiAsync<AlbumListDataDto>(
            AudioStationApiNames.Album,
            new Dictionary<string, string>
            {
                ["method"] = "list",
                ["library"] = AudioStationApiNames.DefaultLibrary,
                ["limit"] = limit.ToString(),
                ["artist"] = Uri.EscapeDataString(artist),
                ["additional"] = "avg_rating",
                ["sort_by"] = "year",
                ["sort_direction"] = "ASC"
            },
            cancellationToken).ConfigureAwait(false);

        return EntityMapper.ToPagedAlbums(data);
    }

    public Task<PagedResult<Album>> GetRecentAlbumsAsync(
        int limit = 50,
        CancellationToken cancellationToken = default) =>
        GetAlbumsAsync(0, limit, "time", "DESC", cancellationToken);

    public async Task<PagedResult<Album>> GetAlbumsByGenreAsync(
        string genre,
        int limit = 1000,
        CancellationToken cancellationToken = default)
    {
        var data = await PostApiAsync<AlbumListDataDto>(
            AudioStationApiNames.Album,
            new Dictionary<string, string>
            {
                ["method"] = "list",
                ["library"] = AudioStationApiNames.DefaultLibrary,
                ["limit"] = limit.ToString(),
                ["genre_filter"] = Uri.EscapeDataString(genre),
                ["additional"] = "avg_rating",
                ["sort_by"] = "year",
                ["sort_direction"] = "ASC"
            },
            cancellationToken).ConfigureAwait(false);

        return EntityMapper.ToPagedAlbums(data);
    }

    public async Task<PagedResult<Artist>> GetArtistsAsync(
        int offset = 0,
        int limit = 1000,
        string sortBy = "name",
        string sortDirection = "ASC",
        CancellationToken cancellationToken = default)
    {
        var data = await PostApiAsync<ArtistListDataDto>(
            AudioStationApiNames.Artist,
            new Dictionary<string, string>
            {
                ["method"] = "list",
                ["library"] = AudioStationApiNames.DefaultLibrary,
                ["offset"] = offset.ToString(),
                ["limit"] = limit.ToString(),
                ["additional"] = "avg_rating",
                ["sort_by"] = sortBy,
                ["sort_direction"] = sortDirection
            },
            cancellationToken).ConfigureAwait(false);

        return EntityMapper.ToPagedArtists(data);
    }

    public async Task<IReadOnlyList<Genre>> GetGenresAsync(CancellationToken cancellationToken = default)
    {
        var data = await PostApiAsync<GenreListDataDto>(
            AudioStationApiNames.Genre,
            new Dictionary<string, string>
            {
                ["method"] = "list",
                ["library"] = "shared",
                ["limit"] = "1000",
                ["additional"] = "avg_rating",
                ["sort_by"] = "name",
                ["sort_direction"] = "ASC"
            },
            cancellationToken).ConfigureAwait(false);

        return data.Genres.ConvertAll(EntityMapper.ToGenre);
    }

    public async Task<FolderBrowseResult> BrowseFolderAsync(
        string? folderId = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, string>
        {
            ["method"] = "list",
            ["library"] = AudioStationApiNames.DefaultLibrary,
            ["additional"] = AudioStationApiNames.SongAdditional,
            ["sort_by"] = "title"
        };

        if (!string.IsNullOrEmpty(folderId))
        {
            parameters["id"] = folderId;
        }

        var data = await PostApiAsync<FolderListDataDto>(
            AudioStationApiNames.Folder,
            parameters,
            cancellationToken).ConfigureAwait(false);

        return EntityMapper.ToFolderBrowse(data);
    }

    public async Task<IReadOnlyList<Playlist>> GetPlaylistsAsync(
        CancellationToken cancellationToken = default)
    {
        var data = await PostApiAsync<PlaylistListDataDto>(
            AudioStationApiNames.Playlist,
            new Dictionary<string, string>
            {
                ["method"] = "list",
                ["library"] = AudioStationApiNames.DefaultLibrary,
                ["sort_direction"] = "ASC"
            },
            cancellationToken).ConfigureAwait(false);

        return data.Playlists.ConvertAll(EntityMapper.ToPlaylist);
    }

    public async Task<IReadOnlyList<Song>> GetPlaylistSongsAsync(
        string playlistId,
        CancellationToken cancellationToken = default)
    {
        var data = await PostApiAsync<PlaylistDetailDataDto>(
            AudioStationApiNames.Playlist,
            new Dictionary<string, string>
            {
                ["method"] = "getinfo",
                ["library"] = AudioStationApiNames.DefaultLibrary,
                ["id"] = Uri.EscapeDataString(playlistId),
                ["additional"] = AudioStationApiNames.PlaylistSongAdditional,
                ["sort_direction"] = "ASC"
            },
            cancellationToken).ConfigureAwait(false);

        var playlist = data.Playlists.FirstOrDefault();
        return playlist?.Additional?.Songs.ConvertAll(EntityMapper.ToSong) ?? [];
    }

    public async Task<SearchResult> SearchAsync(
        string keyword,
        int limit = 50,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        var data = await PostApiAsync<SearchDataDto>(
            AudioStationApiNames.Search,
            new Dictionary<string, string>
            {
                ["method"] = "list",
                ["library"] = AudioStationApiNames.DefaultLibrary,
                ["keyword"] = Uri.EscapeDataString(keyword),
                ["limit"] = limit.ToString(),
                ["offset"] = offset.ToString(),
                ["additional"] = "song_tag,song_audio"
            },
            cancellationToken).ConfigureAwait(false);

        return EntityMapper.ToSearchResult(data);
    }

    public async Task<string?> GetLyricsAsync(string songId, CancellationToken cancellationToken = default)
    {
        var data = await PostApiAsync<LyricsDataDto>(
            AudioStationApiNames.Lyrics,
            new Dictionary<string, string>
            {
                ["method"] = "getlyrics",
                ["library"] = AudioStationApiNames.DefaultLibrary,
                ["id"] = songId
            },
            cancellationToken).ConfigureAwait(false);

        return string.IsNullOrWhiteSpace(data.Lyrics) ? null : data.Lyrics;
    }
}
