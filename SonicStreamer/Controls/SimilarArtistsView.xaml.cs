using SonicStreamer.Pages;
using Windows.UI.Xaml.Controls;

namespace SonicStreamer.Controls
{
    public sealed partial class SimilarArtistsView : UserControl
    {
        public SimilarArtistsView()
        {
            InitializeComponent();
        }

        private void SubsonicSimilarArtist_ItemClick(object sender, ItemClickEventArgs e)
        {
            var rootFrame = Tag as Frame;
            rootFrame?.Navigate(typeof(TrackListingPage), e.ClickedItem);
        }
    }
}