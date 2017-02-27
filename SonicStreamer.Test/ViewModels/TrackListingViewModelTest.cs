using SonicStreamer.ViewModels;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using SonicStreamer.Common.System;
using SonicStreamer.Subsonic.Data;
using SonicStreamer.ViewModelItems;

namespace SonicStreamer.Test.ViewModels
{
    [TestClass]
    public class TrackListingViewModelTest
    {
        [TestInitialize]
        public async Task InitAsync()
        {
            await SubsonicConnector.Current.SetSubsonicTestServerAsync();
        }

        [TestMethod]
        public async Task LoadDataFromArtistAsyncTest()
        {
            var trackListingVm = new TrackListingViewModel();
            await trackListingVm.LoadDataAsync(new Artist { Id = "23" });
            Assert.AreEqual(4, trackListingVm.Albums.Count);
            Assert.AreEqual("The Dada Weatherman", trackListingVm.PageTitle);
            Assert.AreEqual("The Dada Weatherman", trackListingVm.ArtistInfo.Name);
            // Demo Server delivers no TopSongs
            // Assert.AreEqual(0, trackListingVm.TopSongs.Count); 
        }

        [TestMethod]
        public async Task LoadDataFromAlbumAsyncTest()
        {
            var trackListingVm = new TrackListingViewModel();
            await trackListingVm.LoadDataAsync(new Album { Id = "50" });
            Assert.AreEqual(1, trackListingVm.Albums.Count);
            Assert.AreEqual("Birthnight", trackListingVm.PageTitle);
            Assert.AreEqual("The Dada Weatherman", trackListingVm.ArtistInfo.Name);
        }

        [TestMethod]
        public async Task LoadDataFromListingItemAsyncTest()
        {
            var trackListingVm = new TrackListingViewModel();
            await trackListingVm.LoadDataAsync(new ListingItem { MusicObject = new Artist { Id = "23" } });
            Assert.AreEqual(4, trackListingVm.Albums.Count);
            Assert.AreEqual("The Dada Weatherman", trackListingVm.PageTitle);
            Assert.AreEqual("The Dada Weatherman", trackListingVm.ArtistInfo.Name);
        }
    }
}