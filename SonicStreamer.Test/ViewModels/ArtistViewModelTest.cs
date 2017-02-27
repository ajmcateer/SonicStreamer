using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using SonicStreamer.Common.System;
using SonicStreamer.Subsonic.Data;
using SonicStreamer.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace SonicStreamer.Test.ViewModels
{
    [TestClass]
    public class ArtistViewModelTest
    {
        [TestInitialize]
        public async Task InitAsync()
        {
            await SubsonicConnector.Current.SetSubsonicTestServerAsync();
        }

        [TestMethod]
        public async Task LoadDataTestAsync()
        {
            var artistVm = new ArtistsViewModel();
            await artistVm.LoadDataAsync();
            Assert.AreEqual(11, artistVm.Items.Count);
            Assert.AreEqual(2, artistVm.Items.First().Count);
            Assert.AreEqual("a", artistVm.Items.First().Key);
            var listingItem = artistVm.Items.First().First();
            Assert.AreEqual("Antígona", listingItem.Name);
            Assert.AreEqual("ar-12", listingItem.Cover.Id);
            Assert.AreEqual("12", listingItem.Id);
            Assert.AreEqual("1 Albums", listingItem.InfRow1);
            Assert.IsInstanceOfType(listingItem.MusicObject, typeof(Artist));
        }
    }
}