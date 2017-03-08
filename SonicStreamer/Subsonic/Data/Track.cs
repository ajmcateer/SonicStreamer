using Windows.Media.Playback;

namespace SonicStreamer.Subsonic.Data
{
    public class Track : SubsonicPlayableObject
    {
        #region Properties

        private string _trackNr;

        public string TrackNr
        {
            get { return _trackNr; }
            set { Set(ref _trackNr, value); }
        }

        #endregion

        public Track() : base()
        {
        }

        public Track(MediaPlaybackItem playbackItem) : base(playbackItem)
        {
            TrackNr = playbackItem.GetDisplayProperties().MusicProperties.TrackNumber.ToString();
        }
    }
}