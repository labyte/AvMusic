using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Media.Imaging;
using AvMusic.Core.Models;
using AvMusic.Core.Playback;
using AvMusic.Services;
using AvMusic.Synology;
using AvMusic.Synology.Services;
using ReactiveUI;

namespace AvMusic.ViewModels;

/// <summary>
/// 播放器 UI 状态（迷你条与全屏播放页共用）。
/// </summary>
public class PlayerViewModel : ViewModelBase
{
    private readonly ICoverCacheService _coverCache;
    private readonly IAudioStationService _audioStation;
    private string _title = "未播放";
    private string _subtitle = string.Empty;
    private bool _isPlaying;
    private bool _isLoading;
    private double _position;
    private double _duration;
    private double _volume = 0.85;
    private bool _isSeeking;
    private bool _isNowPlayingVisible;
    private int _rating;
    private Bitmap? _cover;
    private Song? _currentSong;
    private int _coverLoadGeneration;

    public PlayerViewModel(
        IMusicPlayerService player,
        ICoverCacheService coverCache,
        IAudioStationService audioStation)
    {
        Player = player;
        _coverCache = coverCache;
        _audioStation = audioStation;

        Player.Engine.StateChanged += OnStateChanged;
        UpdateFromState(Player.Engine.State);
        _ = SetVolumeAsync(Volume);

        var hasTrack = this.WhenAnyValue(x => x.CurrentSong).Select(s => s is not null);

        PlayPauseCommand = ReactiveCommand.CreateFromTask(PlayPauseAsync, hasTrack);
        NextCommand = ReactiveCommand.CreateFromTask(() => Player.PlayNextAsync(), hasTrack);
        PreviousCommand = ReactiveCommand.CreateFromTask(() => Player.PlayPreviousAsync(), hasTrack);
        SeekCommand = ReactiveCommand.CreateFromTask<double>(SeekAsync, hasTrack);
        ToggleFavoriteCommand = ReactiveCommand.CreateFromTask(ToggleFavoriteAsync, hasTrack);
        ShowNowPlayingCommand = ReactiveCommand.Create(() => { IsNowPlayingVisible = true; }, hasTrack);
        HideNowPlayingCommand = ReactiveCommand.Create(() => { IsNowPlayingVisible = false; });

        this.WhenAnyValue(x => x.Volume)
            .Throttle(TimeSpan.FromMilliseconds(150))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(v => _ = SetVolumeAsync(v));
    }

    public IMusicPlayerService Player { get; }

    public Song? CurrentSong
    {
        get => _currentSong;
        private set
        {
            this.RaiseAndSetIfChanged(ref _currentSong, value);
            this.RaisePropertyChanged(nameof(CanSeek));
        }
    }

