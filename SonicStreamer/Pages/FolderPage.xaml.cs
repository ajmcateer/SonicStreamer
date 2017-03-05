using SonicStreamer.Common.System;
using SonicStreamer.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace SonicStreamer.Pages
{
    public sealed partial class FolderPage : Page
    {
        public readonly FolderViewModel FolderVm;
        public readonly PlaylistViewModel PlaylistVm;
        public readonly MainViewModel MainVm;

        public FolderPage()
        {
            InitializeComponent();

            if (ResourceLoader.Current.GetResource(ref FolderVm, Constants.ViewModelFolder) == false)
                FolderVm = new FolderViewModel();
            if (ResourceLoader.Current.GetResource(ref PlaylistVm, Constants.ViewModelPlaylist) == false)
                PlaylistVm = new PlaylistViewModel();
            if (ResourceLoader.Current.GetResource(ref MainVm, Constants.ViewModelMain) == false)
                MainVm = new MainViewModel();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackPageView(GetType().Name);
            await FolderVm.LoadDataAsync();
            var listViewBase = SemanticZoomContainer.ZoomedOutView as ListViewBase;
            if (listViewBase != null)
                listViewBase.ItemsSource = FolderListViewSource.View.CollectionGroups;
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