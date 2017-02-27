using System;
using System.Collections.ObjectModel;

namespace SonicStreamer.Subsonic.Data
{
    /// <summary>
    /// Stellt eine auf Subsonic abgelegte Playlist dar
    /// </summary>
    public class Playlist : SubsonicMusicObject
    {
        #region Properties

        private string _comment;

        public string Comment
        {
            get { return _comment; }
            set { Set(ref _comment, value); }
        }

        private string _trackCount;

        public string TrackCount
        {
            get { return _trackCount; }
            set { Set(ref _trackCount, value); }
        }

        private string _duration;

        public string Duration
        {
            get { return _duration; }
            set
            {
                Set(ref _duration, value);


                if (value == _duration) return;
                _duration = value;
                NotifyPropertyChanged("Duration");
                NotifyPropertyChanged("DurationTime");
                NotifyPropertyChanged("DurationOutput");
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

        public ObservableCollection<Track> Tracks { get; set; }

        #endregion

        public Playlist()
        {
            Tracks = new ObservableCollection<Track>();
        }
    }
}