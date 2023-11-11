using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MusicX.Helpers;
using MusicX.Services;
using VkNet.AudioBypassService.Abstractions.Categories;
using VkNet.AudioBypassService.Models.Ecosystem;
using Wpf.Ui.Input;

namespace MusicX.ViewModels.Modals;

public class LoginVerificationMethodsModalViewModel : BaseViewModel
{
    private readonly IEcosystemCategory _ecosystemCategory;
    private readonly TaskCompletionSource<EcosystemVerificationMethod> _taskSource = new();

    public bool IsLoading { get; set; } = true;
    
    public ObservableRangeCollection<EcosystemVerificationMethod> VerificationMethods { get; } = new();
    
    public ICommand SelectCommand { get; }

    public LoginVerificationMethodsModalViewModel(IEcosystemCategory ecosystemCategory, NavigationService navigationService)
    {
        _ecosystemCategory = ecosystemCategory;
        SelectCommand = new RelayCommand<EcosystemVerificationMethod>(method =>
        {
            if (method is null)
                return;

            navigationService.CloseModal();
            _taskSource.SetResult(method);
        });
    }

    public async Task<Task<EcosystemVerificationMethod>> LoadAsync(string sid)
    {
        var response = await _ecosystemCategory.GetVerificationMethodsAsync(sid);
        
        VerificationMethods.ReplaceRange(response.Methods.Where(b => !string.IsNullOrEmpty(b.Name?.ToString())).OrderBy(b => b.Priority));
        IsLoading = false;
        
        return _taskSource.Task;
    }
}