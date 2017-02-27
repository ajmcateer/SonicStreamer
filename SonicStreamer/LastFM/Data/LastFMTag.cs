using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SonicStreamer.LastFM.Data
{
    public class LastFmTag : INotifyPropertyChanged
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set { Set(ref _name, value); }
        }

        private string _uri;

        public string Uri
        {
            get { return _uri; }
            set { Set(ref _uri, value); }
        }

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