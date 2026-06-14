using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvMusic.ViewModels;

namespace AvMusic.Views;

/// <summary>
/// 为播放进度 Slider 绑定拖动与松手 Seek（兼容 Thumb 捕获指针的情况）。
/// </summary>
internal static class SeekSliderBehavior
{
    public static void Attach(Slider slider, Func<PlayerViewModel?> getViewModel)
    {
        var dragging = false;

        slider.AddHandler(
            InputElement.PointerPressedEvent,
            (_, _) =>
            {
                if (getViewModel() is not { } vm)
                {
                    return;
                }

                dragging = true;
                vm.IsSeeking = true;
            },
            RoutingStrategies.Tunnel);

        slider.ValueChanged += (_, e) =>
        {
            if (!dragging || getViewModel() is not { } vm)
            {
                return;
            }

            vm.Position = e.NewValue;
            vm.NotifyPositionTextChanged();
        };

        void EndDrag()
        {
            if (!dragging)
            {
                return;
            }

            dragging = false;
            if (getViewModel() is not { } vm)
            {
                return;
            }

            _ = vm.SeekToAsync(slider.Value);
        }

        slider.AddHandler(
            InputElement.PointerReleasedEvent,
            (_, _) => EndDrag(),
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble);

        slider.AddHandler(
            InputElement.PointerCaptureLostEvent,
            (_, _) => EndDrag(),
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
    }
}
