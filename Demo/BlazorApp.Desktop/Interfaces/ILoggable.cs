using Microsoft.AspNetCore.Components;

namespace BlazorApp.Desktop.Interfaces;

public interface ILoggable : IComponent
{
    public void Log();
}
