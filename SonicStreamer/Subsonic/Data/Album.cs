using System.Collections.ObjectModel;

namespace SonicStreamer.Subsonic.Data
{
    public class Album : SubsonicMusicObject
    {
        #region Properties

        private string _duration;

        public string Duration
        {
            get { return _duration; }
            set { Set(ref _duration, value); }
        }

        private string _trackCount;

        public string TrackCount
        {
            get { return _trackCount; }
            set { Set(ref _trackCount, value); }
        }

        private string _artist;

        public string Artist
        {
            get { return _artist; }
            set { Set(ref _artist, value); }
        }

        private string _artistId;

        public string ArtistId
        {
            get { return _artistId; }
            set { Set(ref _artistId, value); }
        }

        private string _year;

        public string Year
        {
            get { return _year; }
            set { Set(ref _year, value); }
        }

        public ObservableCollection<Track> Tracks { get; set; }

        #endregion

        public Album()
        {
            Tracks = new ObservableCollection<Track>();
        }
    }
}