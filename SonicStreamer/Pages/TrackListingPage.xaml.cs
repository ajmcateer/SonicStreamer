using SonicStreamer.Common.System;
using SonicStreamer.Subsonic.Data;
using SonicStreamer.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace SonicStreamer.Pages
{
    public sealed partial class TrackListingPage : Page
    {
        public readonly TrackListingViewModel TrackListingVm;
        public readonly MainViewModel MainVm;
        public readonly PlaylistViewModel PlaylistVm;

        public TrackListingPage()
        {
            InitializeComponent();

            if (ResourceLoader.Current.GetResource(ref TrackListingVm, Constants.ViewModelTrackListing) == false)
                TrackListingVm = new TrackListingViewModel();
            if (ResourceLoader.Current.GetResource(ref MainVm, Constants.ViewModelMain) == false)
                MainVm = new MainViewModel();
            if (ResourceLoader.Current.GetResource(ref PlaylistVm, Constants.ViewModelPlaylist) == false)
                PlaylistVm = new PlaylistViewModel();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackPageView(GetType().Name);
            var param = e.Parameter as SubsonicMusicObject;
            if (param != null)
            {
                await TrackListingVm.LoadDataAsync(param);
            }
        }

        private async void PlaylistFlyout_Opening(object sender, object e)
        {
            await PlaylistVm.LoadFlyoutDataAsync();
        }

        private void PlaylistFlyout_Closed(object sender, object e)
        {
            PlaylistVm.ResetFlyoutInputs();
        }

        private void AddToPlayback_Click(object sender, RoutedEventArgs e)
        {
            PlaylistFlyout.Hide();
        }
    }
}