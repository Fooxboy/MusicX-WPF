using System.Windows;
using System.Windows.Controls;
using MusicX.Core.Models;

namespace MusicX.Controls.Blocks;

public partial class FollowingsUpdateInfoListBlockControl : UserControl
{
    public static readonly DependencyProperty BlockProperty = DependencyProperty.Register(
        nameof(Block), typeof(Block), typeof(FollowingsUpdateInfoListBlockControl));

    public Block Block
    {
        get => (Block)GetValue(BlockProperty);
        set => SetValue(BlockProperty, value);
    }
    
    public FollowingsUpdateInfoListBlockControl()
    {
        InitializeComponent();
    }
}