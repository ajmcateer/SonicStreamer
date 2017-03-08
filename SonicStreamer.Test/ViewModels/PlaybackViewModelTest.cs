using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media;
using Windows.UI.Xaml.Controls;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using SonicStreamer.Common.Extension;
using SonicStreamer.Common.System;
using SonicStreamer.Subsonic.Data;
using SonicStreamer.ViewModels;

namespace SonicStreamer.Test.ViewModels
{
    [TestClass]
    public class PlaybackViewModelTest
    {
        [TestInitialize]
        public async Task InitAsync()
        {
            await SubsonicConnector.Current.SetSubsonicTestServerAsync();
        }

        /// <summary>
        /// Get some sample tracks from the server and return the results 
        /// </summary>
        /// <returns></returns>
        private async Task<List<Track>> GetSampleTracks()
        {
            return await SubsonicConnector.Current.CurrentConnection.GetAlbumTracksAsync("50");
        }

        /// <summary>
        /// Initializes a new playback list
        /// </summary>
        /// <param name="tracks">Tracks which should be added to the new playback</param>
        /// <returns>Instance of the current used <see cref="PlaybackViewModel"/></returns>
        private async Task<PlaybackViewModel> InitNewPlayback(IEnumerable<Track> tracks)
        {
            var playbackVm = new PlaybackViewModel();
            ResourceLoader.Current.SetNewTempResource(playbackVm, Constants.ViewModelPlayback);
            tracks = await GetSampleTracks();
            await PlaybackService.Current.AddToPlaybackAsync(tracks);
            await Task.Delay(2000);
            return playbackVm;
        }

        /// <summary>
        /// Checks all properties of the CurrentTrack
        /// </summary>
        /// <param name="playbackVm">Current <see cref="PlaybackViewModel"/> Instance</param>
        private static void AssertCurrentTrack(PlaybackViewModel playbackVm)
        {
            Assert.AreEqual(
                PlaybackService.Current.Playback.CurrentItem.Source.CustomProperties[Constants.PlaybackTrackId].ToString
                    (), playbackVm.CurrentTrack.Id);
            Assert.AreEqual(
                PlaybackService.Current.Playback.CurrentItem.Source.CustomProperties[Constants.PlaybackArtistId]
                    .ToString(), playbackVm.CurrentTrack.ArtistId);
            Assert.AreEqual(
                ((CoverArt)
                    PlaybackService.Current.Playback.CurrentItem.Source.CustomProperties[Constants.PlaybackCover]).Id,
                playbackVm.CurrentTrack.Cover.Id);
            Assert.AreEqual(
                PlaybackService.Current.Playback.CurrentItem.Source.CustomProperties[Constants.PlaybackDuration]
                    .ToString(), playbackVm.CurrentTrack.Duration);

            var displayProperties = PlaybackService.Current.Playback.CurrentItem.GetDisplayProperties();
            Assert.AreEqual(displayProperties.MusicProperties.Title, playbackVm.CurrentTrack.Name);
            Assert.AreEqual(displayProperties.MusicProperties.Artist, playbackVm.CurrentTrack.Artist);
            Assert.AreEqual(displayProperties.MusicProperties.AlbumTitle, playbackVm.CurrentTrack.Album);
            Assert.AreEqual(PlaybackService.Current.Playback.Items.Count, playbackVm.Tracks.Count);
        }

        [TestMethod]
        public async Task LoadDataTestAsync()
        {
            using (var playbackVm = new PlaybackViewModel())
            {
                await playbackVm.LoadDataAsync();
                Assert.AreEqual(0, playbackVm.Tracks.Count);
                Assert.AreEqual(Symbol.Play, playbackVm.PlayButtonIcon);
                Assert.AreEqual(PlaybackViewModel.PlaybackPanelStatus.Large, playbackVm.PanelStaus);
                Assert.IsNull(playbackVm.CurrentTrack);
                Assert.IsNull(playbackVm.CurrentArtistInfo);
                Assert.IsFalse(playbackVm.IsPlaybackPanelVisible);
                PlaybackService.Current.ResetPlayabck();
            }
        }

        [TestMethod]
        public void ShuffleAndRepeatEnableTest()
        {
            using (var playbackVm = new PlaybackViewModel())
            {
                playbackVm.SwitchOnShuffleMode();
                Assert.IsTrue(playbackVm.IsShuffling);
                Assert.IsTrue(PlaybackService.Current.Playback.ShuffleEnabled);
                playbackVm.SwitchOffShuffleMode();
                Assert.IsFalse(playbackVm.IsShuffling);
                Assert.IsFalse(PlaybackService.Current.Playback.ShuffleEnabled);

                playbackVm.SwitchOnRepeatingMode();
                Assert.IsTrue(playbackVm.IsRepeating);
                playbackVm.SwitchOffRepeatingMode();
                Assert.IsFalse(playbackVm.IsRepeating);
                PlaybackService.Current.ResetPlayabck();
            }
        }

        [TestMethod]
        public async Task AddToPlaybackTestAsync()
        {
            var tracks = await GetSampleTracks();
            using (var playbackVm = await InitNewPlayback(tracks.GetRange(0, 2)))
            {
                AssertCurrentTrack(playbackVm);

                // Add test with duplicates
                await PlaybackService.Current.AddToPlaybackAsync(tracks, false);
                Assert.AreEqual(PlaybackService.Current.Playback.Items.Count, playbackVm.Tracks.Count);
                PlaybackService.Current.ResetPlayabck();
            }
        }

        [TestMethod]
        public async Task PlayNextPreviousTestAsync()
        {
            using (var playbackVm = await InitNewPlayback(await GetSampleTracks()))
            {
                playbackVm.PlayNext();
                await Task.Delay(2000);
                AssertCurrentTrack(playbackVm);

                // Test PlayPrevious
                playbackVm.PlayPrevious();
                await Task.Delay(2000);
                AssertCurrentTrack(playbackVm);
                PlaybackService.Current.ResetPlayabck();
            }
        }

        [TestMethod]
        public async Task JumpTestAsync()
        {
            using (var playbackVm = await InitNewPlayback(await GetSampleTracks()))
            {
                playbackVm.Jump(playbackVm.Tracks.Last());
                await Task.Delay(2000);
                AssertCurrentTrack(playbackVm);
                PlaybackService.Current.ResetPlayabck();
            }
        }

        [TestMethod]
        public async Task ArtistInfoTestAsync()
        {
            using (var playbackVm = await InitNewPlayback(await GetSampleTracks()))
            {
                await playbackVm.LoadDataAsync();
                Assert.AreEqual(playbackVm.CurrentTrack.Artist, playbackVm.CurrentArtistInfo.Name);
                Assert.AreNotEqual(playbackVm.CurrentArtistInfo.SocialLinks.Count, 0);
                PlaybackService.Current.ResetPlayabck();
            }
        }

    }
}