using System.Collections.ObjectModel;

namespace SonicStreamer.Subsonic.Data
{
    public class Artist : SubsonicMusicObject
    {
        private string _albumCount;

        public string AlbumCount
        {
            get { return _albumCount; }
            set { Set(ref _albumCount, value); }
        }

        private string _mbid;

        public string Mbid
        {
            get { return _mbid; }
            set { Set(ref _mbid, value); }
        }

        public ObservableCollection<Album> Albums { get; set; }

        public Artist()
        {
            Albums = new ObservableCollection<Album>();
        }
    }
}