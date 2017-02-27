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
        readonly TrackListingViewModel _trackListingVm;
        readonly MainViewModel _mainVm;
        readonly PlaylistViewModel _playlistVm;

        public TrackListingPage()
        {
            InitializeComponent();

            if (ResourceLoader.Current.GetResource(ref _trackListingVm, Constants.ViewModelTrackListing) == false)
                _trackListingVm = new TrackListingViewModel();
            if (ResourceLoader.Current.GetResource(ref _mainVm, Constants.ViewModelMain) == false)
                _mainVm = new MainViewModel();
            if (ResourceLoader.Current.GetResource(ref _playlistVm, Constants.ViewModelPlaylist) == false)
                _playlistVm = new PlaylistViewModel();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackPageView(GetType().Name);
            var param = e.Parameter as SubsonicMusicObject;
            if (param != null)
            {
                await _trackListingVm.LoadDataAsync(param);
            }
        }

        private async void PlaylistFlyout_Opening(object sender, object e)
        {
            await _playlistVm.LoadFlyoutDataAsync();
        }

        private void PlaylistFlyout_Closed(object sender, object e)
        {
            _playlistVm.ResetFlyoutInputs();
        }

        private void AddToPlayback_Click(object sender, RoutedEventArgs e)
        {
            PlaylistFlyout.Hide();
        }
    }
}