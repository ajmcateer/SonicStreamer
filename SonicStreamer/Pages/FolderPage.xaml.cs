using System;
using SonicStreamer.Common.System;
using SonicStreamer.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using SonicStreamer.Controls;

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

        private async void AddToPlaylist_Click(object sender, RoutedEventArgs e)
        {
            AddToPlaylistDialog.Content = new AddToPlaylistDialog();

            var dialogResult = await AddToPlaylistDialog.ShowAsync();
            if (dialogResult == ContentDialogResult.Primary)
            {
                await FolderVm.AddToPlaylistAsync();
            }
            PlaylistVm.ResetDialogInputs();
        }

        private async void AddToPlaylistDialog_OnOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            await PlaylistVm.LoadDialogDataAsync();
        }
    }
}