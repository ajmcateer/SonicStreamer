using SonicStreamer.ViewModels;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using SonicStreamer.Common.System;

namespace SonicStreamer.Test.ViewModels
{
    [TestClass]
    public class StartViewModelTest
    {
        [TestInitialize]
        public async Task InitAsync()
        {
            await SubsonicConnector.Current.SetSubsonicTestServerAsync();
        }

        [TestMethod]
        public async Task LoadDataAsyncTest()
        {
            var startVm = new StartViewModel();
            await startVm.LoadDataAsync();
        }
    }
}