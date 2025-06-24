using Avalonia.Controls;
using Avalonia.Input;
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaDemo.Behaviors
{
public class ScrollViewerBehavior : Behavior<ScrollViewer>
{
    public static readonly StyledProperty<bool> IsScrollEnabledProperty = AvaloniaProperty.Register<ScrollViewerBehavior, bool>(nameof(IsScrollEnabled));

    public bool IsScrollEnabled
    {
        get => GetValue(IsScrollEnabledProperty);
        set => SetValue(IsScrollEnabledProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject != null)
        {
            AssociatedObject.PointerWheelChanged += OnPointerWheelChanged;
        }
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject != null)
        {
            AssociatedObject.PointerWheelChanged -= OnPointerWheelChanged;
        }

        base.OnDetaching();
    }

    private void OnPointerWheelChanged(object sender, PointerWheelEventArgs e)
    {
        if (IsScrollEnabled)
        {
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null)
            {
                var delta = e.Delta.Y;
                scrollViewer.Offset = new Avalonia.Vector(scrollViewer.Offset.X, scrollViewer.Offset.Y - delta);
            }
        }
    }
}
}
