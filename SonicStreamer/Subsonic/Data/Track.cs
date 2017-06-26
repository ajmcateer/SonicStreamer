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
    }
}