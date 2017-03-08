using SonicStreamer.Common.System;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.ApplicationModel.Core;
using Windows.Media.Playback;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Core;
using SonicStreamer.Common.Extension;

namespace SonicStreamer.Subsonic.Data
{
    public class SubsonicPlayableObject : SubsonicMusicObject
    {
        protected enum PlayableObjectStatus
        {
            Online,
            Downloading,
            Offline
        }

        private string _artistId;

        public string ArtistId
        {
            get { return _artistId; }
            set { Set(ref _artistId, value); }
        }

        private string _artist;

        public string Artist
        {
            get { return _artist; }
            set { Set(ref _artist, value); }
        }

        private string _albumId;

        public string AlbumId
        {
            get { return _albumId; }
            set { Set(ref _albumId, value); }
        }

        private string _album;

        public string Album
        {
            get { return _album; }
            set { Set(ref _album, value); }
        }

        private string _duration;

        public string Duration
        {
            get { return _duration; }
            set
            {
                Set(ref _duration, value);
                NotifyPropertyChanged("DurationTime");
                NotifyPropertyChanged("DurationString");
            }
        }

        public TimeSpan DurationTime
        {
            get
            {
                int trackDuration;
                var timeSpan = new TimeSpan();
                if (int.TryParse(_duration, out trackDuration))
                {
                    timeSpan = new TimeSpan(0, 0, trackDuration);
                }
                return timeSpan;
            }
        }

        public string DurationOutput
        {
            get
            {
                int trackDuration;
                if (!int.TryParse(_duration, out trackDuration)) return "0:00";
                var timeSpan = new TimeSpan(0, 0, trackDuration);
                return timeSpan.Hours == 0 ? timeSpan.ToString(@"mm\:ss") : timeSpan.ToString();
            }
        }

        private string _bitRate;

        public string BitRate
        {
            get { return _bitRate; }
            set { Set(ref _bitRate, value); }
        }

        private string _year;

        public string Year
        {
            get { return _year; }
            set { Set(ref _year, value); }
        }

        private string _path;

        public string Path
        {
            get { return _path; }
            set { Set(ref _path, value); }
        }

        private PlayableObjectStatus _status;

        protected PlayableObjectStatus Status
        {
            get { return _status; }
            set
            {
                Set(ref _status, value);
                NotifyPropertyChanged("StatusOutput");
            }
        }

        [XmlIgnore]
        public string StatusOutput
        {
            get
            {
                switch (Status)
                {
                    case PlayableObjectStatus.Online:
                        return "";
                    case PlayableObjectStatus.Downloading:
                        return "";
                    case PlayableObjectStatus.Offline:
                        return "";
                    default:
                        return "";
                }
            }
        }
        
        private IDispatcherWrapper _dispatcherWrapper;

        public SubsonicPlayableObject()
        {
            Status = PlayableObjectStatus.Online;
        }

        public SubsonicPlayableObject(MediaPlaybackItem playbackItem) : this()
        {
            Id = playbackItem.Source.CustomProperties[Constants.PlaybackTrackId] as string;
            ArtistId = playbackItem.Source.CustomProperties[Constants.PlaybackArtistId] as string;
            Cover = playbackItem.Source.CustomProperties[Constants.PlaybackCover] as CoverArt;
            Duration = playbackItem.Source.CustomProperties[Constants.PlaybackDuration] as string;
            Name = playbackItem.GetDisplayProperties().MusicProperties.Title;
            Artist = playbackItem.GetDisplayProperties().MusicProperties.Artist;
            Album = playbackItem.GetDisplayProperties().MusicProperties.AlbumTitle;
        }

        /// <summary>
        /// Baut die Uri zumsammen, um das <see cref="SubsonicPlayableObject"/> zu streamen
        /// </summary>
        /// <returns>
        /// Uri Adresse zum Streamen des <see cref="SubsonicPlayableObject"/>. 
        /// Hat das <see cref="SubsonicPlayableObject"/> keine ID wird null zurückgegeben.
        /// </returns>
        public Uri GetStreamUri()
        {
            if (Id == null) return null;
            var param = new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>("id", Id)};
            return new Uri(SubsonicConnector.Current.CurrentConnection.GetApiMethodUri("stream", param));
        }

        /// <summary>
        /// Gibt die Uri des <see cref="SubsonicPlayableObject"/> zum Streamen oder für die lokale Wiedergabe zurück
        /// </summary>
        public async Task<Uri> GetSource()
        {
            await CheckLocalFileAsync();
            if (Status != PlayableObjectStatus.Offline) return GetStreamUri();
            var result = GetLocalFilePath();
            return result != null ? new Uri(result) : GetStreamUri();
        }

