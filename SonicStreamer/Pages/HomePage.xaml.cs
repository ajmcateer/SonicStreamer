using SonicStreamer.Common.System;
using SonicStreamer.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace SonicStreamer.Pages
{
    public sealed partial class HomePage : Page
    {
        private readonly StartViewModel _startVm;

        public HomePage()
        {
            InitializeComponent();

            if (ResourceLoader.Current.GetResource(ref _startVm, Constants.ViewModelStart) == false)
                _startVm = new StartViewModel();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackPageView(GetType().Name);
            await _startVm.LoadDataAsync();
        }

        private void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(TrackListingPage), e.ClickedItem);
        }
    }
}