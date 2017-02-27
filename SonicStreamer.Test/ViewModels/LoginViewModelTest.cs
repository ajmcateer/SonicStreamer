using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using SonicStreamer.Common.System;
using SonicStreamer.ViewModels;

namespace SonicStreamer.Test.ViewModels
{
    [TestClass]
    public class LoginViewModelTest
    {
        [TestInitialize]
        public async Task InitAsync()
        {
            await SubsonicConnector.Current.SetSubsonicTestServerAsync();
        }

        [TestMethod]
        public async Task LoginAsyncTest()
        {
            var loginVm = new LoginViewModel
            {
                ServerUrl = SubsonicConnector.Current.CurrentConnection.ServerUrl,
                User = SubsonicConnector.Current.CurrentConnection.User,
                Password = SubsonicConnector.Current.CurrentConnection.Password,
                SelectedServerType = "Subsonic"
            };
            Assert.IsTrue(await loginVm.LoginAsync());
        }

        [TestMethod]
        public async Task LogoutAsyncTest()
        {
            var loginVm = new LoginViewModel
            {
                ServerUrl = SubsonicConnector.Current.CurrentConnection.ServerUrl,
                User = SubsonicConnector.Current.CurrentConnection.User,
                Password = SubsonicConnector.Current.CurrentConnection.Password,
                SelectedServerType = "Subsonic"
            };
            Assert.IsTrue(await loginVm.LoginAsync());
            loginVm.Logout();
            Assert.AreEqual(string.Empty, loginVm.ServerUrl);
            Assert.AreEqual(string.Empty, loginVm.User);
            Assert.AreEqual(string.Empty, loginVm.Password);
            Assert.AreEqual(string.Empty, loginVm.SelectedServerType);
            Assert.IsFalse(ApplicationData.Current.RoamingSettings.Containers.ContainsKey(Constants.ContainerLogin));
            Assert.AreEqual(0, PlaybackService.Current.Playback.Items.Count);
            Assert.IsNull(SubsonicConnector.Current.CurrentConnection);
        }
    }
}
