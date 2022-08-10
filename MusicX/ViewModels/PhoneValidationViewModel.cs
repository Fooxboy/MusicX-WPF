using System;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices;
using AsyncAwaitBestPractices.MVVM;
using VkNet.AudioBypassService;
using VkNet.AudioBypassService.Models.Vk;
using Wpf.Ui.Common;
namespace MusicX.ViewModels;

public class PhoneValidationViewModel : BaseViewModel
{
    public PhoneConfirmationEventArgs Args { get; set; }
    
    public string Subtitle { get; set; } = string.Empty;
    
    public string Code { get; set; }
    public ICommand SubmitCommand { get; }
    public ICommand ResendCommand { get; }
    public bool ResendAvailable => Args.ValidationResend is not null;
    public bool CanResend => Args.DelayUntilResend < DateTimeOffset.Now;
    public string? TimeLeft { get; set; }

    private readonly Func<Task<PhoneConfirmationEventArgs>> _resend;
    private readonly LoginViewModel _viewModel;
    public PhoneValidationViewModel(PhoneConfirmationEventArgs args, Func<Task<PhoneConfirmationEventArgs>> resend, LoginViewModel viewModel)
    {
        Args = args;
        _resend = resend;
        _viewModel = viewModel;

        SubmitCommand = new RelayCommand(Submit);
        ResendCommand = new AsyncCommand(Resend, _ => Args.ValidationResend is not null);
        
        Update();
    }
    private async Task Resend()
    {
        Args = await _resend();
        Update();
    }
    private void Update()
    {
        Subtitle = Args.ValidationType switch
        {
            PhoneConfirmationType.CallReset => $"Сейчас тебе поступит звонок-сброс",
            PhoneConfirmationType.Sms => $"Сейчас тебе придет смс с кодом",
            _ => throw new ArgumentOutOfRangeException()
        };

        if (Args.PhoneTemplate is not null)
            Subtitle += $"\nС номера:\n{Args.PhoneTemplate}";

        Subtitle += $"\nНужно ввести {Args.CodeLength} цифры";
        
        Changed(nameof(ResendAvailable));
        Changed(nameof(CanResend));
        TimeLeftTimer().SafeFireAndForget();

        Code = string.Empty;
    }

    private async Task TimeLeftTimer()
    {
        while (true)
        {
            var timeLeft = Args.DelayUntilResend - DateTimeOffset.Now;
            if (timeLeft?.TotalMilliseconds < 0)
                break;

            TimeLeft = timeLeft?.ToString("mm\\:ss");
            await Task.Delay(1000);
        }
        
        TimeLeft = null;
        Changed(nameof(CanResend));
    }
    private void Submit()
    {
        if (Code.Length != Args.CodeLength || !uint.TryParse(Code, out _))
            return;
        
        _viewModel.CodeSource.SetResult(Code);
    }
}
