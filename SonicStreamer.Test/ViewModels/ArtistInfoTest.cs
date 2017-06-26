using System.Linq;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System.Threading.Tasks;
using SonicStreamer.Common.Extension;
using SonicStreamer.Common.System;
using SonicStreamer.ViewModelItems;

namespace SonicStreamer.Test.ViewModels
{
    [TestClass]
    public class ArtistInfoTest
    {
        [TestInitialize]
        public async Task InitAsync()
        {
            await SubsonicConnector.Current.SetSubsonicTestServerAsync();
        }

        [TestMethod]
        public async Task CreateArtistInfoTestAsync()
        {
            var activeServices = new ArtistInfo.ActiveServices
            {
                LastFm = true,
                MusicBrainz = true,
                Twitter = true
            };
            var firstArtistTest = await ArtistInfo.CreateAsync("23", "The Dada Weatherman", activeServices, new FakeDispatcherWrapper());
            Assert.AreEqual("The Dada Weatherman", firstArtistTest.Name);
            Assert.AreEqual("cb974849-8230-483f-869e-643528bb3565", firstArtistTest.Mbid);
            Assert.IsTrue(firstArtistTest.HasBiography);
            Assert.IsTrue(firstArtistTest.HasGeneralInfo);
            Assert.IsTrue(firstArtistTest.HasSocialLinks);
            Assert.IsTrue(firstArtistTest.HasTags);
            Assert.IsNotNull(firstArtistTest.Image);
            Assert.AreNotEqual(string.Empty, firstArtistTest.Image);
            Assert.AreEqual(3, firstArtistTest.Infos.Count);
            foreach (var info in firstArtistTest.Infos)
            {
                Assert.IsFalse(string.IsNullOrEmpty(info.Item1));
                Assert.IsFalse(string.IsNullOrEmpty(info.Item2));
            }
            Assert.AreEqual(5, firstArtistTest.LastFmSimilarArtists.Count);
            foreach (var item in firstArtistTest.LastFmSimilarArtists)
            {
                Assert.IsFalse(string.IsNullOrEmpty(item.Name));
                Assert.IsFalse(string.IsNullOrEmpty(item.LastFmUrl));
                Assert.IsFalse(string.IsNullOrEmpty(item.Cover.Uri));
            }
            Assert.AreEqual(0, firstArtistTest.SubsonicSimilarArtists.Count);
            Assert.AreEqual(6, firstArtistTest.SocialLinks.Count);
            Assert.AreEqual("ms-appx:///Assets/SocialIcons/lastfm.png", firstArtistTest.SocialLinks.First().Item1);
            Assert.AreEqual("http://www.last.fm/music/The+Dada+Weatherman", firstArtistTest.SocialLinks.First().Item2);
            Assert.AreEqual(5, firstArtistTest.Tags.Count);
            foreach (var tag in firstArtistTest.Tags)
            {
                Assert.IsFalse(string.IsNullOrEmpty(tag.Name));
                Assert.IsFalse(string.IsNullOrEmpty(tag.Uri));
            }

            // Test Artist with SubsonicSimilarArtists
            var secondArtistTest = await ArtistInfo.CreateAsync("12", "Antígona", activeServices, new FakeDispatcherWrapper());
            Assert.AreEqual(3, secondArtistTest.SubsonicSimilarArtists.Count);
            foreach (var subsonicSimilarArtist in secondArtistTest.SubsonicSimilarArtists)
            {
                Assert.IsFalse(string.IsNullOrEmpty(subsonicSimilarArtist.Id));
                Assert.IsFalse(string.IsNullOrEmpty(subsonicSimilarArtist.Name));
                Assert.IsFalse(string.IsNullOrEmpty(subsonicSimilarArtist.InfRow1));
                Assert.IsFalse(string.IsNullOrEmpty(subsonicSimilarArtist.Cover.Id));
            }
        }
    }
}