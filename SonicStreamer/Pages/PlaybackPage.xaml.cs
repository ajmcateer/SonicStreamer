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
        readonly PlaybackViewModel _playbackVm;

        public PlaybackPage()
        {
            InitializeComponent();

            if (ResourceLoader.Current.GetResource(ref _playbackVm, Constants.ViewModelPlayback) == false)
                _playbackVm = new PlaybackViewModel();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackPageView(GetType().Name);

            if (!_playbackVm.IsSmallPlaybackEnabled) return;

            _playbackVm.PanelStaus = PlaybackViewModel.PlaybackPanelStatus.Page;
            _playbackVm.IsPlaybackPanelVisible = false;
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
            if (!_playbackVm.IsSmallPlaybackEnabled) return;
            _playbackVm.PanelStaus = PlaybackViewModel.PlaybackPanelStatus.Small;
            _playbackVm.IsPlaybackPanelVisible = true;
        }

        private void PlaybackTracks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var control = sender as ListView;
            control?.ScrollIntoView(control.SelectedItem);
        }
    }
}