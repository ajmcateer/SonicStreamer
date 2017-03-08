using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SonicStreamer.Subsonic.Data
{
    public class SubsonicObject : INotifyPropertyChanged
    {
        private string _id;

        public string Id
        {
            get { return _id; }
            set { Set(ref _id, value); }
        }

        private string _name;

        public string Name
        {
            get { return _name; }
            set { Set(ref _name, value); }
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}