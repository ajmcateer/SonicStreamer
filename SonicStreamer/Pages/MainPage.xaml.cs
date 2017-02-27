using SonicStreamer.Common.System;
using SonicStreamer.Pages;
using SonicStreamer.ViewModels;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SonicStreamer
{
    public sealed partial class MainPage : Page
    {
        readonly MainViewModel _mainVm;
        readonly LoginViewModel _loginVm;
        readonly PlaybackViewModel _playbackVm;

        public MainPage()
        {
            InitializeComponent();

            Loaded += MainPage_Loaded;
            Unloaded += MainPage_Unloaded;

            if (ResourceLoader.Current.GetResource(ref _mainVm, Constants.ViewModelMain) == false)
                _mainVm = new MainViewModel();
            if (ResourceLoader.Current.GetResource(ref _loginVm, Constants.ViewModelLogin) == false)
                _loginVm = new LoginViewModel();
            if (ResourceLoader.Current.GetResource(ref _playbackVm, Constants.ViewModelPlayback) == false)
                _playbackVm = new PlaybackViewModel();
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            _mainVm.LoadData();
            await _playbackVm.LoadDataAsync();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += Navigation_BackRequested;
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                AppViewBackButtonVisibility.Collapsed;
            SystemNavigationManager.GetForCurrentView().BackRequested -= Navigation_BackRequested;
        }

        private void Navigation_BackRequested(object sender, BackRequestedEventArgs e)
        {
            var handled = e.Handled;

            if (_mainVm.MainFrame.CanGoBack && !handled)
            {
                handled = true;
                _mainVm.MainFrame.GoBack();
            }

            e.Handled = handled;
        }

        private void LogoutButtonClick(object sender, RoutedEventArgs e)
        {
            _loginVm.Logout();
            Frame.Navigate(typeof(LoginPage), true);
        }

        private void Flyout_Opening(object sender, object e)
        {
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Xbox")
            {
                XboxVolumeSlider.Value = PlaybackService.Current.Player.Volume * 100;
            }
            else
            {
                VolumeSlider.Value = PlaybackService.Current.Player.Volume * 100;
            }
        }

        private void VolumeSlider_ValueChanged(object sender,
            Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            PlaybackService.Current.Player.Volume = e.NewValue / 100;
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Xbox")
            {
                XboxVolumeButtonIcon.Symbol = e.NewValue == 0.0 ? Symbol.Mute : Symbol.Volume;
            }
            else
            {
                VolumeButtonIcon.Symbol = e.NewValue == 0.0 ? Symbol.Mute : Symbol.Volume;
            }
        }
    }
}