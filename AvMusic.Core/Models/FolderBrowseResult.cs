namespace AvMusic.Core.Models;

public sealed class MusicFolder
{
    public required string Id { get; init; }

    public required string Title { get; init; }
}

public sealed class FolderBrowseResult
{
    public string? FolderId { get; init; }

    public required IReadOnlyList<MusicFolder> Subfolders { get; init; }

    public required IReadOnlyList<Song> Songs { get; init; }

    public int Total { get; init; }
}
