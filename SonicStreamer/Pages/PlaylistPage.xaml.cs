using SonicStreamer.Common.System;
using SonicStreamer.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace SonicStreamer.Pages
{
    public sealed partial class PlaylistPage : Page
    {
        private readonly PlaylistViewModel _playlistVm;

        public PlaylistPage()
        {
            InitializeComponent();

            if (ResourceLoader.Current.GetResource(ref _playlistVm, Constants.ViewModelPlaylist) == false)
                _playlistVm = new PlaylistViewModel();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackPageView(GetType().Name);
            await _playlistVm.LoadDataAsync();
        }

        private void RenamePopupOpened(object sender, object e)
        {
            RenameTextBox.Focus(FocusState.Programmatic);
            RenameTextBox.SelectAll();
        }
    }
}