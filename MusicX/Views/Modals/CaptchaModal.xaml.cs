using System.Windows;
using MusicX.Controls;
using MusicX.ViewModels.Modals;

namespace MusicX.Views.Modals;

public partial class CaptchaModal : ModalPage
{
    public CaptchaModal()
    {
        InitializeComponent();
    }

    private void CaptchaModal_OnClosed(object sender, RoutedEventArgs e)
    {
        ((CaptchaModalViewModel)DataContext).CloseCommand.Execute(null);
    }
}