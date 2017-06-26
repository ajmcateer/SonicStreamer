using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Playback;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using SonicStreamer.Common.System;
using SonicStreamer.Subsonic.Data;

namespace SonicStreamer.Test.Common
{
    [TestClass]
    public class PlaybackServiceTest
    {
        [TestInitialize]
        public async Task InitAsync()
        {
            await SubsonicConnector.Current.SetSubsonicTestServerAsync();
        }

        private async Task<List<Track>> GetSampleTracks()
        {
            return await SubsonicConnector.Current.CurrentConnection.GetAlbumTracksAsync("50");
        }

        [TestMethod]
        public async Task AddToPlaybackTestAsync()
        {
            var tracks = await GetSampleTracks();
            PlaybackService.Current.AddToPlaybackAsync(tracks);
            Assert.AreEqual(tracks.Count, PlaybackService.Current.Playback.Items.Count);
            // Duplicate Check
            PlaybackService.Current.AddToPlaybackAsync(tracks, false);
            Assert.AreEqual(tracks.Count, PlaybackService.Current.Playback.Items.Count);
            // Replace Playback
            tracks.RemoveAt(0);
            PlaybackService.Current.AddToPlaybackAsync(tracks);
            Assert.AreEqual(tracks.Count, PlaybackService.Current.Playback.Items.Count);
            PlaybackService.Current.ResetPlayabck();
        }

        private string GetTrackIdOrder()
        {
            if (PlaybackService.Current.Playback.ShuffleEnabled)
            {
                return PlaybackService.Current.Playback.ShuffledItems.Aggregate(string.Empty, (current, item) =>
                    current + item.Source.CustomProperties[Constants.PlaybackTrackId].ToString());
            }
            return PlaybackService.Current.Playback.Items.Aggregate(string.Empty, (current, item) =>
                current + item.Source.CustomProperties[Constants.PlaybackTrackId].ToString());
        }

        [TestMethod]
        public async Task ShuffleModeTestAsync()
        {
            var tracks = await GetSampleTracks();
            PlaybackService.Current.SetShuffleMode(false);
            PlaybackService.Current.AddToPlaybackAsync(tracks);
            var playbackItem = PlaybackService.Current.Playback.Items.First();
            var displayProperties = playbackItem.GetDisplayProperties();
            Assert.AreEqual(tracks.First().Id,
                playbackItem.Source.CustomProperties[Constants.PlaybackTrackId].ToString());
            Assert.AreEqual(tracks.First().ArtistId,
                playbackItem.Source.CustomProperties[Constants.PlaybackArtistId].ToString());
            Assert.AreEqual(tracks.First().Cover,
                playbackItem.Source.CustomProperties[Constants.PlaybackCover] as CoverArt);
            Assert.AreEqual(tracks.First().Duration,
                playbackItem.Source.CustomProperties[Constants.PlaybackDuration].ToString());

            // delay required to wait for updated CurrentItem
            await Task.Delay(2000);
            Assert.AreEqual(playbackItem.Source.CustomProperties[Constants.PlaybackTrackId].ToString(),
                PlaybackService.Current.Playback.CurrentItem.Source.CustomProperties[Constants.PlaybackTrackId].ToString
                    ());

            Assert.AreEqual(MediaPlaybackType.Music, displayProperties.Type);
            Assert.AreEqual(tracks.First().Name, displayProperties.MusicProperties.Title);
            Assert.AreEqual(tracks.First().Artist, displayProperties.MusicProperties.Artist);
            Assert.AreEqual(tracks.First().Album, displayProperties.MusicProperties.AlbumTitle);

            var oldOrder = GetTrackIdOrder();

            PlaybackService.Current.SetShuffleMode(true);
            Assert.IsTrue(PlaybackService.Current.Playback.ShuffleEnabled);
            var newOrder = GetTrackIdOrder();
            Assert.AreNotEqual(oldOrder, newOrder);

            PlaybackService.Current.AddToPlaybackAsync(tracks);
            Assert.AreNotEqual(newOrder, GetTrackIdOrder());
            PlaybackService.Current.ResetPlayabck();
        }