        #region Cache Handling

        /// <summary>
        /// Prüft, ob das <see cref="SubsonicPlayableObject"/> im LocalFolder vorhanden ist.
        /// </summary>
        public void CheckLocalFile()
        {
            try
            {
                _dispatcherWrapper = new DispatcherWrapper(CoreApplication.MainView.CoreWindow.Dispatcher);
            }
            catch (System.Exception)
            {
                _dispatcherWrapper = new FakeDispatcherWrapper();
            }
            var startResult =
                _dispatcherWrapper.RunAsync(
                    async () =>
                    {
                        Status = await IsLocalFileAvailable()
                            ? PlayableObjectStatus.Offline
                            : PlayableObjectStatus.Online;
                    });
        }

        /// <summary>
        /// Prüft, ob das <see cref="SubsonicPlayableObject"/> im LocalFolder vorhanden ist.
        /// </summary>
        public async Task CheckLocalFileAsync()
        {
            Status = await IsLocalFileAvailable() ? PlayableObjectStatus.Offline : PlayableObjectStatus.Online;
        }

        private async Task<bool> IsLocalFileAvailable()
        {
            try
            {
                await StorageFile.GetFileFromPathAsync(GetLocalFilePath());
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gibt den kompletten Pfad für die lokalen <see cref="SubsonicPlayableObject"/> Dateien zurück
        /// </summary>
        private string GetLocalFilePath()
        {
            return (string.IsNullOrEmpty(Path))
                ? null
                : string.Format("{0}\\tracks\\{1}", ApplicationData.Current.LocalFolder.Path, Path);
        }

        /// <summary>
        /// Lädt das <see cref="SubsonicPlayableObject"/> in den LocalFolder herunter. Die Verzeichnisstruktur vom Server wird dabei übernommen
        /// </summary>
        public async Task DownloadAsync()
        {
            var downloadUri = GetDownloadUri();
            if (downloadUri != null)
            {
                try
                {
                    // Zielpfad + Dateinamen ermitteln
                    var fullPath = GetLocalFilePath();
                    var targetFileName = System.IO.Path.GetFileName(fullPath);

                    // Ziel-StorageFile erstellen
                    var targetFolder = await GetDestinationFolderAsync();
                    var targetFile = await targetFolder.CreateFileAsync(targetFileName,
                        CreationCollisionOption.ReplaceExisting);

                    // Download erstellen
                    var downloader = new BackgroundDownloader();
                    var download = downloader.CreateDownload(downloadUri, targetFile);
                    Status = PlayableObjectStatus.Downloading;

                    // Prüfen, ob es den Download schon gibt
                    foreach (var item in await BackgroundDownloader.GetCurrentDownloadsAsync())
                    {
                        if (download.RequestedUri != item.RequestedUri) continue;
                        await item.AttachAsync();
                        CheckLocalFile();
                        return;
                    }

                    // Starten und Prozess überwachen
                    await download.StartAsync();
                    CheckLocalFile();
                }
                catch (System.Exception)
                {
                    // ignored
                }
            }
        }

        /// <summary>
        /// Baut die Uri zumsammen, um ein <see cref="SubsonicPlayableObject"/> zu downloaden
        /// </summary>
        private Uri GetDownloadUri()
        {
            if (Id == null) return null;
            var param = new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>("id", Id)};
            return new Uri(SubsonicConnector.Current.CurrentConnection.GetApiMethodUri("download", param));
        }

        /// <summary>
        /// Übernimmt die Verzeichnisstruktur des <see cref="SubsonicPlayableObject"/> auf dem Server, 
        /// erstellt die entsprechenden Ornder im LocalFolder und gibt den Zielordner wo das 
        /// <see cref="SubsonicPlayableObject"/> abgelegt werden soll zurück
        /// </summary>
        private async Task<StorageFolder> GetDestinationFolderAsync()
        {
            var tracksFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("tracks",
                CreationCollisionOption.OpenIfExists);
            var subFolders = Path.Split('\\');

            var nextFolder = tracksFolder;
            for (var i = 0; i < subFolders.Length - 1; i++)
            {
                nextFolder = await nextFolder.CreateFolderAsync(subFolders[i], CreationCollisionOption.OpenIfExists);
            }
            return nextFolder;
        }

        #endregion
    }
}