using Windows.Media.Playback;

namespace SonicStreamer.Subsonic.Data
{
    public class PodcastEpisode : SubsonicPlayableObject
    {
        private string _episodeId;

        public string EpisodeId
        {
            get { return _episodeId; }
            set { Set(ref _episodeId, value); }
        }

        private string _description;

        public string Description
        {
            get { return _description; }
            set { Set(ref _description, value); }
        }

        private string _released;

        public string Released
        {
            get { return _released; }
            set { Set(ref _released, value); }
        }

        private DownloadStatus _episodeStatus;

        public DownloadStatus EpisodeStatus
        {
            get { return _episodeStatus; }
            set { Set(ref _episodeStatus, value); }
        }

        public enum DownloadStatus
        {
            Completed,
            Downloading,
            Skipped,
            Unkown
        }

        public PodcastEpisode() : base()
        {
            EpisodeStatus = DownloadStatus.Unkown;
        }
    }
}