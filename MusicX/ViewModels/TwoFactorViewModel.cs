using System.Windows.Input;
using Wpf.Ui.Common;
namespace MusicX.ViewModels;

public class TwoFactorViewModel
{
    private readonly LoginViewModel _viewModel;

    public TwoFactorViewModel(LoginViewModel viewModel)
    {
        _viewModel = viewModel;
        SubmitCommand = new RelayCommand(Submit);
    }
    private void Submit()
    {
        if (!string.IsNullOrEmpty(Code))
            _viewModel.TwoFactorSource?.SetResult(Code);
    }
    public ICommand SubmitCommand { get; }
    public string Code { get; set; } = string.Empty;
}
