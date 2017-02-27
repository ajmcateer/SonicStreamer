using SonicStreamer.Common.System;
using SonicStreamer.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace SonicStreamer.Pages
{
    public sealed partial class SearchPage : Page
    {
        private readonly SearchViewModel _searchVm;
        private readonly PlaylistViewModel _playlistVm;

        public SearchPage()
        {
            InitializeComponent();

            if (ResourceLoader.Current.GetResource(ref _searchVm, Constants.ViewModelSearch) == false)
                _searchVm = new SearchViewModel();
            if (ResourceLoader.Current.GetResource(ref _playlistVm, Constants.ViewModelPlaylist) == false)
                _playlistVm = new PlaylistViewModel();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackPageView(GetType().Name);
        }

        private void SearchResult_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(TrackListingPage), e.ClickedItem);
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