using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using SonicStreamer.Common.System;
using SonicStreamer.ViewModels;

namespace SonicStreamer.Test.ViewModels
{
    [TestClass]
    public class SearchViewModelTest
    {
        [TestInitialize]
        public async Task InitAsync()
        {
            await SubsonicConnector.Current.SetSubsonicTestServerAsync();
        }

        [TestMethod]
        public void LoadDataTest()
        {
            var searchVm = new SearchViewModel();
            searchVm.LoadData();
            Assert.AreEqual(0, searchVm.ResultArtist.Count);
            Assert.AreEqual(0, searchVm.ResultAlbum.Count);
            Assert.AreEqual(0, searchVm.ResultTrack.Count);
            Assert.AreEqual(0, searchVm.SelectedArtists.Count);
            Assert.AreEqual(0, searchVm.SelectedAlbums.Count);
            Assert.AreEqual(0, searchVm.SelectedTracks.Count);
            Assert.IsFalse(searchVm.IsResultViewVisible);
            Assert.IsFalse(searchVm.IsArtistResultViewVisible);
            Assert.IsFalse(searchVm.IsAlbumResultViewVisible);
            Assert.IsFalse(searchVm.IsTrackResultViewVisible);
            Assert.IsTrue(searchVm.IsItemClickEnabled);
            Assert.AreEqual(ListViewSelectionMode.None, searchVm.SelectionMode);
        }

        [TestMethod]
        public async Task SearchAsyncTest()
        {
            var searchVm = new SearchViewModel();
            searchVm.LoadData();
            await searchVm.SearchAsync("Dada");
            Assert.AreNotEqual(0, searchVm.ResultArtist.Count);
            Assert.AreNotEqual(0, searchVm.ResultAlbum.Count);
            Assert.AreNotEqual(0, searchVm.ResultTrack.Count);
            Assert.IsTrue(searchVm.IsResultViewVisible);
            Assert.IsTrue(searchVm.IsArtistResultViewVisible);
            Assert.IsTrue(searchVm.IsAlbumResultViewVisible);
            Assert.IsTrue(searchVm.IsTrackResultViewVisible);
            Assert.IsFalse(searchVm.IsNoArtistResultLabelVisible);
            Assert.IsFalse(searchVm.IsNoAlbumResultLabelVisible);
            Assert.IsFalse(searchVm.IsNoTrackResultLabelVisible);

            // Negative Test
            await searchVm.SearchAsync("foobar");
            Assert.AreEqual(0, searchVm.ResultArtist.Count);
            Assert.AreEqual(0, searchVm.ResultAlbum.Count);
            Assert.AreEqual(0, searchVm.ResultTrack.Count);
            Assert.IsFalse(searchVm.IsResultViewVisible);
            Assert.IsFalse(searchVm.IsArtistResultViewVisible);
            Assert.IsFalse(searchVm.IsAlbumResultViewVisible);
            Assert.IsFalse(searchVm.IsTrackResultViewVisible);
            Assert.IsTrue(searchVm.IsNoArtistResultLabelVisible);
            Assert.IsTrue(searchVm.IsNoAlbumResultLabelVisible);
            Assert.IsTrue(searchVm.IsNoTrackResultLabelVisible);
        }
    }
}