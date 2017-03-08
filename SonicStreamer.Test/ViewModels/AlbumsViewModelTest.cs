using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using SonicStreamer.Common.System;
using SonicStreamer.Subsonic.Data;
using SonicStreamer.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace SonicStreamer.Test.ViewModels
{
    [TestClass]
    public class AlbumsViewModelTest
    {
        [TestInitialize]
        public async Task InitAsync()
        {
            await SubsonicConnector.Current.SetSubsonicTestServerAsync();
        }

        [TestMethod]
        public async Task LoadDataTestAsync()
        {
            var albumVm = new AlbumViewModel();
            await albumVm.LoadDataAsync();

            Assert.AreEqual(20, albumVm.Items.Count);
            Assert.AreEqual(1, albumVm.Items.First().Count);
            Assert.AreEqual("2", albumVm.Items.First().Key);
            var listingItem = albumVm.Items.First().First();
            Assert.AreEqual("2005 - I Would Have Given So Much More", listingItem.Name);
            Assert.AreEqual("al-43", listingItem.Cover.Id);
            Assert.AreEqual("43", listingItem.Id);
            Assert.AreEqual("Michael Ellis", listingItem.InfRow1);
            Assert.AreEqual("18 Tracks", listingItem.InfRow2);
            Assert.AreEqual("01:18:58", listingItem.InfRow3);
            Assert.IsInstanceOfType(listingItem.MusicObject, typeof(Album));
        }
    }
}