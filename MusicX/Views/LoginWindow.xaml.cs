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
using MusicX.ViewModels.Login;
using MusicX.ViewModels.Modals;
using MusicX.Views.Login;
using MusicX.Views.Modals;
using VkNet.AudioBypassService;
using VkNet.AudioBypassService.Abstractions;
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
        private readonly IBypassAuthorizationFlow _authFlow;
        private readonly LoginViewModel _viewModel;
        private readonly CaptchaSolver _captchaSolver;
        public LoginWindow()
        {
            InitializeComponent();

            var vkService = StaticService.Container.Resolve<VkService>();
            _authFlow = (IBypassAuthorizationFlow)vkService.vkApi.AuthorizationFlow;
            _captchaSolver = (CaptchaSolver)vkService.vkApi.CaptchaSolver;
            
            _authFlow.PhoneConfirmationRequested += AuthFlowOnPhoneConfirmationRequested;
            _authFlow.PasswordRequested += AuthFlowOnPasswordRequested;
            _authFlow.TwoFactorRequested += AuthFlowOnTwoFactorRequested;
            
            _captchaSolver.CaptchaRequested += CaptchaRequested;
            
            RootFrame.Navigate(new EnterPhonePage());
            
            _viewModel = ((LoginViewModel)DataContext);
            
            _viewModel.AuthorizationCompleted += ViewModelOnAuthorizationCompleted;
            NotificationsLoop().SafeFireAndForget();
        }
        private void ViewModelOnAuthorizationCompleted(object? sender, EventArgs e)
        {
            _viewModel.AuthorizationCompleted -= ViewModelOnAuthorizationCompleted;
            Dispatcher.InvokeAsync(Close);
        }
        private async Task<string> CaptchaRequested(string url)
        {
            var viewModel = new CaptchaViewModel(url);
            
            await Dispatcher.InvokeAsync(() =>
            {
                ModalCard.Visibility = Visibility.Visible;
                ModalFrame.Navigate(new CaptchaModal
                {
                    DataContext = viewModel
                });
            });

            var result = await viewModel.ResultSource.Task.ConfigureAwait(false);
            
            await Dispatcher.InvokeAsync(() =>
            {
                ModalCard.Visibility = Visibility.Collapsed;
            });

            return result;
        }
        private Task<string> AuthFlowOnTwoFactorRequested()
        {
            var viewModel = new TwoFactorViewModel();
            Dispatcher.InvokeAsync(() =>
            {
                RootFrame.Navigate(new TwoFactorPage
                {
                    DataContext = viewModel
                });
            });

            return viewModel.TwoFactorSource.Task;
        }
        private Task<string> AuthFlowOnPasswordRequested(ValidatePhoneProfile profile)
        {
            var viewModel = new PasswordViewModel(profile);
            Dispatcher.InvokeAsync(() =>
            {
                RootFrame.Navigate(new PasswordPage
                {
                    DataContext = viewModel
                });
            });
            return viewModel.PasswordSource.Task;
        }
        private Task<string> AuthFlowOnPhoneConfirmationRequested(PhoneConfirmationEventArgs args, Func<Task<PhoneConfirmationEventArgs>> resend)
        {
            var viewModel = new PhoneValidationViewModel(args, resend);
            Dispatcher.InvokeAsync(() =>
            {
                RootFrame.Navigate(new PhoneValidationPage
                {
                    DataContext = viewModel
                });
            });
            return viewModel.CodeSource.Task;
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
        private void LoginWindow_OnUnloaded(object sender, RoutedEventArgs e)
        {
            _tokenSource.Cancel();
            _tokenSource.Dispose();
            
            _authFlow.PhoneConfirmationRequested -= AuthFlowOnPhoneConfirmationRequested;
            _authFlow.PasswordRequested -= AuthFlowOnPasswordRequested;
            _authFlow.TwoFactorRequested -= AuthFlowOnTwoFactorRequested;
            
            _captchaSolver.CaptchaRequested -= CaptchaRequested;
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
