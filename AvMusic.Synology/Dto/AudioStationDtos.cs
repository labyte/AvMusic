using System.Text.Json.Serialization;

namespace AvMusic.Synology.Dto;

#region 歌曲

public sealed class SongListDataDto
{
    [JsonPropertyName("offset")]
    public int Offset { get; init; }

    [JsonPropertyName("total")]
    public int Total { get; init; }

    [JsonPropertyName("songs")]
    public List<SongItemDto> Songs { get; init; } = [];
}

public sealed class SongItemDto
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("path")]
    public string? Path { get; init; }

    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("additional")]
    public SongAdditionalDto? Additional { get; init; }
}

public sealed class SongAdditionalDto
{
    [JsonPropertyName("song_tag")]
    public SongTagDto? SongTag { get; init; }

    [JsonPropertyName("song_audio")]
    public SongAudioDto? SongAudio { get; init; }

    [JsonPropertyName("song_rating")]
    public RatingDto? SongRating { get; init; }
}

public sealed class SongTagDto
{
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    [JsonPropertyName("artist")]
    public string? Artist { get; init; }

    [JsonPropertyName("album")]
    public string? Album { get; init; }

    [JsonPropertyName("album_artist")]
    public string? AlbumArtist { get; init; }

    [JsonPropertyName("genre")]
    public string? Genre { get; init; }

    [JsonPropertyName("year")]
    public int Year { get; init; }

    [JsonPropertyName("track")]
    public int Track { get; init; }

    [JsonPropertyName("disc")]
    public int Disc { get; init; }
}

public sealed class SongAudioDto
{
    [JsonPropertyName("duration")]
    public int Duration { get; init; }

    [JsonPropertyName("bitrate")]
    public int Bitrate { get; init; }

    [JsonPropertyName("codec")]
    public string? Codec { get; init; }

    [JsonPropertyName("container")]
    public string? Container { get; init; }

    [JsonPropertyName("filesize")]
    public long Filesize { get; init; }

    [JsonPropertyName("frequency")]
    public int Frequency { get; init; }
}

public sealed class RatingDto
{
    [JsonPropertyName("rating")]
    public double Rating { get; init; }
}

#endregion

#region 专辑 / 艺术家 / 类型

public sealed class AlbumListDataDto
{
    [JsonPropertyName("offset")]
    public int Offset { get; init; }

    [JsonPropertyName("total")]
    public int Total { get; init; }

    [JsonPropertyName("albums")]
    public List<AlbumItemDto> Albums { get; init; } = [];
}

public sealed class AlbumItemDto
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("album_artist")]
    public string? AlbumArtist { get; init; }

    [JsonPropertyName("artist")]
    public string? Artist { get; init; }

    [JsonPropertyName("display_artist")]
    public string? DisplayArtist { get; init; }

    [JsonPropertyName("year")]
    public int Year { get; init; }

    [JsonPropertyName("additional")]
    public AlbumAdditionalDto? Additional { get; init; }
}

public sealed class AlbumAdditionalDto
{
    [JsonPropertyName("avg_rating")]
    public RatingDto? AvgRating { get; init; }
}

public sealed class ArtistListDataDto
{
    [JsonPropertyName("offset")]
    public int Offset { get; init; }

    [JsonPropertyName("total")]
    public int Total { get; init; }

    [JsonPropertyName("artists")]
    public List<ArtistItemDto> Artists { get; init; } = [];
}

public sealed class ArtistItemDto
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("additional")]
    public ArtistAdditionalDto? Additional { get; init; }
}

public sealed class ArtistAdditionalDto
{
    [JsonPropertyName("avg_rating")]
    public RatingDto? AvgRating { get; init; }
}

public sealed class GenreListDataDto
{
    [JsonPropertyName("genres")]
    public List<GenreItemDto> Genres { get; init; } = [];
}

public sealed class GenreItemDto
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("additional")]
    public GenreAdditionalDto? Additional { get; init; }
}

public sealed class GenreAdditionalDto
{
    [JsonPropertyName("avg_rating")]
    public RatingDto? AvgRating { get; init; }
}

#endregion

#region 播放列表

public sealed class PlaylistListDataDto
{
    [JsonPropertyName("offset")]
    public int Offset { get; init; }

    [JsonPropertyName("total")]
    public int Total { get; init; }

    [JsonPropertyName("playlists")]
    public List<PlaylistItemDto> Playlists { get; init; } = [];
}

public sealed class PlaylistItemDto
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("library")]
    public string? Library { get; init; }

    [JsonPropertyName("type")]
    public string? Type { get; init; }
}

public sealed class PlaylistDetailDataDto
{
    [JsonPropertyName("playlists")]
    public List<PlaylistDetailItemDto> Playlists { get; init; } = [];
}

public sealed class PlaylistDetailItemDto
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("additional")]
    public PlaylistAdditionalDto? Additional { get; init; }
}

public sealed class PlaylistAdditionalDto
{
    [JsonPropertyName("songs")]
    public List<SongItemDto> Songs { get; init; } = [];
}

#endregion

#region 文件夹

public sealed class FolderListDataDto
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("offset")]
    public int Offset { get; init; }

    [JsonPropertyName("total")]
    public int Total { get; init; }

    [JsonPropertyName("folder_total")]
    public int FolderTotal { get; init; }

    [JsonPropertyName("items")]
    public List<SongItemDto> Items { get; init; } = [];
}

#endregion

#region 搜索

public sealed class SearchDataDto
{
    [JsonPropertyName("songTotal")]
    public int SongTotal { get; init; }

    [JsonPropertyName("songs")]
    public List<SongItemDto> Songs { get; init; } = [];

    [JsonPropertyName("albumTotal")]
    public int AlbumTotal { get; init; }

    [JsonPropertyName("albums")]
    public List<AlbumItemDto> Albums { get; init; } = [];

    [JsonPropertyName("artistTotal")]
    public int ArtistTotal { get; init; }

    [JsonPropertyName("artists")]
    public List<ArtistItemDto> Artists { get; init; } = [];
}

#endregion

#region 歌词

public sealed class LyricsDataDto
{
    [JsonPropertyName("lyrics")]
    public string? Lyrics { get; init; }
}

#endregion
