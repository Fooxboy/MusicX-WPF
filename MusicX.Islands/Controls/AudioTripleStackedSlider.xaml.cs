using Microsoft.UI.Xaml.Controls;
using MusicX.Core.Models;

namespace MusicX.Islands.Controls;

public partial class AudioTripleStackedSlider : UserControl
{
    public AudioTripleStackedSlider()
    {
        InitializeComponent();
    }

    public Block Block => (Block)DataContext;
}