using SonicStreamer.Common.System;
using SonicStreamer.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace SonicStreamer.Pages
{
    public sealed partial class PodcastPage : Page
    {
        public readonly PodcastViewModel PodcastVm;
        public readonly MainViewModel MainVm;

        public PodcastPage()
        {
            InitializeComponent();

            if (ResourceLoader.Current.GetResource(ref PodcastVm, Constants.ViewModelPodcast) == false)
                PodcastVm = new PodcastViewModel();
            if (ResourceLoader.Current.GetResource(ref MainVm, Constants.ViewModelMain) == false)
                MainVm = new MainViewModel();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackPageView(GetType().Name);
            await PodcastVm.LoadDataAsync();
        }
    }
}