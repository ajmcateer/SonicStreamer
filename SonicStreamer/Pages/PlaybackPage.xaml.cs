using SonicStreamer.Common.System;
using SonicStreamer.Controls;
using SonicStreamer.ViewModels;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace SonicStreamer.Pages
{
    public sealed partial class PlaybackPage : Page
    {
        public readonly PlaybackViewModel PlaybackVm;
        public readonly MainViewModel MainVm;

        public PlaybackPage()
        {
            InitializeComponent();

            if (ResourceLoader.Current.GetResource(ref PlaybackVm, Constants.ViewModelPlayback) == false)
                PlaybackVm = new PlaybackViewModel();
            if (ResourceLoader.Current.GetResource(ref MainVm, Constants.ViewModelMain) == false)
                MainVm = new MainViewModel();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackPageView(GetType().Name);

            if (!PlaybackVm.IsSmallPlaybackEnabled) return;

            PlaybackVm.PanelStaus = PlaybackViewModel.PlaybackPanelStatus.Page;
            PlaybackVm.IsPlaybackPanelVisible = false;
            var firstPivotItem = PlaybackPivot.Items.First() as PivotItem;
            if (firstPivotItem?.Header as string == "playing") return;
            var playbackView = new PivotItem
            {
                Content = new MobilePlaybackView(),
                Header = "playing"
            };
            PlaybackPivot.Items?.Insert(0, playbackView);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (!PlaybackVm.IsSmallPlaybackEnabled) return;
            PlaybackVm.PanelStaus = PlaybackViewModel.PlaybackPanelStatus.Small;
            PlaybackVm.IsPlaybackPanelVisible = true;
        }

        private void PlaybackTracks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var control = sender as ListView;
            control?.ScrollIntoView(control.SelectedItem);
        }
    }
}