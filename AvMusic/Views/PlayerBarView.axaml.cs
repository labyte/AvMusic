using Avalonia.Controls;
using AvMusic.ViewModels;

namespace AvMusic.Views;

public partial class PlayerBarView : UserControl
{
    public PlayerBarView()
    {
        InitializeComponent();
        SeekSliderBehavior.Attach(SeekSlider, () => DataContext as PlayerViewModel);
    }
}
