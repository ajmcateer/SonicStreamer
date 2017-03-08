using SonicStreamer.Subsonic.Data;

namespace SonicStreamer.ViewModelItems
{
    public class TrackListingItem : Album
    {
        #region Properties

        private bool _isTrackListVisible;

        public bool IsTrackListVisible
        {
            get { return _isTrackListVisible; }
            set
            {
                Set(ref _isTrackListVisible, value);
                // muss außerhalb sein, damit bei der Initialisierung auch das Symbol gesetzt wird
                ExpandButtonSymbol = value ? "\uE010" : "\uE011";
            }
        }

        private string _expandButtonSymbol;

        public string ExpandButtonSymbol
        {
            get { return _expandButtonSymbol; }
            private set { Set(ref _expandButtonSymbol, value); }
        }

        #endregion

        public TrackListingItem() : base()
        {
        }

        public TrackListingItem(Album album) : this(album, false)
        {
        }

        public TrackListingItem(Album album, bool isExpand) : this()
        {
            Artist = album.Artist;
            Cover = album.Cover;
            Duration = album.Duration;
            Id = album.Id;
            Name = album.Name;
            TrackCount = album.TrackCount;
            Tracks = album.Tracks;
            Year = album.Year;
            IsTrackListVisible = isExpand;
        }

        /// <summary>
        /// Bindable Methode um die Tracks des Albums ein- oder auszublenden
        /// </summary>
        public void ExpandCollapseButtonClick()
        {
            IsTrackListVisible = !IsTrackListVisible;
        }
    }
}