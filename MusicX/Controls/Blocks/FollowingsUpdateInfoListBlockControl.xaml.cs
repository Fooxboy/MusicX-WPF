using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MusicX.Core.Models;
using MusicX.ViewModels.Controls;

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
        DataContextChanged += FollowingsUpdateInfoListBlockControl_DataContextChanged;
    }

    private void FollowingsUpdateInfoListBlockControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is not Block block)
            return;

        Block = block;
        ViewModel = new BlockButtonViewModel(
                                Block.Actions.First(b => b.RefDataType == "audio_followings_update_info"),
                                parentBlock: Block);
    }
}