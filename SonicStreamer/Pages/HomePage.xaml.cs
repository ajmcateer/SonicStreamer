using SonicStreamer.Common.System;
using SonicStreamer.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace SonicStreamer.Pages
{
    public sealed partial class HomePage : Page
    {
        public readonly StartViewModel StartVm;
        public readonly MainViewModel MainVm;

        public HomePage()
        {
            InitializeComponent();

            if (ResourceLoader.Current.GetResource(ref StartVm, Constants.ViewModelStart) == false)
                StartVm = new StartViewModel();
            if (ResourceLoader.Current.GetResource(ref MainVm, Constants.ViewModelMain) == false)
                MainVm = new MainViewModel();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackPageView(GetType().Name);
            await StartVm.LoadDataAsync();
        }

        private void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(TrackListingPage), e.ClickedItem);
        }
    }
}