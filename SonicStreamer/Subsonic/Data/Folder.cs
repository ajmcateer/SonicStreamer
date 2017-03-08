using System.Collections.ObjectModel;

namespace SonicStreamer.Subsonic.Data
{
    public class Folder : SubsonicMusicObject
    {
        #region Properties

        public ObservableCollection<Folder> Folders { get; set; }
        public ObservableCollection<Track> Tracks { get; set; }

        #endregion

        public Folder()
        {
            Folders = new ObservableCollection<Folder>();
            Tracks = new ObservableCollection<Track>();
            Cover = new CoverArt();
        }
    }
}