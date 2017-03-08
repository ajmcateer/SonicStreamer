using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SonicStreamer.ViewModels
{
    public interface IBaseViewModel : INotifyPropertyChanged
    {
        void Unload();
    }

    public class BaseViewModel : IBaseViewModel
    {
        protected bool IsLoaded;

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

        /// <summary>
        /// Setzt den Lade Status des ViewModels zurück
        /// </summary>
        public void Unload()
        {
            IsLoaded = false;
        }
    }
}