using System;
using SonicStreamer.Common.System;
using SonicStreamer.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using SonicStreamer.Controls;

namespace SonicStreamer.Pages
{
    public sealed partial class SearchPage : Page
    {
        public readonly SearchViewModel SearchVm;
        public readonly PlaylistViewModel PlaylistVm;
        public readonly MainViewModel MainVm;

        public SearchPage()
        {
            InitializeComponent();

            if (ResourceLoader.Current.GetResource(ref SearchVm, Constants.ViewModelSearch) == false)
                SearchVm = new SearchViewModel();
            if (ResourceLoader.Current.GetResource(ref PlaylistVm, Constants.ViewModelPlaylist) == false)
                PlaylistVm = new PlaylistViewModel();
            if (ResourceLoader.Current.GetResource(ref MainVm, Constants.ViewModelMain) == false)
                MainVm = new MainViewModel();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackPageView(GetType().Name);
        }

        private void SearchResult_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(TrackListingPage), e.ClickedItem);
        }

        private async void AddToPlaylist_Click(object sender, RoutedEventArgs e)
        {
            AddToPlaylistDialog.Content = new AddToPlaylistDialog();

            var dialogResult = await AddToPlaylistDialog.ShowAsync();
            if (dialogResult == ContentDialogResult.Primary)
            {
                await SearchVm.AddToPlaylistAsync();
            }
            PlaylistVm.ResetDialogInputs();
        }

        private async void AddToPlaylistDialog_OnOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            await PlaylistVm.LoadDialogDataAsync();
        }
    }
}