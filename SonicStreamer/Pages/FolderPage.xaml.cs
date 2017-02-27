using SonicStreamer.Common.System;
using SonicStreamer.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace SonicStreamer.Pages
{
    public sealed partial class FolderPage : Page
    {
        private readonly FolderViewModel _folderVm;
        private readonly PlaylistViewModel _playlistVm;

        public FolderPage()
        {
            InitializeComponent();

            if (ResourceLoader.Current.GetResource(ref _folderVm, Constants.ViewModelFolder) == false)
                _folderVm = new FolderViewModel();
            if (ResourceLoader.Current.GetResource(ref _playlistVm, Constants.ViewModelPlaylist) == false)
                _playlistVm = new PlaylistViewModel();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackPageView(GetType().Name);
            await _folderVm.LoadDataAsync();
            var listViewBase = SemanticZoomContainer.ZoomedOutView as ListViewBase;
            if (listViewBase != null)
                listViewBase.ItemsSource = FolderListViewSource.View.CollectionGroups;
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