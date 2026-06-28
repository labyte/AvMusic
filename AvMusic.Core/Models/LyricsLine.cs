using System.Text.RegularExpressions;

namespace AvMusic.Core.Models;

/// <summary>单行歌词（LRC 格式中的一行）。</summary>
public sealed class LyricsLine
{
    public TimeSpan Timestamp { get; init; }
    public string Text { get; init; } = string.Empty;
    public bool IsEmptyLine => string.IsNullOrWhiteSpace(Text);
}

/// <summary>LRC 歌词解析/生成工具。</summary>
public static class LrcParser
{
    private static readonly Regex LineRegex = new(
        @"^\[(\d{2}):(\d{2})[\.:](\d{2,3})\]\s*(.*)$",
        RegexOptions.Compiled);

    /// <summary>将 LRC 文本解析为歌词行列表。</summary>
    public static List<LyricsLine> Parse(string lrcText)
    {
        var result = new List<LyricsLine>();
        foreach (var line in lrcText.Split('\n', StringSplitOptions.None))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
                continue;

            var match = LineRegex.Match(trimmed);
            if (!match.Success)
                continue;

            var minutes = int.Parse(match.Groups[1].Value);
            var seconds = int.Parse(match.Groups[2].Value);
            var millisStr = match.Groups[3].Value;
            var millis = millisStr.Length == 2
                ? int.Parse(millisStr) * 10
                : int.Parse(millisStr);
            var text = match.Groups[4].Value.Trim();

            result.Add(new LyricsLine
            {
                Timestamp = TimeSpan.FromMinutes(minutes) +
                            TimeSpan.FromSeconds(seconds) +
                            TimeSpan.FromMilliseconds(millis),
                Text = text
            });
        }
        return result;
    }

    /// <summary>将歌词行列表序列化为 LRC 文本。</summary>
    public static string ToLrc(IEnumerable<LyricsLine> lines)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var line in lines)
        {
            if (line.IsEmptyLine)
                continue;
            var ts = line.Timestamp;
            sb.AppendLine($"[{ts.Minutes:D2}:{ts.Seconds:D2}.{ts.Milliseconds / 10:D2}]{line.Text}");
        }
        return sb.ToString();
    }
}
