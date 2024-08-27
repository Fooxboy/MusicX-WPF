using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MusicX.Core.Models;
using MusicX.ViewModels;
using MusicX.ViewModels.Controls;
using Block = MusicX.Core.Models.Block;
using Button = MusicX.Core.Models.Button;

namespace MusicX.Controls;
/// <summary>
/// Interaction logic for ActionButtonControl.xaml
/// </summary>
public partial class ActionButtonControl : UserControl
{
    public ActionButtonControl()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty ActionProperty = DependencyProperty.Register(nameof(Action), typeof(Button), typeof(ActionButtonControl), new(null, OnChanged));

    public Button Action
    {
        get => (Button)GetValue(ActionProperty);
        set => SetValue(ActionProperty, value);
    }
    public static readonly DependencyProperty ArtistProperty = DependencyProperty.Register(nameof(Artist), typeof(Artist), typeof(ActionButtonControl), new(null, OnChanged));

    public Artist? Artist
    {
        get => (Artist?)GetValue(ArtistProperty);
        set => SetValue(ArtistProperty, value);
    }
    public static readonly DependencyProperty ParentBlockProperty = DependencyProperty.Register(nameof(ParentBlock), typeof(BlockViewModel), typeof(ActionButtonControl), new(null, OnChanged));

    public BlockViewModel? ParentBlock
    {
        get => (BlockViewModel?)GetValue(ParentBlockProperty);
        set => SetValue(ParentBlockProperty, value);
    }

    private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ActionButtonControl control && control.Action is not null)
            control.RecreateViewModel();
    }

    private void RecreateViewModel()
    {
        DataContext = new BlockButtonViewModel(Action, Artist, ParentBlock);
    }
}
