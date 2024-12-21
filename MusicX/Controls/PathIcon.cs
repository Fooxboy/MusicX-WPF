using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace MusicX.Controls;

public class PathIcon : IconElement
{
    public static readonly DependencyProperty PathProperty = DependencyProperty.Register(
        nameof(Path), typeof(Path), typeof(PathIcon), new PropertyMetadata(default(Path)));

    public Path Path
    {
        get => (Path)GetValue(PathProperty);
        set => SetValue(PathProperty, value);
    }

    protected override UIElement InitializeChildren()
    {
        Path.Stretch = Stretch.UniformToFill;
        Path.SetBinding(Shape.FillProperty, new Binding("Foreground") { Source = this });
        return Path;
    }
}