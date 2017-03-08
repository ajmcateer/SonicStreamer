using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace SonicStreamer.LastFM.Data
{
    public class LastFmImage
    {
        private string _uri;

        [XmlAttribute]
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

        public LastFmImage()
        {
        }

        public LastFmImage(string uri)
        {
            Uri = uri;
        }
    }
}