using SonicStreamer.Common.System;
using SonicStreamer.ViewModels;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace SonicStreamer.Pages
{
    public sealed partial class LoginPage : Page
    {
        private enum LastFocusedType
        {
            Unknown = 0,
            ServerTextBox = 1,
            UserTextBox = 2,
            PasswordTextBox = 3,
            ServerTypeComboBox = 4
        }

        private readonly LoginViewModel _loginVm;
        private readonly PlaybackViewModel _playbackVm;
        private LastFocusedType _lastFocusElement;

        public LoginPage()
        {
            InitializeComponent();

            if (ResourceLoader.Current.GetResource(ref _loginVm, Constants.ViewModelLogin) == false)
                _loginVm = new LoginViewModel();
            if (ResourceLoader.Current.GetResource(ref _playbackVm, Constants.ViewModelPlayback) == false)
                _playbackVm = new PlaybackViewModel();
            _lastFocusElement = LastFocusedType.Unknown;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            ConnectButton.Focus(FocusState.Programmatic);

            if (e.Parameter == null || e.Parameter as string == string.Empty)
            {
                _loginVm.RestoreData();

                if (!string.IsNullOrEmpty(_loginVm.ServerUrl) && !string.IsNullOrEmpty(_loginVm.User) &&
                    !string.IsNullOrEmpty(_loginVm.Password))
                {
                    SetInputMode(false);
                    ProgressRing.Visibility = Visibility.Visible;
                    ProgressRing.IsActive = true;
                    if (await _loginVm.LoginAsync())
                    {
                        Frame.Navigate(typeof(MainPage));
                    }
                    ProgressRing.Visibility = Visibility.Collapsed;
                    ProgressRing.IsActive = false;
                }
            }

            SetInputMode(true);
        }

        private void SetInputMode(bool value)
        {
            ServerTextBox.IsEnabled = value;
            UserTextBox.IsEnabled = value;
            PasswordTextBox.IsEnabled = value;
            ConnectButton.IsEnabled = value;
            ServerTypeComboBox.IsEnabled = value;
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_loginVm.ServerUrl) && !string.IsNullOrEmpty(_loginVm.User) &&
                !string.IsNullOrEmpty(_loginVm.Password))
            {
                ProgressRing.IsActive = true;
                var originPassword = _loginVm.Password;
                _loginVm.Message = string.Empty;
                ProgressRing.Visibility = Visibility.Visible;
                ProgressRing.IsActive = true;
                SetInputMode(false);
                if (await _loginVm.ConnectAsync())
                {
                    ProgressRing.Visibility = Visibility.Collapsed;
                    ProgressRing.IsActive = false;
                    Frame.Navigate(typeof(MainPage));
                }
                else
                {
                    ProgressRing.Visibility = Visibility.Collapsed;
                    ProgressRing.IsActive = false;
                    SetInputMode(true);
                    _loginVm.Password = originPassword;
                }
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBlock = sender as Control;
            if (textBlock != null)
            {
                LastFocusedType result;
                var parseResult = Enum.TryParse(textBlock.Name, out result);
                _lastFocusElement = parseResult ? result : LastFocusedType.Unknown;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
#if WINDOWS_PHONE_APP
            LoginGrid.VerticalAlignment = VerticalAlignment.Center;
#endif
        }

        private void PasswordTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                ConnectButton.Focus(FocusState.Programmatic);
                ConnectButton_Click(sender, e);
            }
        }

        private void NextAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            switch (_lastFocusElement)
            {
                case LastFocusedType.ServerTextBox:
                    UserTextBox.Focus(FocusState.Programmatic);
                    break;
                case LastFocusedType.UserTextBox:
                    PasswordTextBox.Focus(FocusState.Programmatic);
                    break;
                case LastFocusedType.PasswordTextBox:
                    ConnectButton.Focus(FocusState.Programmatic);
                    break;
                case LastFocusedType.ServerTypeComboBox:
                    ServerTypeComboBox.Focus(FocusState.Programmatic);
                    break;
                case LastFocusedType.Unknown:
                default:
                    break;
            }
        }
    }
}