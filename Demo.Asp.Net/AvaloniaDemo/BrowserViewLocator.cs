using Avalonia.Controls.Templates;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using ReactiveUI;

namespace AvaloniaDemo
{
    public class ViewLocator : IDataTemplate
    {
        private Dictionary<object, Control> _cache = new Dictionary<object, Control>();
        public Control? Build(object? data)
        {
            if (data is null)
                return null;

            var name = data.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
            var type = Type.GetType(name);

            if (type != null)
            {
 
                if (_cache.TryGetValue(data, out var res))
                {
                    return res;
                }

                var control = (Control)Activator.CreateInstance(type)!;
                control.DataContext = data;
                _cache[control.DataContext] = control;
                return control;
            }

            return new TextBlock { Text = "Not Found: " + name };
        }

        public bool Match(object? data)
        {
            return data is ReactiveObject;
        }
    }
}