    public string Title
    {
        get => _title;
        private set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    public string Subtitle
    {
        get => _subtitle;
        private set => this.RaiseAndSetIfChanged(ref _subtitle, value);
    }

    public bool IsPlaying
    {
        get => _isPlaying;
        private set => this.RaiseAndSetIfChanged(ref _isPlaying, value);
    }

    /// <summary>正在下载或加载媒体文件。</summary>
    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            this.RaiseAndSetIfChanged(ref _isLoading, value);
            this.RaisePropertyChanged(nameof(CanSeek));
        }
    }

    /// <summary>有当前曲目即可拖动进度。</summary>
    public bool CanSeek => CurrentSong is not null && !IsLoading;

    public double Position
    {
        get => _position;
        set
        {
            this.RaiseAndSetIfChanged(ref _position, value);
            this.RaisePropertyChanged(nameof(PositionText));
            this.RaisePropertyChanged(nameof(SeekMaximum));
        }
    }

    public double Duration
    {
        get => _duration;
        private set
        {
            this.RaiseAndSetIfChanged(ref _duration, value);
            this.RaisePropertyChanged(nameof(DurationText));
            this.RaisePropertyChanged(nameof(CanSeek));
            this.RaisePropertyChanged(nameof(SeekMaximum));
        }
    }

    /// <summary>进度条最大值（时长未知时随当前位置扩展，避免 Maximum=0 无法拖动）。</summary>
    public double SeekMaximum => Math.Max(Math.Max(Duration, Position), 1);

    public string PositionText => FormatTime(Position);

    public string DurationText => FormatTime(Duration);

    public double Volume
    {
        get => _volume;
        set => this.RaiseAndSetIfChanged(ref _volume, Math.Clamp(value, 0, 1));
    }

    public bool IsSeeking
    {
        get => _isSeeking;
        set => this.RaiseAndSetIfChanged(ref _isSeeking, value);
    }

    public bool IsNowPlayingVisible
    {
        get => _isNowPlayingVisible;
        set => this.RaiseAndSetIfChanged(ref _isNowPlayingVisible, value);
    }

    public int Rating
    {
        get => _rating;
        private set
        {
            this.RaiseAndSetIfChanged(ref _rating, value);
            this.RaisePropertyChanged(nameof(IsFavorite));
            this.RaisePropertyChanged(nameof(FavoriteIcon));
        }
    }

    public bool IsFavorite => Rating >= 5;

    public string FavoriteIcon => IsFavorite ? "★" : "☆";

    public Bitmap? Cover
    {
        get => _cover;
        private set => this.RaiseAndSetIfChanged(ref _cover, value);
    }

    public ReactiveCommand<Unit, Unit> PlayPauseCommand { get; }

    public ReactiveCommand<Unit, Unit> NextCommand { get; }

    public ReactiveCommand<Unit, Unit> PreviousCommand { get; }

    public ReactiveCommand<double, Unit> SeekCommand { get; }

    public ReactiveCommand<Unit, Unit> ToggleFavoriteCommand { get; }

    public ReactiveCommand<Unit, Unit> ShowNowPlayingCommand { get; }

    public ReactiveCommand<Unit, Unit> HideNowPlayingCommand { get; }

    public void NotifyPositionTextChanged() => this.RaisePropertyChanged(nameof(PositionText));

    private void OnStateChanged(object? sender, PlaybackState state)
    {
        // LibVLC 事件可能在非 UI 线程触发
        Avalonia.Threading.Dispatcher.UIThread.Post(() => UpdateFromState(state));
    }

    private void UpdateFromState(PlaybackState state)
    {
        var song = state.CurrentSong;
        var songChanged = song?.Id != CurrentSong?.Id;
        CurrentSong = song;

        Title = song?.Title ?? "未播放";
        IsLoading = state.Status == PlaybackStatus.Loading;

        if (state.Status == PlaybackStatus.Error && !string.IsNullOrWhiteSpace(state.ErrorMessage))
        {
            Subtitle = state.ErrorMessage;
        }
        else if (IsLoading)
        {
            Subtitle = "加载中…";
        }
        else
        {
            Subtitle = song is null ? string.Empty : $"{song.Artist} · {song.Album}";
        }

        IsPlaying = !IsLoading && (state.Status == PlaybackStatus.Playing || Player.Engine.IsPlaying);

        if (songChanged)
        {
            Rating = song?.Rating ?? 0;
            Cover = null;
            Position = 0;
            Duration = song?.DurationSeconds > 0 ? song.DurationSeconds : 0;
        }

        var posSec = Math.Max(0, state.Position.TotalSeconds);
        var durationSec = state.Duration.TotalSeconds;
        if (durationSec <= 0 && song?.DurationSeconds > 0)
        {
            durationSec = song.DurationSeconds;
        }

        if (durationSec < posSec)
        {
            durationSec = posSec;
        }

        Duration = Math.Max(0, durationSec);

        if (!IsSeeking && !songChanged)
        {
            Position = posSec;
        }

        if (song is not null && (songChanged || Cover is null))
        {
            _ = LoadCoverAsync(song);
        }
    }

    private async Task LoadCoverAsync(Song? song)
    {
        var generation = Interlocked.Increment(ref _coverLoadGeneration);

        if (song is null)
        {
            Cover = null;
            return;
        }

        try
        {
            var bitmap = await _coverCache.GetCoverAsync(song.Id).ConfigureAwait(true);
            if (generation != _coverLoadGeneration)
            {
                return;
            }

            Cover = bitmap;
        }
        catch (IOException)
        {
            if (generation == _coverLoadGeneration)
            {
                Cover = null;
            }
        }
        catch (Exception)
        {
            if (generation == _coverLoadGeneration)
            {
                Cover = null;
            }
        }
    }

    private async Task PlayPauseAsync()
    {
        if (IsPlaying)
        {
            await Player.PauseAsync().ConfigureAwait(false);
        }
        else
        {
            await Player.PlayAsync().ConfigureAwait(false);
        }
    }

    public async Task SeekToAsync(double seconds)
    {
        IsSeeking = false;
        var clamped = Duration > 0 ? Math.Clamp(seconds, 0, Duration) : Math.Max(0, seconds);
        Position = clamped;
        await Player.SeekAsync(TimeSpan.FromSeconds(clamped)).ConfigureAwait(true);
    }

    private Task SeekAsync(double seconds) => SeekToAsync(seconds);

    private async Task ToggleFavoriteAsync()
    {
        if (CurrentSong is null)
        {
            return;
        }

        var newRating = IsFavorite ? 0 : 5;
        try
        {
            await _audioStation.SetSongRatingAsync(CurrentSong.Id, newRating).ConfigureAwait(false);
            Rating = newRating;
        }
        catch (SynologyApiException)
        {
            // 保持原评级
        }
    }

    private async Task SetVolumeAsync(double volume) =>
        await Player.SetVolumeAsync(volume).ConfigureAwait(false);

    private static string FormatTime(double seconds)
    {
        if (double.IsNaN(seconds) || seconds < 0)
        {
            return "0:00";
        }

        var ts = TimeSpan.FromSeconds(seconds);
        return ts.TotalHours >= 1
            ? ts.ToString(@"h\:mm\:ss")
            : ts.ToString(@"m\:ss");
    }
}
