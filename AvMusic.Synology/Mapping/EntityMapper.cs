using AvMusic.Core.Models;
using AvMusic.Synology.Dto;

namespace AvMusic.Synology.Mapping;

internal static class EntityMapper
{
    public static Song ToSong(SongItemDto dto)
    {
        var tag = dto.Additional?.SongTag;
        var audio = dto.Additional?.SongAudio;
        return new Song
        {
            Id = dto.Id,
            Title = dto.Title,
            Path = dto.Path,
            Artist = tag?.Artist,
            Album = tag?.Album,
            AlbumArtist = tag?.AlbumArtist,
            DurationSeconds = audio?.Duration ?? 0,
            Rating = (int)(dto.Additional?.SongRating?.Rating ?? 0),
            Codec = audio?.Codec,
            Container = audio?.Container,
            Bitrate = audio?.Bitrate ?? 0,
            Frequency = audio?.Frequency ?? 0,
            FileSize = audio?.Filesize ?? 0
        };
    }

    public static Album ToAlbum(AlbumItemDto dto) => new()
    {
        Name = dto.Name,
        AlbumArtist = dto.AlbumArtist,
        DisplayArtist = dto.DisplayArtist ?? dto.Artist,
        Year = dto.Year,
        AvgRating = dto.Additional?.AvgRating?.Rating ?? 0
    };

    public static Artist ToArtist(ArtistItemDto dto) => new()
    {
        Name = dto.Name,
        AvgRating = dto.Additional?.AvgRating?.Rating ?? 0
    };

    public static Genre ToGenre(GenreItemDto dto) => new()
    {
        Name = dto.Name,
        AvgRating = dto.Additional?.AvgRating?.Rating ?? 0
    };

    public static Playlist ToPlaylist(PlaylistItemDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        Library = dto.Library,
        Type = dto.Type
    };

    public static PagedResult<Song> ToPagedSongs(SongListDataDto data) => new()
    {
        Items = data.Songs.ConvertAll(ToSong),
        Offset = data.Offset,
        Total = data.Total
    };

    public static PagedResult<Album> ToPagedAlbums(AlbumListDataDto data) => new()
    {
        Items = data.Albums.ConvertAll(ToAlbum),
        Offset = data.Offset,
        Total = data.Total
    };

    public static PagedResult<Artist> ToPagedArtists(ArtistListDataDto data) => new()
    {
        Items = data.Artists.ConvertAll(ToArtist),
        Offset = data.Offset,
        Total = data.Total
    };

    public static FolderBrowseResult ToFolderBrowse(FolderListDataDto data)
    {
        var subfolders = new List<MusicFolder>();
        var songs = new List<Song>();

        foreach (var item in data.Items)
        {
            if (string.Equals(item.Type, "file", StringComparison.OrdinalIgnoreCase))
            {
                songs.Add(ToSong(item));
            }
            else
            {
                subfolders.Add(new MusicFolder
                {
                    Id = item.Id,
                    Title = item.Title
                });
            }
        }

        return new FolderBrowseResult
        {
            FolderId = data.Id,
            Subfolders = subfolders,
            Songs = songs,
            Total = data.Total
        };
    }

    public static SearchResult ToSearchResult(SearchDataDto data) => new()
    {
        Songs = data.Songs.ConvertAll(ToSong),
        SongTotal = data.SongTotal,
        Albums = data.Albums.ConvertAll(ToAlbum),
        AlbumTotal = data.AlbumTotal,
        Artists = data.Artists.ConvertAll(ToArtist),
        ArtistTotal = data.ArtistTotal
    };
}
