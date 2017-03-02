using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using SonicStreamer.Common.System;
using SonicStreamer.ViewModels;

namespace SonicStreamer.Pages
{
    public sealed partial class SettingsPage : Page
    {
        private readonly ApplicationSettingsViewModel _settingsVm;

        public SettingsPage()
        {
            InitializeComponent();

            if (ResourceLoader.Current.GetResource(ref _settingsVm, Constants.ViewModelApplicationSettings) == false)
                _settingsVm = new ApplicationSettingsViewModel();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackPageView(GetType().Name);

            await _settingsVm.LoadDataAsync();
        }
    }
}