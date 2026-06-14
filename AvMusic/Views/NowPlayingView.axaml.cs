using Avalonia.Controls;
using AvMusic.ViewModels;

namespace AvMusic.Views;

public partial class NowPlayingView : UserControl
{
    public NowPlayingView()
    {
        InitializeComponent();
        SeekSliderBehavior.Attach(FullSeekSlider, () => DataContext as PlayerViewModel);
    }
}
