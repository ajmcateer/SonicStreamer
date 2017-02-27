using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using SonicStreamer.Common.System;
using SonicStreamer.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace SonicStreamer.Test.ViewModels
{
    [TestClass]
    public class FolderViewModelTest
    {
        [TestInitialize]
        public async Task InitAsync()
        {
            await SubsonicConnector.Current.SetSubsonicTestServerAsync();
        }

        [TestMethod]
        public async Task LoadDataTestAsync()
        {
            var folderVm = new FolderViewModel();
            await folderVm.LoadDataAsync();
            Assert.AreEqual(11, folderVm.SubFolders.Count);
            Assert.AreEqual("a", folderVm.SubFolders.First().Key);
            var folder = folderVm.Current.Folders.First();
            Assert.AreEqual(folder, folderVm.SubFolders.First().First());
            Assert.AreEqual("14", folder.Id);
            Assert.AreEqual("Antigona", folder.Name);
            Assert.AreEqual(0, folderVm.History.Count);
            Assert.AreEqual(0, folderVm.SelectedFolders.Count);
            Assert.AreEqual(0, folderVm.SelectedTracks.Count);
        }

        [TestMethod]
        public async Task NavigationTestAsync()
        {
            var folderVm = new FolderViewModel();
            await folderVm.LoadDataAsync();
            var folder = (from f in folderVm.Current.Folders
                where f.Id == "306"
                select f).FirstOrDefault();
            await folderVm.Navigate(folder);
            Assert.AreEqual(folder.Id, folderVm.Current.Id);
            Assert.AreEqual("The Dada Weatherman", folderVm.Current.Name);
            Assert.AreEqual("306", folderVm.Current.Id);
            Assert.AreEqual(4, folderVm.Current.Folders.Count);
            Assert.AreEqual(0, folderVm.Current.Tracks.Count);

            await folderVm.Navigate(folderVm.Current.Folders.First());
            Assert.AreEqual(folder.Id, folderVm.History.Last().Id);
            Assert.AreEqual("Birthnight", folderVm.Current.Name);
            Assert.AreEqual("434", folderVm.Current.Id);
            Assert.AreEqual(0, folderVm.Current.Folders.Count);
            Assert.AreEqual(5, folderVm.Current.Tracks.Count);
        }
    }
}