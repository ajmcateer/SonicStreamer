using SonicStreamer.Common.System;
using SonicStreamer.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

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