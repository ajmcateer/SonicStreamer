using SonicStreamer.Common.System;
using SonicStreamer.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace SonicStreamer.Pages
{
    public sealed partial class PlaylistPage : Page
    {
        public readonly PlaylistViewModel PlaylistVm;
        public readonly MainViewModel MainVm;

        public PlaylistPage()
        {
            InitializeComponent();

            if (ResourceLoader.Current.GetResource(ref PlaylistVm, Constants.ViewModelPlaylist) == false)
                PlaylistVm = new PlaylistViewModel();
            if (ResourceLoader.Current.GetResource(ref MainVm, Constants.ViewModelMain) == false)
                MainVm = new MainViewModel();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackPageView(GetType().Name);
            await PlaylistVm.LoadDataAsync();
        }

        private void RenamePopupOpened(object sender, object e)
        {
            RenameTextBox.Focus(FocusState.Programmatic);
            RenameTextBox.SelectAll();
        }
    }
}