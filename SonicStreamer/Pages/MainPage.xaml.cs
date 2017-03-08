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
        public readonly MainViewModel MainVm;
        public readonly LoginViewModel LoginVm;
        public readonly PlaybackViewModel PlaybackVm;

        public MainPage()
        {
            InitializeComponent();

            Loaded += MainPage_Loaded;
            Unloaded += MainPage_Unloaded;

            if (ResourceLoader.Current.GetResource(ref MainVm, Constants.ViewModelMain) == false)
                MainVm = new MainViewModel();
            if (ResourceLoader.Current.GetResource(ref LoginVm, Constants.ViewModelLogin) == false)
                LoginVm = new LoginViewModel();
            if (ResourceLoader.Current.GetResource(ref PlaybackVm, Constants.ViewModelPlayback) == false)
                PlaybackVm = new PlaybackViewModel();
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            MainVm.LoadData();
            await PlaybackVm.LoadDataAsync();
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

            if (MainVm.MainFrame.CanGoBack && !handled)
            {
                handled = true;
                MainVm.MainFrame.GoBack();
            }

            e.Handled = handled;
        }

        private void LogoutButtonClick(object sender, RoutedEventArgs e)
        {
            LoginVm.Logout();
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