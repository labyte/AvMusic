using System;
using Avalonia;
using AvMusic.Audio.LibVlc;
using ReactiveUI.Avalonia;

namespace AvMusic.Desktop;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // 在 Avalonia / DI 之前加载 libvlc 原生库（需本工程引用 VideoLAN.LibVLC.Windows）
        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            LibVlcBootstrap.EnsureInitialized();
        }

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .UseReactiveUI()
            .LogToTrace();
}
