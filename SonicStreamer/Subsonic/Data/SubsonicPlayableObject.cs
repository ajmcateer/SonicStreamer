using SonicStreamer.Common.System;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;

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

        public SubsonicPlayableObject()
        {
            Status = PlayableObjectStatus.Online;
        }

        /// <summary>
        /// Creates and returns an <see cref="Uri"/> to stream the <see cref="SubsonicPlayableObject"/>.
        /// If the <see cref="SubsonicPlayableObject"/> has no Id it returns null.
        /// </summary>
        public Uri GetStreamUri()
        {
            if (Id == null) return null;
            var param = new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>("id", Id)};
            return new Uri(SubsonicConnector.Current.CurrentConnection.GetApiMethodUri("stream", param));
        }

        /// <summary>
        /// Checks if the Id is listed in the Cache Pool and sets the <see cref="Status"/> property
        /// </summary>
        public void SetStatus()
        {
            if (!PlaybackService.Current.CachedPlayableObjects.TryGetValue(Id, out string path))
                Status = PlayableObjectStatus.Online;
            if (path == Path)
            {
                Status = PlayableObjectStatus.Offline;
            }
            else
            {
                // Inconsistent Data - Either File was moved Id has been changed on server
                PlaybackService.Current.CachedPlayableObjects.Remove(Id);
                Status = PlayableObjectStatus.Online;
            }
            // TODO Check if Files really exists in background
        }

        /// <summary>
        /// Checks if the local file still really exists
        /// </summary>
        public async Task CheckStatusAsync()
        {
            if (Status == PlayableObjectStatus.Online) return;
            if (await IsLocalFileAvailableAsync()) return;
            PlaybackService.Current.CachedPlayableObjects.Remove(Id);
            Status = PlayableObjectStatus.Online;
        }

        /// <summary>
        /// Gibt die Uri des <see cref="SubsonicPlayableObject"/> zum Streamen oder für die lokale Wiedergabe zurück
        /// </summary>
        public Uri GetSource()
        {
            return Status == PlayableObjectStatus.Offline ? new Uri(GetLocalFilePath()) : GetStreamUri();
        }

        #region Cache Handling

        private async Task<bool> IsLocalFileAvailableAsync()
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
                        await CheckDownloadedFileAsync();
                        return;
                    }

                    // Starten und Prozess überwachen
                    await download.StartAsync();
                    await CheckDownloadedFileAsync();
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
            var tracksFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(Constants.TrackCacheFolder,
                CreationCollisionOption.OpenIfExists);
            var subFolders = Path.Split('\\');

            var nextFolder = tracksFolder;
            for (var i = 0; i < subFolders.Length - 1; i++)
            {
                nextFolder = await nextFolder.CreateFolderAsync(subFolders[i], CreationCollisionOption.OpenIfExists);
            }
            return nextFolder;
        }

        /// <summary>
        /// Checks if the Download was successfully and adds Id to central Cached Pool
        /// </summary>
        private async Task CheckDownloadedFileAsync()
        {
            if (!await IsLocalFileAvailableAsync()) return;
            try
            {
                PlaybackService.Current.CachedPlayableObjects.Add(Id, Path);
            }
            catch (ArgumentException)
            {
                // Cached Id already exists, update Path in Cache Pool
                PlaybackService.Current.CachedPlayableObjects[Id] = Path;
            }
            Status = PlayableObjectStatus.Offline;
        }

        #endregion
    }
}