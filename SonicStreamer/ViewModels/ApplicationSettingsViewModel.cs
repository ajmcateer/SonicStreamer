using SonicStreamer.Common.System;
using System;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Input;

namespace SonicStreamer.ViewModels
{
    public class ApplicationSettingsViewModel : BaseViewModel
    {
        /// <summary>
        /// True - Nach Auswahl einer Aktion (z.B. Play) wird die Auswahlliste zurückgesetzt
        /// </summary>
        private bool _isSelectionCleared;

        public bool IsSelectionCleared
        {
            get { return _isSelectionCleared; }
            set
            {
                Set(ref _isSelectionCleared, value);
                ApplicationData.Current.RoamingSettings.Values["SelectionCleared"] = value;
            }
        }

        private bool _isScrobbleActivated;

        public bool IsScrobbleActivated
        {
            get { return _isScrobbleActivated; }
            set
            {
                Set(ref _isScrobbleActivated, value);
                ApplicationData.Current.RoamingSettings.Values["ScrobbleActivated"] = value;
            }
        }

        private string _version;

        public string Version
        {
            get { return _version; }
            set { Set(ref _version, value); }
        }

        private const string Homepage = "http://axelander.net";
        private const string Mail = "mailto:sonicstreamer@outlook.com";
        private const string Twitter = "http://twitter.com/SonicStreamer";
        private const string Policy = "http://axelander.net/?page_id=41";

        public ApplicationSettingsViewModel()
        {
            var packageVersion = Windows.ApplicationModel.Package.Current.Id.Version;
            Version = string.Format("{0}.{1} (Build {2})", packageVersion.Major.ToString(),
                packageVersion.Minor.ToString(), packageVersion.Build.ToString());

            RestoreData();
        }

        /// <summary>
        /// Stellt die Einstellungen aus den Roaming Settings wieder her
        /// </summary>
        public void RestoreData()
        {
            bool? boolValue;

            boolValue = ApplicationData.Current.RoamingSettings.Values["SelectionCleared"] as bool?;
            IsSelectionCleared = (boolValue != false);

            boolValue = ApplicationData.Current.RoamingSettings.Values["ScrobbleActivated"] as bool?;
            IsScrobbleActivated = (boolValue != false);
        }

        #region Tapped Methods

        public async void HomepageTextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var webAddress = new Uri(Homepage);
            await Windows.System.Launcher.LaunchUriAsync(webAddress);
        }

        public async void MailTextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var webAddress = new Uri(Mail);
            await Windows.System.Launcher.LaunchUriAsync(webAddress);
        }

        public async void PolicyTextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var webAddress = new Uri(Policy);
            await Windows.System.Launcher.LaunchUriAsync(webAddress);
        }

        #endregion

        public async void ClearTrackCacheClick()
        {
            var messageDialog =
                new MessageDialog("Warning: This action will stop the current playback and delete all local tracks. " +
                                  "Do you stil want to continue?");
            messageDialog.Commands.Add(new UICommand("Delete",
                new UICommandInvokedHandler(DeleteTrackCacheCommandHandler)));
            messageDialog.Commands.Add(new UICommand("Cancel"));
            messageDialog.DefaultCommandIndex = 1;
            messageDialog.CancelCommandIndex = 1;
            await messageDialog.ShowAsync();
        }

        private async void DeleteTrackCacheCommandHandler(IUICommand command)
        {
            var playbackVm = new PlaybackViewModel();
            ResourceLoader.Current.GetResource(ref playbackVm, Constants.ViewModelPlayback);
            playbackVm.Clear();
            try
            {
                var trackFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("tracks");
                await trackFolder.DeleteAsync();
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}