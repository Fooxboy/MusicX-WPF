using MusicX.Core.Services;
using MusicX.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using AsyncAwaitBestPractices;
using DryIoc;
using MusicX.ViewModels;
using MusicX.Views.Login;
using VkNet.AudioBypassService;
using VkNet.AudioBypassService.Models.Vk;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace MusicX.Views
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : UiWindow
    {
        private readonly CancellationTokenSource _tokenSource = new();
        private readonly IBoomAuthorizationFlow _authFlow;
        private readonly LoginViewModel _viewModel;
        public LoginWindow()
        {
            InitializeComponent();

            _authFlow = (IBoomAuthorizationFlow)StaticService.Container.Resolve<VkService>().vkApi.AuthorizationFlow;
            
            _authFlow.StateChanged += AuthFlowOnStateChanged;
            _authFlow.PhoneConfirmationRequested += AuthFlowOnPhoneConfirmationRequested;
            _authFlow.PasswordRequested += AuthFlowOnPasswordRequested;
            _authFlow.TwoFactorRequested += AuthFlowOnTwoFactorRequested;
            
            RootFrame.Navigate(new EnterPhonePage());
            
            _viewModel = ((LoginViewModel)DataContext);
            NotificationsLoop().SafeFireAndForget();
        }
        private Task<string> AuthFlowOnTwoFactorRequested()
        {
            RootFrame.Navigate(new TwoFactorPage
            {
                DataContext = new TwoFactorViewModel(_viewModel)
            });

            _viewModel.TwoFactorSource = new();
            return _viewModel.TwoFactorSource.Task;
        }
        private Task<string> AuthFlowOnPasswordRequested(ValidatePhoneProfile profile)
        {
            RootFrame.Navigate(new PasswordPage
            {
                DataContext = new PasswordViewModel(profile, _viewModel)
            });
            return _viewModel.PasswordSource.Task;
        }
        private Task<string> AuthFlowOnPhoneConfirmationRequested(PhoneConfirmationEventArgs args, Func<Task<PhoneConfirmationEventArgs>> resend)
        {
            RootFrame.Navigate(new PhoneValidationPage
            {
                DataContext = new PhoneValidationViewModel(args, resend, _viewModel)
            });
            return _viewModel.CodeSource.Task;
        }

        private async Task NotificationsLoop()
        {
            try
            {
                await foreach (var (header, content) in _viewModel.Notifications.ReadAllAsync(_tokenSource.Token))
                {
                    await Dispatcher.InvokeAsync(() => RootSnackbar.Show(header, content));
                }
            }
            catch (OperationCanceledException)
            {
                // i dont care
            }
        }
        
        private void AuthFlowOnStateChanged(object? sender, BoomAuthState e)
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                switch (e)
                {
                    case BoomAuthState.None:
                        RootFrame.Navigate(new EnterPhonePage());
                        break;
                    case BoomAuthState.GetSilentToken:
                    case BoomAuthState.CheckSilentToken:
                    case BoomAuthState.GetBearerToken:
                    case BoomAuthState.GetVkToken:
                    case BoomAuthState.GetPrivateKey:
                    case BoomAuthState.GetAnonymousToken:
                        RootFrame.Navigate(new LoadingPage());
                        break;
                    case BoomAuthState.PhoneValidation:
                        break;
                    case BoomAuthState.PhoneConfirmation:
                        break;
                    case BoomAuthState.Authorized:
                        RootFrame.Navigate(new TextBlock
                        {
                            Text = "Это окно можно закрыть",
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        });
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(e), e, null);
                }
            });
        }
        private void LoginWindow_OnUnloaded(object sender, RoutedEventArgs e)
        {
            _tokenSource.Cancel();
            _tokenSource.Dispose();
        }
        
        private void RootFrame_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateFrameDataContext();
        }
        private void RootFrame_LoadCompleted(object sender, NavigationEventArgs e)
        {
            UpdateFrameDataContext();
        }
        private void UpdateFrameDataContext()
        {
            if (RootFrame.Content is not FrameworkElement {DataContext: null} content)
                return;
            content.DataContext = RootFrame.DataContext;
        }
    }
}
