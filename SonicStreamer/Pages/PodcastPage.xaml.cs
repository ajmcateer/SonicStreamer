using SonicStreamer.Common.System;
using SonicStreamer.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace SonicStreamer.Pages
{
    public sealed partial class PodcastPage : Page
    {
        private readonly PodcastViewModel _podcastVm;

        public PodcastPage()
        {
            InitializeComponent();

            _podcastVm = Application.Current.Resources[Constants.ViewModelPodcast] as PodcastViewModel;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackPageView(GetType().Name);
            await _podcastVm.LoadDataAsync();
        }
    }
}