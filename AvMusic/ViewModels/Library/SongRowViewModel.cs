using System.Reactive;
using AvMusic.Core.Models;
using ReactiveUI;

namespace AvMusic.ViewModels.Library;

/// <summary>
/// 歌曲列表行，便于 XAML 绑定播放命令。
/// </summary>
public sealed class SongRowViewModel : ViewModelBase
{
    public SongRowViewModel(Song song, Func<Song, Task> playAsync)
    {
        Song = song;
        PlayCommand = ReactiveCommand.CreateFromTask(() => playAsync(Song));
    }

    public Song Song { get; }

    public string Title => Song.Title;

    public string Subtitle => $"{Song.Artist} · {Song.Album}";

    public string DurationText => TimeSpan.FromSeconds(Song.DurationSeconds).ToString(@"m\:ss");

    public ReactiveCommand<Unit, Unit> PlayCommand { get; }
}
