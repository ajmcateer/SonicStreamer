using System.Collections.ObjectModel;

namespace SonicStreamer.Subsonic.Data
{
    public class Podcast : SubsonicMusicObject
    {
        private string _source;

        public string Source
        {
            get { return _source; }
            set { Set(ref _source, value); }
        }

        private string _description;

        public string Description
        {
            get { return _description; }
            set { Set(ref _description, value); }
        }

        private string _status;

        public string Status
        {
            get { return _status; }
            set { Set(ref _status, value); }
        }

        public ObservableCollection<PodcastEpisode> Episodes { get; private set; }

        public Podcast()
        {
            Episodes = new ObservableCollection<PodcastEpisode>();
        }
    }
}