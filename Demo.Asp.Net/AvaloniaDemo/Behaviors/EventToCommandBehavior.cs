using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Collections.Generic;
using System.Windows.Input;

namespace AvaloniaDemo.Behaviors
{

    public class EventToCommandBehavior : AvaloniaObject
{
    public static readonly AttachedProperty<string> EventNameProperty =
        AvaloniaProperty.RegisterAttached<EventToCommandBehavior, Interactive, string>(
            "EventName", default!, false, BindingMode.OneTime);

    public static readonly AttachedProperty<ICommand> CommandProperty =
        AvaloniaProperty.RegisterAttached<EventToCommandBehavior, Interactive, ICommand>(
            "Command", default!, false, BindingMode.OneTime);

    public static readonly AttachedProperty<object> CommandParameterProperty =
        AvaloniaProperty.RegisterAttached<EventToCommandBehavior, Interactive, object>(
            "CommandParameter", default!, false, BindingMode.OneWay);

    // internal use to prevent multiple handlers
    private static readonly AttachedProperty<bool> IsHandlerAttachedProperty =
        AvaloniaProperty.RegisterAttached<EventToCommandBehavior, Interactive, bool>(
            "IsHandlerAttached");

    // Supported routed events
    private static readonly Dictionary<string, RoutedEvent> _routedEvents = new()
    {
        { "KeyDown", InputElement.KeyDownEvent },
        { "KeyUp", InputElement.KeyUpEvent },
        { "GotFocus", InputElement.GotFocusEvent },
        { "LostFocus", InputElement.LostFocusEvent },
        { "TextInput", InputElement.TextInputEvent },
        { "PointerPressed", InputElement.PointerPressedEvent },
        { "PointerMoved", InputElement.PointerMovedEvent },
        { "PointerReleased", InputElement.PointerReleasedEvent },
        { "Button.Click", Button.ClickEvent },
        { "MenuItem.Click", MenuItem.ClickEvent }
        // Add more as needed
    };

    static EventToCommandBehavior()
    {
        EventNameProperty.Changed.AddClassHandler<Interactive>((x, _) => TryAttachHandler(x));
        CommandProperty.Changed.AddClassHandler<Interactive>((x, _) => TryAttachHandler(x));
    }

    private static void TryAttachHandler(Interactive control)
    {
        if (control.GetValue(IsHandlerAttachedProperty))
            return;

        var eventName = control.GetValue(EventNameProperty);
        var command = control.GetValue(CommandProperty);

        if (string.IsNullOrWhiteSpace(eventName) || command == null)
            return;

        var routedEvent = GetRoutedEvent(control, eventName);
        if (routedEvent == null)
            return;

        control.AddHandler(routedEvent, Handler);
        control.SetValue(IsHandlerAttachedProperty, true);
    }

private static void Handler(object sender, RoutedEventArgs e)
{
    if (sender is not Interactive control)
        return;

    var command = control.GetValue(CommandProperty);
    var parameter = control.GetValue(CommandParameterProperty) ?? e;

    if (command?.CanExecute(parameter) == true)
        command.Execute(parameter);
}

    private static RoutedEvent? GetRoutedEvent(Interactive control, string eventName)
    {
        // First, try built-in dictionary
        if (_routedEvents.TryGetValue(eventName, out var evt))
            return evt;

        // Fallback: Try reflection on control class
        var field = control.GetType().GetField(eventName + "Event",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        return field?.GetValue(null) as RoutedEvent;
    }

    // Getters and setters
    public static void SetEventName(AvaloniaObject element, string value) =>
        element.SetValue(EventNameProperty, value);
    public static string GetEventName(AvaloniaObject element) =>
        element.GetValue(EventNameProperty);

    public static void SetCommand(AvaloniaObject element, ICommand value) =>
        element.SetValue(CommandProperty, value);
    public static ICommand GetCommand(AvaloniaObject element) =>
        element.GetValue(CommandProperty);

    public static void SetCommandParameter(AvaloniaObject element, object value) =>
        element.SetValue(CommandParameterProperty, value);
    public static object GetCommandParameter(AvaloniaObject element) =>
        element.GetValue(CommandParameterProperty);
}
}