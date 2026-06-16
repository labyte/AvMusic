using AvMusic.Core.Models;
using AvMusic.Synology.Dto;
using System.Text.RegularExpressions;

namespace AvMusic.Synology.Mapping;

internal static class EntityMapper
{
    private static readonly Regex TrackNumberPrefixRegex = new(@"^\d+\s+(.+)$", RegexOptions.Compiled);
    public static Song ToSong(SongItemDto dto)
    {
        var tag = dto.Additional?.SongTag;
        var audio = dto.Additional?.SongAudio;
        return new Song
        {
            Id = dto.Id,
            Title = ResolveSongTitle(dto),
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

    /// <summary>优先使用 ID3/元数据标题；若 API title 为文件名则从路径或文件名解析歌名。</summary>
    private static string ResolveSongTitle(SongItemDto dto)
    {
        var tagTitle = dto.Additional?.SongTag?.Title;
        if (!string.IsNullOrWhiteSpace(tagTitle))
        {
            return tagTitle.Trim();
        }

        var title = dto.Title?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(title))
        {
            return ExtractTitleFromPath(dto.Path);
        }

        if (LooksLikeFileName(title))
        {
            var fromPath = ExtractTitleFromPath(dto.Path);
            if (!string.IsNullOrWhiteSpace(fromPath))
            {
                return fromPath;
            }

            return CleanFileNameTitle(title);
        }

        return title;
    }

    private static bool LooksLikeFileName(string title)
    {
        string[] extensions =
        [
            ".flac", ".mp3", ".wav", ".aac", ".m4a", ".ogg", ".wma", ".ape", ".dsf", ".dff", ".aiff", ".alac"
        ];

        foreach (var ext in extensions)
        {
            if (title.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string ExtractTitleFromPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        var fileName = Path.GetFileNameWithoutExtension(path);
        return ParseTrackTitle(fileName);
    }

    private static string CleanFileNameTitle(string title)
    {
        var withoutExt = Path.GetFileNameWithoutExtension(title);
        return ParseTrackTitle(withoutExt);
    }

    /// <summary>从「艺术家 - 歌名」或「01 歌名」等文件名格式提取元数据歌名。</summary>
    private static string ParseTrackTitle(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        var text = raw.Trim();
        var artistSeparator = text.LastIndexOf(" - ", StringComparison.Ordinal);
        if (artistSeparator >= 0 && artistSeparator + 3 < text.Length)
        {
            return text[(artistSeparator + 3)..].Trim();
        }

        var trackMatch = TrackNumberPrefixRegex.Match(text);
        if (trackMatch.Success)
        {
            return trackMatch.Groups[1].Value.Trim();
        }

        return text;
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
