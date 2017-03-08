using SonicStreamer.Pages;
using Windows.UI.Xaml.Controls;
using SonicStreamer.Common.System;
using SonicStreamer.ViewModels;

namespace SonicStreamer.Controls
{
    public sealed partial class SimilarArtistsView : UserControl
    {
        public TrackListingViewModel TrackListingVm;

        public SimilarArtistsView()
        {
            InitializeComponent();

            if (ResourceLoader.Current.GetResource(ref TrackListingVm, Constants.ViewModelTrackListing) == false)
                TrackListingVm = new TrackListingViewModel();
        }

        private void SubsonicSimilarArtist_ItemClick(object sender, ItemClickEventArgs e)
        {
            var rootFrame = Tag as Frame;
            rootFrame?.Navigate(typeof(TrackListingPage), e.ClickedItem);
        }
    }
}