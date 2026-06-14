namespace AvMusic.Synology.Services;

internal static class AudioStationApiNames
{
    public const string Song = "SYNO.AudioStation.Song";
    public const string Album = "SYNO.AudioStation.Album";
    public const string Artist = "SYNO.AudioStation.Artist";
    public const string Folder = "SYNO.AudioStation.Folder";
    public const string Genre = "SYNO.AudioStation.Genre";
    public const string Playlist = "SYNO.AudioStation.Playlist";
    public const string Search = "SYNO.AudioStation.Search";
    public const string Lyrics = "SYNO.AudioStation.Lyrics";

    public const string SongAdditional = "song_tag,song_audio,song_rating";
    public const string PlaylistSongAdditional = "songs_song_tag,songs_song_audio,songs_song_rating,sharing_info";
    public const string DefaultLibrary = "all";
}
