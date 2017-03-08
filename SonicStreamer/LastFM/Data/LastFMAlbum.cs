namespace SonicStreamer.LastFM.Data
{
    public class LastFmAlbum : LastFmObject
    {
        private string _releaseDate;

        public string ReleaseDate
        {
            get { return _releaseDate; }
            set { Set(ref _releaseDate, value); }
        }
    }
}