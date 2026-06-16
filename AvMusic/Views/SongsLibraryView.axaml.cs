using Avalonia.Controls;
using AvMusic.ViewModels.Library;

namespace AvMusic.Views;

public partial class SongsLibraryView : UserControl
{
    public SongsLibraryView()
    {
        InitializeComponent();

        InfiniteScrollBehavior.Attach(
            SongsScroll,
            () => DataContext is SongsLibraryViewModel { IsLoading: false, HasMore: true },
            () =>
            {
                if (DataContext is SongsLibraryViewModel vm)
                {
                    vm.RequestLoadMore();
                }
            });

        DataContextChanged += (_, _) =>
        {
            if (DataContext is SongsLibraryViewModel vm)
            {
                vm.RequestLoadMore();
            }
        };
    }
}
