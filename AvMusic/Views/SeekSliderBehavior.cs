using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvMusic.ViewModels;

namespace AvMusic.Views;

/// <summary>
/// 为播放进度 Slider 绑定拖动与松手 Seek，并在拖动期间保持交互态样式。
/// </summary>
internal static class SeekSliderBehavior
{
    private const string ActiveClass = "seek-slider-active";

    public static void Attach(Slider slider, Func<PlayerViewModel?> getViewModel, Control? activeHost = null)
    {
        var host = activeHost ?? slider;
        var dragging = false;

        slider.Focusable = false;

        slider.PointerExited += (_, _) =>
        {
            if (dragging)
            {
                return;
            }

            ClearFocus(slider);
        };

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
                host.Classes.Add(ActiveClass);
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
            host.Classes.Remove(ActiveClass);
            ClearFocus(slider);

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

    private static void ClearFocus(Slider slider)
    {
        if (!slider.IsKeyboardFocusWithin)
        {
            return;
        }

        var topLevel = TopLevel.GetTopLevel(slider);
        topLevel?.FocusManager?.ClearFocus();
    }
}
