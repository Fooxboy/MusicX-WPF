using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using MusicX.Helpers;
using MusicX.Services;
using VkNet.Extensions.Auth.Models.Auth;
using VkNet.Extensions.Auth.Models.Ecosystem;
using Wpf.Ui.Input;

namespace MusicX.ViewModels.Modals;

public class LoginVerificationMethodsModalViewModel : BaseViewModel
{
    public TaskCompletionSource<LoginWay> Submitted { get; } = new();
    
    public ObservableRangeCollection<EcosystemVerificationMethod> VerificationMethods { get; }
    
    public ICommand SelectCommand { get; }

    public LoginVerificationMethodsModalViewModel(IReadOnlyCollection<EcosystemVerificationMethod> verificationMethods, NavigationService navigationService)
    {
        VerificationMethods = new(verificationMethods);
        SelectCommand = new RelayCommand<EcosystemVerificationMethod>(method =>
        {
            if (method is null)
                return;

            navigationService.CloseModal();
            Submitted.SetResult(method.Name);
        });
    }
}