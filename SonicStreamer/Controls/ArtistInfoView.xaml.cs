using Windows.UI.Xaml.Controls;
using SonicStreamer.Common.System;
using SonicStreamer.ViewModels;

namespace SonicStreamer.Controls
{
    public sealed partial class ArtistInfoView : UserControl
    {
        public TrackListingViewModel TrackListingVm;

        public ArtistInfoView()
        {
            InitializeComponent();

            if (ResourceLoader.Current.GetResource(ref TrackListingVm, Constants.ViewModelTrackListing) == false)
                TrackListingVm = new TrackListingViewModel();
        }
    }
}