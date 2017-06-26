using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using SonicStreamer.Common.System;
using SonicStreamer.Pages;
using SonicStreamer.Subsonic.Data;
using SonicStreamer.Subsonic.Server;
using SonicStreamer.ViewModels;

namespace SonicStreamer.ViewModelItems.Sections
{

    public class AlbumListsSection : BaseSection
    {
        #region Properties

        public ObservableCollection<AlbumList> AlbumLists { get; set; }

        #endregion

        public AlbumListsSection()
        {
            ContentTemplateResourceKey = "AlbumListsSectionTemplate";
            AlbumLists = new ObservableCollection<AlbumList>();
        }

        public AlbumListsSection(string title, int index) : this()
        {
            Title = title;
            Index = index;
        }

        public override async Task LoadDataAsync()
        {
            var albumListCreationTasks = new List<Task<AlbumList>>
            {
                AlbumList.CreateAsync(BaseSubsonicConnection.AlbumListType.Random, "random albums", 12, 0),
                AlbumList.CreateAsync(BaseSubsonicConnection.AlbumListType.Recent, "recent albums", 12, 0),
                AlbumList.CreateAsync(BaseSubsonicConnection.AlbumListType.Frequent, "frequent albums", 12, 0),
                AlbumList.CreateAsync(BaseSubsonicConnection.AlbumListType.Newest, "newest albums", 12, 0)
            };

            var orderedAlbumList = from albumList in await Task.WhenAll(albumListCreationTasks) orderby albumList.Index select albumList;
            foreach (var albumList in orderedAlbumList)
            {
                AlbumLists.Add(albumList);
            }
        }
    }

    public class AlbumList
    {
        #region Properties

        public Guid Id { get; private set; }
        public int Index { get; private set; }

        private int _maxItemCount;

        public int MaxItemCount
        {
            get { return _maxItemCount; }
            set { Set(ref _maxItemCount, value); }
        }

        private string _header;

        public string Header
        {
            get { return _header; }
            set { Set(ref _header, value); }
        }

        public ObservableCollection<Album> Albums { get; set; }

        #endregion

        public AlbumList()
        {
            Albums = new ObservableCollection<Album>();
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

        public static async Task<AlbumList> CreateAsync(BaseSubsonicConnection.AlbumListType type, string title,
            int count, int index)
        {
            var result = new AlbumList
            {
                Id = new Guid(),
                Index = index,
                Header = title,
                MaxItemCount = count
            };
            foreach (var album in await SubsonicConnector.Current.CurrentConnection.GetAlbumSectionAsync(type, count))
            {
                result.Albums.Add(album);
            }
            return result;
        }

        public void Album_ItemClick(object sender, ItemClickEventArgs e)
        {
            MainViewModel mainVm = null;
            if (ResourceLoader.Current.GetResource(ref mainVm, Constants.ViewModelMain) == false)
                mainVm = new MainViewModel();
            mainVm.MainFrame.Navigate(typeof(TrackListingPage), e.ClickedItem);
        }
    }
}
