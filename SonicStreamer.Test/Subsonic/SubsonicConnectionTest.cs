using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using SonicStreamer.Subsonic.Server;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SonicStreamer.Test.Subsonic
{
    [TestClass]
    public class SubsonicConnectionTest : ISubsonicConnectionTest
    {
        private DemoServerConfig _subsonicDemo;

        [TestInitialize]
        public void Init()
        {
            _subsonicDemo = DemoServerConfig.GetSubsonicDemoServer();
        }

        /// <summary>
        /// Used to have a new fresh instance of <see cref="SubsonicConnection"/>
        /// </summary>
        private async Task<SubsonicConnection> GetNewConnector()
        {
            var result = new SubsonicConnection();
            await result.Connect(_subsonicDemo.Server, _subsonicDemo.User, _subsonicDemo.Password);
            return result;
        }

        [TestMethod]
        public async Task TestServerConnectionTestAsync()
        {
            var connector = new SubsonicConnection();

            Assert.AreEqual(connector.ServerApiVersion, "1.0.0");
            Assert.IsTrue(
                (await connector.Connect(_subsonicDemo.Server, _subsonicDemo.User, _subsonicDemo.Password)).Item1);
            Assert.AreNotEqual(connector.ServerApiVersion, "1.0.0");

            connector.Disconnect();
            Assert.AreEqual(string.Empty, connector.ServerUrl);
            Assert.AreEqual(string.Empty, connector.User);
            Assert.AreEqual(string.Empty, connector.Password);
            Assert.AreEqual("1.0.0", connector.ServerApiVersion);
        }

        [TestMethod]
        public async Task WrongEntriesTestAsync()
        {
            var connector = new SubsonicConnection();

            var result = await connector.Connect($"{_subsonicDemo.Server}/foobar", _subsonicDemo.User,
                _subsonicDemo.Password);
            Assert.IsFalse(result.Item1, "Negative Test wrong Server failed");

            result = await connector.Connect(_subsonicDemo.Server, "wrongsUser", _subsonicDemo.Password);
            Assert.IsFalse(result.Item1, "Negative Test wrong User failed");
            Assert.IsTrue(result.Item2.Contains("Wrong username or password."));

            result = await connector.Connect(_subsonicDemo.Server, _subsonicDemo.User, "wrongPassword");
            Assert.IsFalse(result.Item1, "Negative Test wrong Password failed");
            Assert.IsTrue(result.Item2.Contains("Wrong username or password."));
        }

        [TestMethod]
        public async Task EmptyAndNullEntriesTestAsync()
        {
            var connector = new SubsonicConnection();

            var result = await connector.Connect(string.Empty, _subsonicDemo.User, _subsonicDemo.Password);
            Assert.IsFalse(result.Item1, "Negative Test empty Server failed");

            result = await connector.Connect(_subsonicDemo.Server, string.Empty, _subsonicDemo.Password);
            Assert.IsFalse(result.Item1, "Negative Test empty User failed");
            Assert.AreEqual("Wrong username or password.", result.Item2);

            result = await connector.Connect(_subsonicDemo.Server, null, _subsonicDemo.Password);
            Assert.IsFalse(result.Item1, "Negative Test NULL User failed");
            Assert.AreEqual("Wrong username or password.", result.Item2);

            result = await connector.Connect(_subsonicDemo.Server, _subsonicDemo.User, string.Empty);
            Assert.IsFalse(result.Item1, "Negative Test empty Password failed");
            Assert.AreEqual("Wrong username or password.", result.Item2);

            result = await connector.Connect(_subsonicDemo.Server, _subsonicDemo.User, null);
            Assert.IsFalse(result.Item1, "Negative Test NULL Password failed");
            Assert.AreEqual("Wrong username or password.", result.Item2);
        }

        [TestMethod]
        public async Task GetArtistsTestAsync()
        {
            var connector = await GetNewConnector();
            var results = await connector.GetArtistsAsync();
            Assert.AreEqual(24, results.Count);
            var artist = results.First();
            Assert.AreEqual("12", artist.Id);
            Assert.AreEqual("Antígona", artist.Name);
            Assert.AreEqual("ar-12", artist.Cover.Id);
            Assert.AreEqual("1", artist.AlbumCount);
            Assert.AreEqual(false, artist.IsSelected);
            // Data not available in GetArtists API Request
            Assert.AreEqual(0, artist.Albums.Count);
            Assert.AreEqual(string.Empty, artist.Mbid);
        }

        [TestMethod]
        public async Task GetArtistTestAsync()
        {
            var connector = await GetNewConnector();
            var result = await connector.GetArtistAsync("12");
            Assert.AreEqual("12", result.Id);
            Assert.AreEqual("Antígona", result.Name);
            Assert.AreEqual("ar-12", result.Cover.Id);
            Assert.AreEqual("1", result.AlbumCount);
            Assert.AreEqual(1, result.Albums.Count);
            var album = result.Albums.First();
            Assert.AreEqual("21", album.Id);
            Assert.AreEqual("Antígona", album.Name);
            Assert.AreEqual("al-21", album.Cover.Id);
            Assert.AreEqual("2008", album.Year);
            Assert.AreEqual("1043", album.Duration);
            Assert.AreEqual("Antígona", album.Artist);
            Assert.AreEqual("12", album.ArtistId);
            Assert.AreEqual("4", album.TrackCount);
            Assert.AreEqual(false, album.IsSelected);
            // Data not available in GetArtist API Request
            Assert.AreEqual(string.Empty, result.Mbid);
            Assert.AreEqual(0, album.Tracks.Count);
        }

        [TestMethod]
        public async Task GetArtistInfoTestAsync()
        {
            var connector = await GetNewConnector();
            var result = await connector.GetArtistInfoAsync("23");
            Assert.IsTrue(result.Biography.StartsWith("The Dada Weatherman is Jonathan's folk project"));
            Assert.AreEqual("http://img2-ak.lst.fm/i/u/71fe26e282594c91a53aba688cd0ab54.png", result.Image);
            Assert.AreEqual("http://www.last.fm/music/The+Dada+Weatherman", result.LastFmUrl);
            Assert.AreEqual("cb974849-8230-483f-869e-643528bb3565", result.Mbid);
            // This artist has no SubsonicSimilarArtists
            Assert.AreEqual(0, result.SubsonicSimilarArtists.Count);

            // Data not generated via Subsonic
            Assert.AreEqual(0, result.Infos.Count);
            Assert.AreEqual(0, result.LastFmSimilarArtists.Count);
            Assert.AreEqual(null, result.Name);
            Assert.AreEqual(0, result.SocialLinks.Count);
            Assert.AreEqual(0, result.Tags.Count);
            Assert.AreEqual(null, result.Type);
            // Data will be set afterwards in ViewModel
            Assert.AreEqual(false, result.HasBiography);
            Assert.AreEqual(false, result.HasGeneralInfo);
            Assert.AreEqual(false, result.HasSocialLinks);
            Assert.AreEqual(false, result.HasTags);

            // Test artist with SubsonicSimilarArtists
            var result2 = await connector.GetArtistInfoAsync("12");
            Assert.AreEqual(3, result2.SubsonicSimilarArtists.Count);
            var artist = result2.SubsonicSimilarArtists.First();
            Assert.AreEqual("9", artist.Id);
            Assert.AreEqual("PeerGynt Lobogris", artist.Name);
            Assert.AreEqual("ar-9", artist.Cover.Id);
            Assert.AreEqual(false, artist.IsSelected);
            Assert.AreEqual("4 Albums", artist.InfRow1);
            Assert.AreEqual(string.Empty, artist.InfRow2);
            Assert.AreEqual(string.Empty, artist.InfRow3);
            Assert.AreEqual(typeof(SonicStreamer.Subsonic.Data.Artist), artist.MusicObject.GetType());
            var musicObject = artist.MusicObject as SonicStreamer.Subsonic.Data.Artist;
            Assert.AreEqual("9", musicObject.Id);
            Assert.AreEqual("PeerGynt Lobogris", musicObject.Name);
            Assert.AreEqual("ar-9", musicObject.Cover.Id);
        }

        [TestMethod]
        public async Task GetAlbumTestAsync()
        {
            var connector = await GetNewConnector();
            // Curb Jaw - Cake or Death
            var result = await connector.GetAlbumAsync("22");
            Assert.AreEqual("22", result.Id);
            Assert.AreEqual("Cake or Death", result.Name);
            Assert.AreEqual("Curb Jaw", result.Artist);
            Assert.AreEqual("13", result.ArtistId);
            Assert.AreEqual("al-22", result.Cover.Id);
            Assert.AreEqual("1863", result.Duration);
            Assert.AreEqual("15", result.TrackCount);
            Assert.AreEqual("2008", result.Year);
            Assert.AreEqual(false, result.IsSelected);
            Assert.AreEqual(15, result.Tracks.Count);
            var track = result.Tracks.First();
            Assert.AreEqual("236", track.Id);
            Assert.AreEqual("Voices", track.Name);
            Assert.AreEqual("Cake or Death", track.Album);
            Assert.AreEqual("22", track.AlbumId);
            Assert.AreEqual("Curb Jaw", track.Artist);
            Assert.AreEqual("13", track.ArtistId);
            Assert.AreEqual("231", track.Cover.Id);
            Assert.AreEqual("219", track.Duration);
            Assert.AreEqual("1", track.TrackNr);
            Assert.AreEqual("128 kbit/s", track.BitRate);
            Assert.AreEqual(@"Curb Jaw\Cake or Death\01 - Voices.mp3", track.Path);
        }

        [TestMethod]
        public async Task GetAlbumWithMissingDataTestAsync()
        {
            var connector = await GetNewConnector();
            // The Dada Weatherman - EarthQuakes & Failed Mutations
            var result = await connector.GetAlbumAsync("47");
            Assert.AreEqual(string.Empty, result.Year);
            var track = result.Tracks.First();
            Assert.AreEqual(string.Empty, track.TrackNr);
        }

        [TestMethod]
        public async Task GetArtistTracksTestAsync()
        {
            var connector = await GetNewConnector();
            // The Dada Weatherman - 4 Albums
            var result = await connector.GetArtistTracksAsync("23");
            Assert.AreEqual(37, result.Count);
        }

        [TestMethod]
        public async Task GetAlbumTracksTestAsync()
        {
            var connector = await GetNewConnector();
            // The Dada Weatherman - EarthQuakes & Failed Mutations
            var results = await connector.GetAlbumTracksAsync("47");
            Assert.AreEqual(12, results.Count);
        }

        [Ignore]
        [TestMethod]
        public async Task GetTopSongsTestAsync()
        {
            // Subsonic Test subsonicDemo.Server responses are valid but empty
            // no real test data available
        }

        [TestMethod]
        public async Task GetPlaylistsTestAsync()
        {
            var connector = await GetNewConnector();
            var results = await connector.GetPlaylistsAsync();
            Assert.AreNotEqual(results.Count, 0);
            // no static test data, assume that all fields contain some string data
            var playlist = results.First();
            Assert.AreNotEqual(string.Empty, playlist.Id);
            Assert.AreNotEqual(string.Empty, playlist.Name);
            Assert.AreNotEqual(string.Empty, playlist.Duration);
            Assert.AreNotEqual(string.Empty, playlist.TrackCount);
            Assert.AreNotEqual(string.Empty, playlist.Cover.Id);
            Assert.AreEqual(false, playlist.IsSelected);

            // Check if at least one playlist has tracks
            var hasTracks = false;
            foreach (var item in results)
            {
                int count;
                if (!int.TryParse(item.TrackCount, out count) || count <= 0) continue;
                hasTracks = true;
                break;
            }
            Assert.AreEqual(hasTracks, true);

            // Only playlist are loaded, no tracks
            foreach (var item in results)
            {
                Assert.AreEqual(0, item.Tracks.Count);
            }
        }

        [TestMethod]
        public async Task GetPlaylistTracksTestAsync()
        {
            var connector = await GetNewConnector();
            var results = await connector.GetPlaylistsAsync();
            foreach (var item in results)
            {
                int count;
                if (!int.TryParse(item.TrackCount, out count) || count <= 0) continue;
                var tracks = await connector.GetPlaylistTracksAsync(item.Id);
                Assert.AreEqual(count, tracks.Count);
                return;
            }
            Assert.Fail("No playlist with tracks available");
        }

        [TestMethod]
        public async Task GetPodcastsTestAsync()
        {
            var connector = await GetNewConnector();
            var results = await connector.GetPodcastsAsync();
            Assert.AreEqual(4, results.Count);
            var podcast = results.First();
            Assert.AreEqual("0", podcast.Id);
            Assert.AreEqual("Click", podcast.Name);
            Assert.AreEqual("http://downloads.bbc.co.uk/podcasts/worldservice/digitalp/rss.xml", podcast.Source);
            Assert.AreEqual("Technological and digital news from around the world.", podcast.Description);
            Assert.AreEqual("pod-0", podcast.Cover.Id);
            Assert.AreNotEqual(string.Empty, podcast.Status);
            // Only podcasts are loaded, no episodes
            foreach (var item in results)
            {
                Assert.AreEqual(0, item.Episodes.Count);
            }
        }

        [TestMethod]
        public async Task GetPodcastEpisodesTestAsync()
        {
            var connector = await GetNewConnector();
            var results = await connector.GetPodcastEpisodesAsync("0");
            Assert.AreNotEqual(results.Count, 0);
            var episode = results.First();
            // Fixed subsonicDemo.Server data
            Assert.AreEqual("Click", episode.Album);
            Assert.AreEqual("31", episode.AlbumId);
            Assert.AreEqual("BBC World Service", episode.Artist);
            Assert.AreEqual("17", episode.ArtistId);
            // Generic subsonicDemo.Server data
            Assert.AreNotEqual(string.Empty, episode.Id);
            Assert.AreNotEqual(string.Empty, episode.Name);
            Assert.AreNotEqual(string.Empty, episode.Cover.Id);
            Assert.AreNotEqual("0", episode.Cover.Id);
            Assert.AreNotEqual(string.Empty, episode.EpisodeId);
            Assert.AreNotEqual(string.Empty, episode.Description);
            Assert.AreNotEqual(string.Empty, episode.Released);
            Assert.AreNotEqual(SonicStreamer.Subsonic.Data.PodcastEpisode.DownloadStatus.Unkown, episode.EpisodeStatus);
            Assert.AreNotEqual(string.Empty, episode.BitRate);
            // Fixed generated data
            Assert.IsTrue(episode.Path.Contains("Podcasts"));
            Assert.IsTrue(episode.Path.Contains(episode.Artist));
            Assert.IsTrue(episode.Path.Contains(episode.Album));
            Assert.IsTrue(episode.Path.Contains(episode.EpisodeId));
        }

        [TestMethod]
        public async Task GetNewestPodcastEpisodesTestAsync()
        {
            var connector = await GetNewConnector();
            var results = await connector.GetNewestPodcastEpisodesAsync();
            Assert.AreNotEqual(0, results.Count);
        }

        [Ignore]
        [TestMethod]
        public async Task DownloadEpisodeToServerTestAsync()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public async Task GetTopFolderTestAsync()
        {
            var connector = await GetNewConnector();
            var results = await connector.GetTopFolderAsync();
            Assert.AreEqual("0", results.Id);
            Assert.AreEqual("Folders", results.Name);
            Assert.AreEqual(24, results.Folders.Count);
            Assert.AreEqual(0, results.Tracks.Count);
            Assert.AreEqual(null, results.Cover.Id);
            var folder = results.Folders.First();
            Assert.AreEqual("14", folder.Id);
            Assert.AreEqual("Antigona", folder.Name);
            Assert.AreEqual(null, folder.Cover.Id);
            Assert.AreEqual(false, folder.IsSelected);
            Assert.AreEqual(0, folder.Folders.Count);
            Assert.AreEqual(0, folder.Tracks.Count);
        }

        [TestMethod]
        public async Task GetFolderContentTestAsync()
        {
            var connector = await GetNewConnector();
            var result = await connector.GetFolderContentAsync("306");
            Assert.AreEqual("306", result.Id);
            Assert.AreEqual("The Dada Weatherman", result.Name);
            Assert.AreEqual(4, result.Folders.Count);
            Assert.AreEqual(0, result.Tracks.Count);
            Assert.AreEqual(null, result.Cover.Id);
            var folder = result.Folders.First();
            Assert.AreEqual("434", folder.Id);
            Assert.AreEqual("Birthnight", folder.Name);
            Assert.AreEqual("434", folder.Cover.Id);
            Assert.AreEqual(false, folder.IsSelected);
            Assert.AreEqual(0, folder.Folders.Count);
            Assert.AreEqual(0, folder.Tracks.Count);
        }

        [TestMethod]
        public async Task GetSearchResultsTestAsync()
        {
            var connector = await GetNewConnector();
            var results = await connector.GetSearchResultsAsync("Dada");
            Assert.AreEqual(1, results.Item1.Count);
            Assert.AreEqual(4, results.Item2.Count);
            Assert.AreEqual(20, results.Item3.Count);
        }

        [TestMethod]
        public async Task GetAlbumSectionTestAsync()
        {
            var connector = await GetNewConnector();
            var results = await connector.GetAlbumSectionAsync(BaseSubsonicConnection.AlbumListType.Frequent, 12);
            Assert.AreEqual(12, results.Count);
            results = await connector.GetAlbumSectionAsync(BaseSubsonicConnection.AlbumListType.Newest, 10);
            Assert.AreEqual(10, results.Count);
            results = await connector.GetAlbumSectionAsync(BaseSubsonicConnection.AlbumListType.Random, 30);
            Assert.AreEqual(30, results.Count);
            results = await connector.GetAlbumSectionAsync(BaseSubsonicConnection.AlbumListType.Recent, 20);
            Assert.AreEqual(20, results.Count);
        }
    }
}