        [TestMethod]
        public async Task PlayPauseTestAsync()
        {
            var tracks = await GetSampleTracks();

            // Add only one track to trigger Play mode
            PlaybackService.Current.AddToPlaybackAsync(tracks.First());
            await Task.Delay(2000);
            Assert.AreEqual(MediaPlaybackState.Playing, PlaybackService.Current.Player.PlaybackSession.PlaybackState);

            // Pause test from Playing state
            PlaybackService.Current.PlayPause();
            await Task.Delay(2000);
            Assert.AreEqual(MediaPlaybackState.Paused, PlaybackService.Current.Player.PlaybackSession.PlaybackState);

            // Add now all tracks to test correct Play/Pause behaviour
            PlaybackService.Current.AddToPlaybackAsync(tracks, false);
            await Task.Delay(2000);
            Assert.AreEqual(MediaPlaybackState.Paused, PlaybackService.Current.Player.PlaybackSession.PlaybackState);

            // Play test from Paused state
            PlaybackService.Current.PlayPause();
            await Task.Delay(2000);
            Assert.AreEqual(MediaPlaybackState.Playing, PlaybackService.Current.Player.PlaybackSession.PlaybackState);

            // Test reset playback
            PlaybackService.Current.ResetPlayabck();
            await Task.Delay(2000);
            Assert.AreNotEqual(MediaPlaybackState.Playing, PlaybackService.Current.Player.PlaybackSession.PlaybackState);

            // Test Auto Play for empty playback
            PlaybackService.Current.AddToPlaybackAsync(tracks, false);
            await Task.Delay(2000);
            Assert.AreEqual(MediaPlaybackState.Playing, PlaybackService.Current.Player.PlaybackSession.PlaybackState);
            PlaybackService.Current.ResetPlayabck();
        }

        [TestMethod]
        public async Task PlayNextPreviousTestAsync()
        {
            var tracks = await GetSampleTracks();
            PlaybackService.Current.SetShuffleMode(false);
            PlaybackService.Current.AddToPlaybackAsync(tracks);
            await Task.Delay(2000);

            // Test PlayNext
            PlaybackService.Current.PlayNext();
            // delay required to wait for updated CurrentItem
            await Task.Delay(2000);
            Assert.AreEqual(tracks.ElementAt(1).Id,
                PlaybackService.Current.Playback.CurrentItem.Source.CustomProperties[Constants.PlaybackTrackId].ToString
                    ());

            // Test PlayPrevious
            PlaybackService.Current.PlayPrevious();
            // delay required to wait for updated CurrentItem
            await Task.Delay(2000);
            Assert.AreEqual(tracks.First().Id,
                PlaybackService.Current.Playback.CurrentItem.Source.CustomProperties[Constants.PlaybackTrackId].ToString
                    ());

            // Test PlayPrevious on first playback item
            PlaybackService.Current.PlayPrevious();
            // delay required to wait for updated CurrentItem
            await Task.Delay(2000);
            Assert.AreEqual(tracks.Last().Id,
                PlaybackService.Current.Playback.CurrentItem.Source.CustomProperties[Constants.PlaybackTrackId].ToString
                    ());
            PlaybackService.Current.ResetPlayabck();
        }

        [TestMethod]
        [Ignore]
        public async Task JumpTestAsync()
        {
            // TODO Create new Sample Data
            var tracks = await GetSampleTracks();
            PlaybackService.Current.AddToPlaybackAsync(tracks);
            var rnd = new Random();
            var rndTrack = tracks.ElementAt(rnd.Next(tracks.Count - 1));
            //PlaybackService.Current.Jump(rndTrack);

            // delay required to wait for updated CurrentItem
            await Task.Delay(2000);
            Assert.AreEqual(rndTrack.Id,
                PlaybackService.Current.Playback.CurrentItem.Source.CustomProperties[Constants.PlaybackTrackId].ToString
                    ());
            PlaybackService.Current.ResetPlayabck();
        }

        [TestMethod]
        [Ignore]
        public async Task  OfflinePlaybackTestAsync()
        {
            //TODO Write OfflinePlaybackTest
            PlaybackService.Current.ResetPlayabck();
            throw new NotImplementedException();
        }
    }
}