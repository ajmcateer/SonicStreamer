namespace SonicStreamer.Subsonic.Data
{
    public class SubsonicMusicObject : SubsonicObject
    {
        private CoverArt _cover;

        public CoverArt Cover
        {
            get { return _cover; }
            set { Set(ref _cover, value); }
        }

        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set { Set(ref _isSelected, value); }
        }
    }
}