using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SonicStreamer.LastFM.Data
{
    public class LastFmObject : INotifyPropertyChanged
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set { Set(ref _name, value); }
        }

        private string _musicBrainzId;

        public string MusicBrainzId
        {
            get { return _musicBrainzId; }
            set { Set(ref _musicBrainzId, value); }
        }

        private string _lastFmUrl;

        public string LastFmUrl
        {
            get { return _lastFmUrl; }
            set { Set(ref _lastFmUrl, value); }
        }

        private LastFmImage _cover;

        public LastFmImage Cover
        {
            get { return _cover; }
            set { Set(ref _cover, value); }
        }

        public ObservableCollection<LastFmTag> Tags { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool Set<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }
            field = value;
            NotifyPropertyChanged(propertyName);
            return true;
        }
    }
}