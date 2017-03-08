using SonicStreamer.Common.System;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.UI.Xaml;

namespace SonicStreamer.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        #region Properties

        public enum ServerType
        {
            Subsonic,
            Madsonic,
            Unknown
        }

        private string _user;

        public string User
        {
            get { return _user; }
            set { Set(ref _user, value); }
        }

        private string _serverUrl;

        public string ServerUrl
        {
            get { return _serverUrl; }
            set { Set(ref _serverUrl, value); }
        }

        private string _password;

        public string Password
        {
            get { return _password; }
            set { Set(ref _password, value); }
        }

        public string EncryptedPassword
        {
            get
            {
                if (Password.StartsWith("enc:"))
                {
                    return Password;
                }
                // Passwort verschlüsseln
                var sb = new StringBuilder();
                foreach (var t in Password)
                {
                    sb.Append(Convert.ToInt32(t).ToString("x"));
                }
                return string.Format("enc:{0}", sb.ToString());
            }
        }

        private string _message;

        public string Message
        {
            get { return _message; }
            set { Set(ref _message, value); }
        }

        private string _selectedServerType;

        public string SelectedServerType
        {
            get { return _selectedServerType; }
            set { Set(ref _selectedServerType, value); }
        }

        public ObservableCollection<string> ServerTypes { get; }

        #endregion

        #region Initialization and Restoration

        public LoginViewModel()
        {
            ServerTypes = new ObservableCollection<string>();
            foreach (var item in Enum.GetNames(typeof(ServerType)))
            {
                if (item != Enum.GetName(typeof(ServerType), ServerType.Unknown))
                {
                    ServerTypes.Add(item);
                }
            }

            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                SelectedServerType = Enum.GetName(typeof(ServerType), ServerType.Subsonic);
                Message =
                    "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore " +
                    "et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. " +
                    "Stet clita kasd gubergren, no sea takimata sanctus est Lorem";
            }
        }

        /// <summary>
        /// Ermittelt die LoginDaten aus der PasswordVault und den RoamingSettings
        /// </summary>
        public void RestoreData()
        {
            var vault = new PasswordVault();
            // Abfrage rückwärts ausführen, falls fehlerhaft mehrere Credentials erstellt wurden
            foreach (var item in vault.RetrieveAll().Reverse())
            {
                var credentials = item;
                ServerUrl = credentials.Resource;
                User = credentials.UserName;
                credentials.RetrievePassword();
                Password = credentials.Password;
                break;
            }
            var roamingLoginSettings = ApplicationData.Current.RoamingSettings.CreateContainer(
                Constants.ContainerLogin, ApplicationDataCreateDisposition.Always);
            var restoredServerType = roamingLoginSettings.Values[Constants.SettingServerType] as string;
            SelectedServerType = string.IsNullOrEmpty(restoredServerType)
                ? Enum.GetName(typeof(ServerType), ServerType.Subsonic)
                : restoredServerType;
        }

        /// <summary>
        /// Setzt den Lade Status von allen im App ResourcesDictionary registrierten ViewModels zurück
        /// </summary>
        protected void UnloadAllViewModels()
        {
            try
            {
                foreach (var key in Application.Current.Resources.Keys.OfType<string>())
                {
                    var viewModel = Application.Current.Resources[key] as IBaseViewModel;
                    viewModel?.Unload();
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        #endregion

        #region Connection Handling

        /// <summary>
        /// Stellt mit den neuen Daten eine Verbindung zum Subsonic Server her
        /// </summary>
        /// <returns>true - Verbindung erfolgreich</returns>
        public async Task<bool> ConnectAsync()
        {
            if (await LoginAsync())
            {
                var vault = new PasswordVault();
                vault.Add(new PasswordCredential(ServerUrl, User, Password));

                var roamingLoginSettings =
                    ApplicationData.Current.RoamingSettings.CreateContainer(Constants.ContainerLogin,
                        ApplicationDataCreateDisposition.Always);
                roamingLoginSettings.Values[Constants.SettingServerType] = SelectedServerType;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Loggt sich mit den vorhandenen Daten auf dem Server ein
        /// </summary>
        /// <returns>true - Verbindung erfolgreich</returns>
        public async Task<bool> LoginAsync()
        {
            Tuple<bool, string> loginResult;
            Message = "Connect to server...";

            if (!ServerUrl.StartsWith("http"))
            {
                loginResult = await SubsonicConnector.Current.TryToConnect(string.Format("https://{0}", ServerUrl), User,
                    EncryptedPassword, GetSelectedServerType());
                if (loginResult.Item1 == false && loginResult.Item2.StartsWith("Error"))
                {
                    loginResult = await SubsonicConnector.Current.TryToConnect(string.Format("http://{0}", ServerUrl),
                        User, EncryptedPassword, GetSelectedServerType());
                }
            }
            else
            {
                loginResult = await SubsonicConnector.Current.TryToConnect(ServerUrl, User, EncryptedPassword,
                    GetSelectedServerType());
            }

            if (loginResult.Item1)
            {
                Message = string.Empty;
                return true;
            }
            Message = loginResult.Item2;
            return false;
        }

        /// <summary>
        /// Setzt die Login Daten zurück
        /// </summary>
        public void Logout()
        {
            ServerUrl = string.Empty;
            User = string.Empty;
            Password = string.Empty;
            Message = string.Empty;
            SelectedServerType = string.Empty;
            // alle Credentials löschen, um damit auch fehlerhaft erstellte zu löschen
            var vault = new PasswordVault();
            foreach (var item in vault.RetrieveAll())
            {
                vault.Remove(item);
            }
            if (ApplicationData.Current.RoamingSettings.Containers.ContainsKey(Constants.ContainerLogin))
            {
                ApplicationData.Current.RoamingSettings.DeleteContainer(Constants.ContainerLogin);
            }
            PlaybackService.Current.ResetPlayabck();
            UnloadAllViewModels();
            SubsonicConnector.Current.ResetConnection();
        }

        #endregion

        #region ServerType Handling

        public ServerType GetSelectedServerType()
        {
            ServerType result;
            var parseResult = Enum.TryParse(SelectedServerType, out result);
            return parseResult ? result : ServerType.Unknown;
        }

        #endregion
    }
}