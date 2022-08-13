using System.Windows;
using System.Windows.Media;
namespace MusicX.Helpers;

public static class ControlExtensions
{
    public static T? FindAncestor<T>(this DependencyObject obj)
        where T : DependencyObject
    {
        var dependObj = obj;
        do
        {
            dependObj = VisualTreeHelper.GetParent(dependObj);
            if (dependObj is T parent)
                return parent;
        }
        while (dependObj != null);

        return null;
    }
}
