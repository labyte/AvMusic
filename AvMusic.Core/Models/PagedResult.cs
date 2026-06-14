namespace AvMusic.Core.Models;

public sealed class PagedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }

    public int Offset { get; init; }

    public int Total { get; init; }

    public bool HasMore => Offset + Items.Count < Total;
}
