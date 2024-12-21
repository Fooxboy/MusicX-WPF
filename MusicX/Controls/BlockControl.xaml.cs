using System.Windows;
using System.Windows.Controls;
using MusicX.Core.Models;
using MusicX.ViewModels;

namespace MusicX.Controls
{
    /// <summary>
    /// Логика взаимодействия для BlockControl.xaml
    /// </summary>
    public partial class BlockControl : UserControl
    {
        public BlockControl()
        {
            InitializeComponent();
        }

#if DEBUG
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            if (newContent is BlockViewModel viewModel)
                ToolTip = $"{viewModel.DataType} {viewModel.Layout?.Name ?? ""}";
            else
                ToolTip = null;
        }
#endif

        public static readonly DependencyProperty ArtistProperty = DependencyProperty.Register(
            nameof(Artist), typeof(Artist), typeof(BlockControl));

        public Artist? Artist
        {
            get => (Artist)GetValue(ArtistProperty);
            set => SetValue(ArtistProperty, value);
        }
    }
}
