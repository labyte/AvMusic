using Avalonia.Media;

namespace AvMusic.Ui;

/// <summary>
/// Material Design 风格 24×24 路径，跨平台不依赖系统字体。
/// </summary>
public static class AppIconGeometry
{
    private static readonly Dictionary<(AppIconKind Kind, bool Filled), string> Paths = new()
    {
        [(AppIconKind.Play, false)] = "M8,5v14l11,-7z",
        [(AppIconKind.Pause, false)] = "M6,19h4V5H6v14zm8,0h4V5h-4v14z",
        [(AppIconKind.Previous, false)] = "M6,6h2v12H6zm3.5,6l8.5,6V6z",
        [(AppIconKind.Next, false)] = "M16,6h2v12H16zm-3.5,6l-8.5,6V6z",
        [(AppIconKind.Heart, false)] = "M16.5,3c-1.74,0 -3.41,0.81 -4.5,2.09C10.91,3.81 9.24,3 7.5,3 4.42,3 2,5.42 2,8.5c0,3.78 3.4,6.86 8.55,11.54L12,21.35l1.45,-1.32C18.6,15.36 22,12.28 22,8.5 22,5.42 19.58,3 16.5,3zm-4.4,15.55l-0.1,0.1 -0.1,-0.1C7.14,14.24 4,11.39 4,8.5 4,6.5 5.5,5 7.5,5c1.54,0 3.04,0.99 3.57,2.36h1.87C13.46,5.99 14.96,5 16.5,5c2,0 3.5,1.5 3.5,3.5 0,2.89 -3.14,5.74 -7.9,10.05z",
        [(AppIconKind.Heart, true)] = "M12,21.35l-1.45,-1.32C5.4,15.36 2,12.28 2,8.5 2,5.42 4.42,3 7.5,3c1.74,0 3.41,0.81 4.5,2.09C13.09,3.81 14.76,3 16.5,3 19.58,3 22,5.42 22,8.5c0,3.78 -3.4,6.86 -8.55,11.54L12,21.35z",
        [(AppIconKind.Shuffle, false)] = "M10.59,9.17L5.41,4 4,5.41l5.17,5.17 1.42,-1.41zM14.5,4l2.04,2.04L4,18.59 5.41,20 17.96,7.46 20,9.5V4h-5.5zm0.33,9.41l-1.41,1.41 3.13,3.13L14.5,20H20v-5.5l-2.04,2.04 -3.13,-3.13z",
        [(AppIconKind.RepeatAll, false)] = "M7,7h10v3l4,-4 -4,-4v3H5v6h2V7zm10,10H7v-3l-4,4 4,4v-3h12v-6h-2v4z",
        [(AppIconKind.RepeatOne, false)] = "M7,7h10v3l4,-4 -4,-4v3H5v6h2V7zm10,10H7v-3l-4,4 4,4v-3h4v-2h-4v-4h6v6h2v-8h-8v2h6v4z M11,15h2v4h-2z",
        [(AppIconKind.Volume, false)] = "M3,9v6h4l5,5V4L7,9H3zm13.5,3c0,-1.77 -1.02,-3.29 -2.5,-4.03v8.05c1.48,-0.73 2.5,-2.25 2.5,-4.02zM14,3.23v2.06c2.89,0.86 5,3.54 5,6.71s-2.11,5.85 -5,6.71v2.06c4.01,-0.91 7,-4.49 7,-8.77s-2.99,-7.86 -7,-8.77z",
        [(AppIconKind.VolumeMute, false)] = "M16.5,12c0,-1.77 -1.02,-3.29 -2.5,-4.03v2.21l2.45,2.45c0.03,-0.2 0.05,-0.41 0.05,-0.63zm2.5,0c0,0.94 -0.2,1.82 -0.54,2.64l1.51,1.51C20.63,14.91 21,13.5 21,12c0,-4.28 -2.99,-7.86 -7,-8.77v2.06c2.89,0.86 5,3.54 5,6.71zM4.27,3L3,4.27 7.73,9H3v6h4l5,5v-6.73l4.25,4.25c-0.67,0.52 -1.42,0.93 -2.25,1.18v2.06c1.38,-0.31 2.63,-0.95 3.69,-1.81L19.73,21 21,19.73l-9,-9L4.27,3zM12,4L9.91,6.09 12,8.18V4z",
        [(AppIconKind.Search, false)] = "M15.5,14h-0.79l-0.28,-0.27A6.471,6.471 0,0 0 16,9.5 6.5,6.5 0,1 0 9.5,16c1.61,0 3.09,-0.59 4.23,-1.57l0.27,0.28v0.79l5,4.99L20.49,19l-4.99,-5zm-6,0A4.5,4.5 0,1 1 14,9.5 4.5,4.5 0,0 1 9.5,14z",
        [(AppIconKind.MusicNote, false)] = "M12,3v10.55c-0.59,-0.34 -1.27,-0.55 -2,-0.55 -2.21,0 -4,1.79 -4,4s1.79,4 4,4 4,-1.79 4,-4V7h4V3h-6z",
        [(AppIconKind.Album, false)] = "M12,2C6.48,2 2,6.48 2,12s4.48,10 10,10 10,-4.48 10,-10S17.52,2 12,2zm0,14.5c-2.49,0 -4.5,-2.01 -4.5,-4.5s2.01,-4.5 4.5,-4.5 4.5,2.01 4.5,4.5 -2.01,4.5 -4.5,4.5zm0,-5.5c-0.55,0 -1,0.45 -1,1s0.45,1 1,1 1,-0.45 1,-1 -0.45,-1 -1,-1z",
        [(AppIconKind.Artist, false)] = "M12,12c2.21,0 4,-1.79 4,-4s-1.79,-4 -4,-4 -4,1.79 -4,4 1.79,4 4,4zm0,2c-2.67,0 -8,1.34 -8,4v2h16v-2c0,-2.66 -5.33,-4 -8,-4z",
        [(AppIconKind.Folder, false)] = "M10,4H4c-1.1,0 -1.99,0.9 -1.99,2L2,18c0,1.1 0.9,2 2,2h16c1.1,0 2,-0.9 2,-2V8c0,-1.1 -0.9,-2 -2,-2h-8l-2,-2z",
        [(AppIconKind.Tag, false)] = "M21.41,11.58l-9,-9C12.05,2.22 11.55,2 11,2H4c-1.1,0 -2,0.9 -2,2v7c0,0.55 0.22,1.05 0.59,1.42l9,9c0.36,0.36 0.86,0.58 1.41,0.58 0.55,0 1.05,-0.22 1.41,-0.59l7,-7c0.37,-0.36 0.59,-0.86 0.59,-1.41 0,-0.55 -0.23,-1.06 -0.59,-1.42zM5.5,7C4.67,7 4,6.33 4,5.5 4,4.67 4.67,4 5.5,4 6.33,4 7,4.67 7,5.5 7,6.33 6.33,7 5.5,7z",
        [(AppIconKind.Library, false)] = "M4,6H2v14c0,1.1 0.9,2 2,2h14v-2H4V6zm16,-4H8c-1.1,0 -2,0.9 -2,2v12c0,1.1 0.9,2 2,2h12c1.1,0 2,-0.9 2,-2V4c0,-1.1 -0.9,-2 -2,-2zm-1,9H9V9h10v2zm-4,4H9v-2h6v2zm4,-8H9V5h10v2z",
        [(AppIconKind.ChevronLeft, false)] = "M15.41,7.41L14,6l-6,6 6,6 1.41,-1.41L10.83,12z",
        [(AppIconKind.Close, false)] = "M19,6.41L17.59,5 12,10.59 6.41,5 5,6.41 10.59,12 5,17.59 6.41,19 12,13.41 17.59,19 19,17.59 13.41,12z",
        [(AppIconKind.Expand, false)] = "M7,14H5v5h5v-2H7v-3zm-2,-4h2V7h3V5H5v5zm12,7h-3v2h5v-5h-2v3zM14,5v2h3v3h2V5h-5z",
        [(AppIconKind.SignOut, false)] = "M17,7l-1.41,1.41L18.17,11H8v2h10.17l-2.58,2.58L17,17l5,-5 -5,-5zM4,5h8V3H4c-1.1,0 -2,0.9 -2,2v14c0,1.1 0.9,2 2,2h8v-2H4V5z",
        [(AppIconKind.PlayCircle, false)] = "M12,2C6.48,2 2,6.48 2,12s4.48,10 10,10 10,-4.48 10,-10S17.52,2 12,2zm-2,14.5v-9l6,4.5 -6,4.5z",
        [(AppIconKind.QueueList, false)] = "M15,6H3v2h12V6zm0,4H3v2h12v-2zm0,4H3v2h12v-2zM7,13h2v-2H7v2zm0,4h2v-2H7v2zm0,-8h2V7H7v2zm4,4h14v-2H11v2zm0,4h14v-2H11v2zM11,7v2h14V7H11z",
        [(AppIconKind.PlayOrder, false)] = "M3,14h4v-4H3v4zm0,5h4v-4H3v4zM3,9h4V5H3v4zm5,5h13v-4H8v4zm0,5h13v-4H8v4zM8,5v4h13V5H8z",
    };

    public static StreamGeometry? GetGeometry(AppIconKind kind, bool filled = false)
    {
        if (kind == AppIconKind.Heart && filled)
        {
            return Parse(Paths[(AppIconKind.Heart, true)]);
        }

        if (Paths.TryGetValue((kind, false), out var path))
        {
            return Parse(path);
        }

        return null;
    }

    private static StreamGeometry Parse(string pathData)
    {
        var geometry = StreamGeometry.Parse(pathData);
        geometry.Transform = new ScaleTransform(1, 1);
        return geometry;
    }
}
