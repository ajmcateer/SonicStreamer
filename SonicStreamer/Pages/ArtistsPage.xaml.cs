using SonicStreamer.Common.System;
using SonicStreamer.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace SonicStreamer.Pages
{
    public sealed partial class ArtistsPage : Page
    {
        public readonly ArtistsViewModel ArtistVm;
        public readonly PlaylistViewModel PlaylistVm;
        public readonly MainViewModel MainVm;

        public ArtistsPage()
        {
            InitializeComponent();

            if (ResourceLoader.Current.GetResource(ref ArtistVm, Constants.ViewModelArtist) == false)
                ArtistVm = new ArtistsViewModel();
            if (ResourceLoader.Current.GetResource(ref PlaylistVm, Constants.ViewModelPlaylist) == false)
                PlaylistVm = new PlaylistViewModel();
            if (ResourceLoader.Current.GetResource(ref MainVm, Constants.ViewModelMain) == false)
                MainVm = new MainViewModel();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackPageView(GetType().Name);
            await ArtistVm.LoadDataAsync();
            ListViewSource.Source = ArtistVm.Items;
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