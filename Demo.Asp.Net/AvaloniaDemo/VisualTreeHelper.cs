using Avalonia.Controls;
using Avalonia.VisualTree;
using System.Collections.Generic;
using System.Linq;

namespace AvaloniaDemo
{
    public static class VisualTreeHelper
    {
        public static T? FindVisualChildByName<T>(this Control root, string name) where T : Control
        {
            T? Recurse(IList<Control> list)
            {
                foreach (var child in list)
                {
                    if (child.Name == name && child is T match)
                        return match;

                    var found = Recurse(child.GetVisualChildren().OfType<Control>().ToList());
                    if (found != null)
                        return found;
                }
                return null;
            }

            return Recurse(root.GetVisualChildren().OfType<Control>().ToList());
        }
    }

}



