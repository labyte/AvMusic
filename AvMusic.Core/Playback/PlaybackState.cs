using AvMusic.Core.Models;

namespace AvMusic.Core.Playback;

public sealed record PlaybackState
{
    public PlaybackStatus Status { get; init; } = PlaybackStatus.Idle;

    public Song? CurrentSong { get; init; }

    public TimeSpan Position { get; init; }

    public TimeSpan Duration { get; init; }

    public string? ErrorMessage { get; init; }
}
