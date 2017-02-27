using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using SonicStreamer.Common.System;
using SonicStreamer.ViewModels;

namespace SonicStreamer.Test.ViewModels
{
    [TestClass]
    public class PlaylistViewModelTest
    {
        [TestInitialize]
        public async Task InitAsync()
        {
            await SubsonicConnector.Current.SetSubsonicTestServerAsync();
        }

        [TestMethod]
        public async Task LoadDataTestAsync()
        {
            var playlistVm = new PlaylistViewModel();
            await playlistVm.LoadDataAsync();
            Assert.IsFalse(playlistVm.IsRenamePopupOpen);
            Assert.IsNull(playlistVm.SelectedPlaylist);
            Assert.AreEqual(0, playlistVm.SelectedItems.Count);
            Assert.AreNotEqual(0, playlistVm.Playlists);
            foreach (var item in playlistVm.Playlists)
            {
                Assert.IsFalse(string.IsNullOrEmpty(item.Name));
                Assert.AreEqual(0, item.Tracks.Count);
            }
        }

        [TestMethod]
        public async Task LoadPlaylistTracksTestAsync()
        {
            // Due to the fact that the most playlist on Subsonic Demo Server
            // have no tracks, go through all playlist and check if any track
            // could be loaded. If yes then the test is passed
            var playlistVm = new PlaylistViewModel();
            var tracksFound = false;
            await playlistVm.LoadDataAsync();
            foreach (var item in playlistVm.Playlists)
            {
                await playlistVm.LoadPlaylistTracksAsync(item);
                if (item.Tracks.Count == 0) continue;
                tracksFound = true;
                break;
            }
            Assert.IsTrue(tracksFound);
        }

        [TestMethod]
        public async Task GetPlaylistTestAsync()
        {
            var playlistVm = new PlaylistViewModel();
            await playlistVm.LoadDataAsync();
            var rnd = new Random();
            var index = rnd.Next(playlistVm.Playlists.Count - 1);
            var testPlaylist = playlistVm.Playlists.ElementAt(index);
            var resultPlaylist = await playlistVm.GetPlaylist(testPlaylist.Id);
            Assert.AreEqual(testPlaylist.Name, resultPlaylist.Name);
            Assert.AreEqual(testPlaylist.Comment, resultPlaylist.Comment);
            Assert.AreEqual(testPlaylist.Duration, resultPlaylist.Duration);
            Assert.AreEqual(testPlaylist.Cover.Id, resultPlaylist.Cover.Id);
        }

        [TestMethod]
        [Ignore]
        public async Task CreatePlaylistTestAsync()
        {
            //TODO Write CreatePlaylistTest
            throw new NotImplementedException();
        }

        [TestMethod]
        [Ignore]
        public async Task AddTracksToPlaylistTestAsync()
        {
            //TODO Write AddTracksToPlaylist
            throw new NotImplementedException();
        }

        [TestMethod]
        [Ignore]
        public async Task RenamePlaylistTestAsync()
        {
            //TODO Write RenamePlaylistTest
            throw new NotImplementedException();
        }

        [TestMethod]
        [Ignore]
        public async Task DeletePlaylistTestAsync()
        {
            //TODO Write DeletePlaylistTest
            throw new NotImplementedException();
        }
    }
}
