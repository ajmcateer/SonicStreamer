using System;
using SonicStreamer.Common.System;
using SonicStreamer.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using SonicStreamer.Controls;

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

        private async void AddToPlaylist_Click(object sender, RoutedEventArgs e)
        {
            AddToPlaylistDialog.Content = new AddToPlaylistDialog();

            var dialogResult = await AddToPlaylistDialog.ShowAsync();
            if (dialogResult == ContentDialogResult.Primary)
            {
                await ArtistVm.AddToPlaylistAsync();
            }
            PlaylistVm.ResetDialogInputs();
        }

        private async void AddToPlaylistDialog_OnOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            await PlaylistVm.LoadDialogDataAsync();
        }
    }
}