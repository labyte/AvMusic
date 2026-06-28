namespace AvMusic.Audio.LibVlc;

/// <summary>
/// 加载 libvlc 原生库（须在创建 LibVLC 实例之前调用）。
/// </summary>
public static class LibVlcBootstrap
{
    private static readonly object Gate = new();
    private static bool _initialized;

    public static void EnsureInitialized()
    {
        if (_initialized)
        {
            return;
        }

        lock (Gate)
        {
            if (_initialized)
            {
                return;
            }

            var baseDir = AppContext.BaseDirectory;
            foreach (var dir in EnumerateCandidateDirectories(baseDir))
            {
                if (File.Exists(Path.Combine(dir, NativeLibName)))
                {
                    LibVLCSharp.Shared.Core.Initialize(dir);
                    _initialized = true;
                    return;
                }
            }

            // 回退：由 LibVLCSharp 自动探测
            LibVLCSharp.Shared.Core.Initialize();
            _initialized = true;
        }
    }

    private static IEnumerable<string> EnumerateCandidateDirectories(string baseDir)
    {
        if (OperatingSystem.IsWindows())
        {
            yield return Path.Combine(baseDir, "libvlc", "win-x64");
            yield return Path.Combine(baseDir, "libvlc", "win-x86");
        }
        else if (OperatingSystem.IsMacOS())
        {
            yield return baseDir;
            yield return Path.Combine(baseDir, "libvlc", "darwin-x64");
            yield return Path.Combine(baseDir, "libvlc", "darwin-arm64");
            yield return Path.Combine(baseDir, "libvlc");
        }
        else if (OperatingSystem.IsLinux())
        {
            yield return Path.Combine(baseDir, "libvlc", "linux-x64");
            yield return Path.Combine(baseDir, "libvlc");
        }

        yield return Path.Combine(baseDir, "libvlc");
        yield return baseDir;
    }

    private static string NativeLibName =>
        OperatingSystem.IsWindows() ? "libvlc.dll" : "libvlc.dylib";
}
