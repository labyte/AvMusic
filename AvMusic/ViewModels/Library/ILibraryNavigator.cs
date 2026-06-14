namespace AvMusic.ViewModels.Library;

public interface ILibraryNavigator
{
    void NavigateTo(ViewModelBase page, bool pushBack = true);
}
