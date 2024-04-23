using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Animation;

namespace MusicX.Controls;

public class LoadingBorder : Border
{
    public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register(
        nameof(IsLoading), typeof(bool), typeof(LoadingBorder), new PropertyMetadata(true));

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        if (IsLoading && TryFindResource("LoadingBorderLoaderAnimation") is Storyboard storyboard)
        {
            if (((INameScope)Style).FindName("LoadingBorderLoaderAnimation") is null)
                Style.RegisterName("LoadingBorderLoaderAnimation", new BeginStoryboard
                {
                    Storyboard = storyboard,
                    Name = "LoadingBorderLoaderAnimation"
                });
            BeginStoryboard(storyboard);
        }
    }
}