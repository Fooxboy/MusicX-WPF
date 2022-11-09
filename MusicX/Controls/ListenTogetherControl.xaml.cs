using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Services;
using MusicX.ViewModels.Controls;

namespace MusicX.Controls;

public partial class ListenTogetherControl : UserControl
{
    public ListenTogetherControl()
    {
        InitializeComponent();
        DataContext = StaticService.Container.GetRequiredService<ListenTogetherControlViewModel>();
    }
}