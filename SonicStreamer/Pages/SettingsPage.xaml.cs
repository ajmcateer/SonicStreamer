using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using SonicStreamer.Common.System;
using SonicStreamer.ViewModels;

namespace SonicStreamer.Pages
{
    public sealed partial class SettingsPage : Page
    {
        public readonly ApplicationSettingsViewModel SettingsVm;
        public readonly MainViewModel MainVm;

        public SettingsPage()
        {
            InitializeComponent();

            if (ResourceLoader.Current.GetResource(ref SettingsVm, Constants.ViewModelApplicationSettings) == false)
                SettingsVm = new ApplicationSettingsViewModel();
            if (ResourceLoader.Current.GetResource(ref MainVm, Constants.ViewModelMain) == false)
                MainVm = new MainViewModel();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackPageView(GetType().Name);

            await SettingsVm.LoadDataAsync();
        }
    }
}