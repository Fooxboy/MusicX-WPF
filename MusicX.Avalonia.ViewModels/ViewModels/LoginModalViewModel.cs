using System.ComponentModel.DataAnnotations;
using System.Reactive;
using MusicX.Avalonia.Core.Models.Messages;
using MusicX.Avalonia.Core.Services;
using ReactiveUI;

namespace MusicX.Avalonia.ViewModels.ViewModels;

public class LoginModalViewModel : ModalViewModel
{
    private readonly AuthService _authService;
    public override string Title => "Login";

    public override bool AllowDismiss
    {
        get => false;
        set => throw new NotSupportedException();
    }
    
    [Required(ErrorMessage = "Enter phone number")]
    [Phone(ErrorMessage = "Invalid phone number")]
    public string PhoneNumber { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Enter password")]
    [StringLength(64, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
    public string Password { get; set; } = string.Empty;
    
    public string? TwoFactorCode { get; set; }
    public bool TwoFactorRequired { get; set; }

    public ReactiveCommand<Unit, Unit> SubmitPhoneCommand { get; }

    public LoginModalViewModel(AuthService authService)
    {
        _authService = authService;
        SubmitPhoneCommand = ReactiveCommand.CreateFromTask(SubmitPhoneAsync);
    }

    private async Task SubmitPhoneAsync()
    {
        try
        {
            var response = await _authService.AuthAsync(PhoneNumber, Password, TwoFactorCode);
            var token = await _authService.RefreshTokenAsync(response.AccessToken);
            MessageBus.Current.SendMessage(new LoginState(token, response.UserId));
        }
        catch (AuthException e) when (e.Data.Error == "need_validation")
        {
            MessageBus.Current.SendMessage(new NotificationMessage("Enter 2FA code below"));
            TwoFactorRequired = true;
        }
    }
}