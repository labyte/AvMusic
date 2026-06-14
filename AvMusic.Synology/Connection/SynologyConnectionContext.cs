using AvMusic.Core.Models;

namespace AvMusic.Synology.Connection;

public sealed class SynologyConnectionContext : ISynologyConnectionContext
{
    public ServerProfile? Server { get; set; }
}
