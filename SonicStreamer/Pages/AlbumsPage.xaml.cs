using SonicStreamer.Common.System;
using SonicStreamer.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace SonicStreamer.Pages
{
    public sealed partial class AlbumsPage : Page
    {
        readonly AlbumViewModel _albumVm;
        readonly PlaylistViewModel _playlistVm;

        public AlbumsPage()
        {
            InitializeComponent();

            if (ResourceLoader.Current.GetResource(ref _albumVm, Constants.ViewModelAlbum) == false)
                _albumVm = new AlbumViewModel();
            if (ResourceLoader.Current.GetResource(ref _playlistVm, Constants.ViewModelPlaylist) == false)
                _playlistVm = new PlaylistViewModel();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackPageView(GetType().Name);
            await _albumVm.LoadDataAsync();
            ListViewSource.Source = _albumVm.Items;
            var listViewBase = SemanticZoomContainer.ZoomedOutView as ListViewBase;
            if (listViewBase != null)
                listViewBase.ItemsSource = ListViewSource.View.CollectionGroups;
        }

        private void listingItemsView_ItemClick(object sender, ItemClickEventArgs e)
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