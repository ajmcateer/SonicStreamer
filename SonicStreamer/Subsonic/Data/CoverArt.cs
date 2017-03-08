using SonicStreamer.Common.System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace SonicStreamer.Subsonic.Data
{
    public class CoverArt : INotifyPropertyChanged
    {
        #region Properties

        private string _id;

        [XmlAttribute]
        public string Id
        {
            get { return _id; }
            set
            {
                Set(ref _id, value);
                SetCoverUri();
            }
        }

        private string _uri;

        [XmlIgnore]
        public string Uri
        {
            get { return _uri; }
            private set { Set(ref _uri, value); }
        }

        #endregion

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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public CoverArt()
        {
        }

        public CoverArt(string id) : this()
        {
            Id = id;
        }

        /// <summary>
        /// Baut anhand der <see cref="Id"/> die Uri für das <see cref="BitmapImage"/> zusammen
        /// </summary>
        private void SetCoverUri()
        {
            if (!string.IsNullOrEmpty(Id))
            {
                try
                {
                    var param = new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>("id", Id)};
                    Uri = SubsonicConnector.Current.CurrentConnection.GetApiMethodUri("getCoverArt", param);
                }
                catch (System.Exception)
                {
                    Uri = string.Empty;
                }
            }
            else
            {
                _id = string.Empty;
                Uri = string.Empty;
            }
        }
    }
}