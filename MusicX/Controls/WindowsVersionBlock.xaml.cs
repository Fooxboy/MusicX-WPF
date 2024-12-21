using System;
using System.Windows;
using System.Windows.Controls;

namespace MusicX.Controls;

public partial class WindowsVersionBlock : UserControl
{
    public static readonly DependencyProperty VersionProperty = DependencyProperty.Register(
        nameof(Version), typeof(Version), typeof(WindowsVersionBlock), new PropertyMetadata(default(Version)));

    public Version Version
    {
        get => (Version)GetValue(VersionProperty);
        set => SetValue(VersionProperty, value);
    }
    
    public WindowsVersionBlock()
    {
        InitializeComponent();
    }
}