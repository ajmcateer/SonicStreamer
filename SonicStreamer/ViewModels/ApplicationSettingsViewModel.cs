using SonicStreamer.Common.System;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Input;
using NUnit.Framework.Constraints;

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

        private string _trackCacheSize;

        public string TrackCacheSize
        {
            get { return _trackCacheSize; }
            set { Set(ref _trackCacheSize, value); }
        }

        private const string Homepage = "http://axelander.net";
        private const string Mail = "mailto:sonicstreamer@outlook.com";
        private const string Twitter = "http://twitter.com/SonicStreamer";
        private const string Policy = "http://axelander.net/?page_id=41";

        public async Task LoadDataAsync()
        {
            var packageVersion = Windows.ApplicationModel.Package.Current.Id.Version;
            Version = string.Format("{0}.{1} (Build {2})", packageVersion.Major.ToString(),
                packageVersion.Minor.ToString(), packageVersion.Build.ToString());

            RestoreData();
            await GetTrackCacheSize();
        }

        private async Task GetTrackCacheSize()
        {
            try
            {
                var trackFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("tracks");
                var folderSize = await GetFolderSizeAsync(trackFolder);
                TrackCacheSize = (ConvertBytesToMegabytes(folderSize)).ToString("#.000");
            }
            catch
            {
                TrackCacheSize = "0";
            }

            // Local function to get sizes for all files in all subfolders (Recursion)
            async Task<ulong> GetFolderSizeAsync(IStorageFolder folder)
            {
                var fileSizeTasks =
                    (await folder.GetFilesAsync()).Select(async file => (await file.GetBasicPropertiesAsync()).Size);
                var sizes = await Task.WhenAll(fileSizeTasks);
                var folderSize = sizes.Aggregate<ulong, ulong>(0, (current, size) => current + size);
                var tasks =
                    (from subfolder in await folder.GetFoldersAsync() select GetFolderSizeAsync(subfolder)).ToList();
                folderSize = (await Task.WhenAll(tasks)).Aggregate(folderSize,
                    (current, subFolderSize) => current + subFolderSize);
                return folderSize;
            }

            double ConvertBytesToMegabytes(ulong bytes) => (bytes / 1024f) / 1024f;
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
                TrackCacheSize = "0";
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}