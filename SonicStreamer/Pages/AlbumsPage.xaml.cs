using SonicStreamer.Common.System;
using SonicStreamer.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace SonicStreamer.Pages
{
    public sealed partial class AlbumsPage : Page
    {
        public readonly AlbumViewModel AlbumVm;
        public readonly PlaylistViewModel PlaylistVm;
        public readonly MainViewModel MainVm;

        public AlbumsPage()
        {
            InitializeComponent();

            if (ResourceLoader.Current.GetResource(ref AlbumVm, Constants.ViewModelAlbum) == false)
                AlbumVm = new AlbumViewModel();
            if (ResourceLoader.Current.GetResource(ref PlaylistVm, Constants.ViewModelPlaylist) == false)
                PlaylistVm = new PlaylistViewModel();
            if (ResourceLoader.Current.GetResource(ref MainVm, Constants.ViewModelMain) == false)
                MainVm = new MainViewModel();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackPageView(GetType().Name);
            await AlbumVm.LoadDataAsync();
            ListViewSource.Source = AlbumVm.Items;
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