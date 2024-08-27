using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MusicX.ViewModels;
using MusicX.ViewModels.Controls;

namespace MusicX.Controls.Blocks;

public partial class FollowingsUpdateInfoListBlockControl : UserControl
{
    public static readonly DependencyProperty BlockProperty = DependencyProperty.Register(
        nameof(Block), typeof(BlockViewModel), typeof(FollowingsUpdateInfoListBlockControl), new PropertyMetadata(BlockChangedCallback));

    public BlockViewModel Block
    {
        get => (BlockViewModel)GetValue(BlockProperty);
        set => SetValue(BlockProperty, value);
    }

    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel), typeof(BlockButtonViewModel), typeof(FollowingsUpdateInfoListBlockControl));

    public BlockButtonViewModel ViewModel
    {
        get => (BlockButtonViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public FollowingsUpdateInfoListBlockControl()
    {
        InitializeComponent();
    }
    
    private static void BlockChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FollowingsUpdateInfoListBlockControl control || e.NewValue is not BlockViewModel block)
            return;
        
        control.ViewModel = new BlockButtonViewModel(
            block.Buttons.First(b => b.RefDataType == "audio_followings_update_info"),
            parentBlock: block);
    }
}