namespace AvMusic.ViewModels.Library;

public interface ILibraryPage
{
    string Title { get; }

    bool IsLoading { get; }

    string? ErrorMessage { get; }

    Task LoadAsync(CancellationToken cancellationToken = default);
}
