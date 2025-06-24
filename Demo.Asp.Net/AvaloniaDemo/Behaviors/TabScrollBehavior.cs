using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Xaml.Interactivity;
using System;

namespace AvaloniaDemo.Behaviors
{
    public class DraggableTabBehavior : Behavior<TabControl>
    {
        private double _startX;
        private bool _isDragging;
        private ScrollViewer? scrollViewer;

        protected override void OnAttached()
        {
            base.OnAttached();

            var tabControl = AssociatedObject;
            tabControl.IsHitTestVisible = true;
            tabControl.Background = Brushes.Transparent;
            //tabControl.PointerPressed += OnPointerPressed;
            tabControl.AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);
            tabControl.PointerMoved += OnPointerMoved;
            tabControl.PointerReleased += OnPointerReleased;

        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            var tabControl = AssociatedObject;
            scrollViewer = tabControl.FindVisualChildByName<ScrollViewer>("PART_TabScrollViewer");   //FindControl<ScrollViewer>("PART_TabScrollViewer");
        }
        private void OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            var tabControl = (TabControl)sender;

            // Check if the press is within the TabItemsPanel area
            if (e.GetCurrentPoint(tabControl).Properties.IsLeftButtonPressed)
            {
                _startX = e.GetPosition(tabControl).X;
                _isDragging = true;
            }
        }

        private void OnPointerMoved(object sender, PointerEventArgs e)
        {
            if (_isDragging)
            {
                var tabControl = (TabControl)sender;
                var currentX = e.GetPosition(tabControl).X;

                var deltaX = _startX - currentX;


                if (scrollViewer != null)
                {
                    if (scrollViewer.Extent.Width > scrollViewer.Viewport.Width)
                    {
                        var newX = scrollViewer.Offset.X + deltaX;

                        newX = Math.Max(0, Math.Min(newX, scrollViewer.Extent.Width - scrollViewer.Viewport.Width));

                        scrollViewer.Offset = new Avalonia.Vector(newX, scrollViewer.Offset.Y);
                    }
                }

                _startX = currentX;
            }
        }

        private void OnPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            _isDragging = false;
        }

        protected override void OnDetaching()
        {
            var tabControl = AssociatedObject;

            tabControl.PointerPressed -= OnPointerPressed;
            tabControl.PointerMoved -= OnPointerMoved;
            tabControl.PointerReleased -= OnPointerReleased;

            base.OnDetaching();
        }
    }
    public class TabScrollBehavior
    {

        public static void Attach(TabControl control)
        {
            //var scrollViewer = control.FindVisualChildByName<ScrollViewer>("PART_TabScrollViewer");
            //var leftButton = control.FindVisualChildByName<Button>("PART_ScrollLeftButton");
            //var rightButton = control.FindVisualChildByName<Button>("PART_ScrollRightButton");
            var plusButton = control.FindVisualChildByName<Button>("PART_PlusButton");

            plusButton.Click += (_, _) =>
            {
                (Avalonia.Application.Current as App).AddTab();
            };
        }
    }
}
