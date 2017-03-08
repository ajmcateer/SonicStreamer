using System.Linq;
using SonicStreamer.ViewModels;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using SonicStreamer.Common.System;
using SonicStreamer.Subsonic.Data;

namespace SonicStreamer.Test.ViewModels
{
    [TestClass]
    public class PocastViewModelTest
    {
        [TestInitialize]
        public async Task InitAsync()
        {
            await SubsonicConnector.Current.SetSubsonicTestServerAsync();
        }

        [TestMethod]
        public async Task LoadDataTestAsync()
        {
            var podcastVm = new PodcastViewModel();
            await podcastVm.LoadDataAsync();
            Assert.AreNotEqual(0, podcastVm.Podcasts.Count);
            Assert.AreNotEqual(0, podcastVm.NewEpisodes.Count);
            Assert.IsNull(podcastVm.SelectedPodcast);
            Assert.IsFalse(podcastVm.AreDownloadingEpisodesAvailable);
            Assert.IsFalse(podcastVm.AreSkippedEpisodesAvailable);
            Assert.IsFalse(podcastVm.AreAvailableEpisodesAvailable);
            Assert.AreEqual(0, podcastVm.DownloadingEpisodes.Count);
            Assert.AreEqual(0, podcastVm.SkippedEpisodes.Count);
            Assert.AreEqual(0, podcastVm.AvailableEpisodes.Count);
            Assert.IsFalse(string.IsNullOrEmpty(podcastVm.Podcasts.First().Id));
            Assert.IsFalse(string.IsNullOrEmpty(podcastVm.Podcasts.First().Name));
            Assert.IsFalse(string.IsNullOrEmpty(podcastVm.Podcasts.First().Source));
            Assert.IsFalse(string.IsNullOrEmpty(podcastVm.Podcasts.First().Description));
            Assert.IsFalse(string.IsNullOrEmpty(podcastVm.Podcasts.First().Status));
            Assert.IsFalse(string.IsNullOrEmpty(podcastVm.Podcasts.First().Cover.Id));
        }

        [TestMethod]
        public async Task LoadPodcastTracksAsyncTest()
        {
            var podcastVm = new PodcastViewModel();
            await podcastVm.LoadDataAsync();
            podcastVm.SelectedPodcast = podcastVm.Podcasts.First();
            await podcastVm.LoadSelectedPodcastTracksAsync();
            Assert.AreNotEqual(0, podcastVm.SelectedPodcast.Episodes.Count);
            var episode = podcastVm.SelectedPodcast.Episodes.First();
            Assert.IsFalse(string.IsNullOrEmpty(episode.Id));
            Assert.IsFalse(string.IsNullOrEmpty(episode.Name));
            Assert.IsFalse(string.IsNullOrEmpty(episode.Description));
            Assert.IsFalse(string.IsNullOrEmpty(episode.EpisodeId));
            Assert.IsFalse(string.IsNullOrEmpty(episode.Artist));
            Assert.IsFalse(string.IsNullOrEmpty(episode.ArtistId));
            Assert.IsFalse(string.IsNullOrEmpty(episode.Album));
            Assert.IsFalse(string.IsNullOrEmpty(episode.AlbumId));
            Assert.IsFalse(string.IsNullOrEmpty(episode.BitRate));
            Assert.IsFalse(string.IsNullOrEmpty(episode.Duration));
            Assert.IsFalse(string.IsNullOrEmpty(episode.Released));
            Assert.IsFalse(string.IsNullOrEmpty(episode.Year));
            Assert.IsFalse(string.IsNullOrEmpty(episode.Cover.Id));
            Assert.IsTrue(episode.Path.Contains(episode.Artist));
            Assert.IsTrue(episode.Path.Contains(episode.Album));
            Assert.IsTrue(episode.Path.Contains(episode.EpisodeId));
            Assert.AreNotEqual(PodcastEpisode.DownloadStatus.Unkown, episode.EpisodeStatus);
        }
    }
}