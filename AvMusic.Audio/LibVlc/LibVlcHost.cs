using LibVLCSharp.Shared;

namespace AvMusic.Audio.LibVlc;

/// <summary>
/// LibVLC 进程级单例宿主。
/// </summary>
public sealed class LibVlcHost : IDisposable
{
    public LibVLC Instance { get; }

    public LibVlcHost()
    {
        LibVlcBootstrap.EnsureInitialized();

        // 勿传入仅适用于 Media 的选项（如 :http-ssl-insecure），否则 libvlc_new 会失败
        Instance = new LibVLC(
            "--verbose=-1",
            "--network-caching=3000");
    }

    public void Dispose()
    {
        Instance.Dispose();
    }
}
