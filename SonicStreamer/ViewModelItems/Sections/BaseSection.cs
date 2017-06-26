using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SonicStreamer.ViewModelItems.Sections
{
    public abstract class BaseSection : INotifyPropertyChanged
    {
        #region Properties

        public Guid Id { get; protected set; }

        private int _index;
        public int Index
        {
            get { return _index; }
            set { Set(ref _index, value); }
        }

        private string _title;

        public string Title
        {
            get { return _title; }
            set { Set(ref _title, value); }
        }

        public string ContentTemplateResourceKey { get; protected set; }

        #endregion

        protected BaseSection()
        {
            Id = new Guid();
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

        public abstract Task LoadDataAsync();
    }
}