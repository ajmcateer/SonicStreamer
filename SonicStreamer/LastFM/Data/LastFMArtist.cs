using System.Collections.ObjectModel;

namespace SonicStreamer.LastFM.Data
{
    public class LastFmArtist : LastFmObject
    {
        #region Properties

        private string _biography;

        public string Biography
        {
            get { return _biography; }
            set { Set(ref _biography, value); }
        }

        public ObservableCollection<LastFmObject> SimilarArtists { get; set; }

        #endregion

        public LastFmArtist()
        {
            Tags = new ObservableCollection<LastFmTag>();
            SimilarArtists = new ObservableCollection<LastFmObject>();
        }
    }
}