using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using SonicStreamer.Common.System;
using SonicStreamer.Pages;
using SonicStreamer.Subsonic.Data;
using SonicStreamer.Subsonic.Server;
using SonicStreamer.ViewModels;

namespace SonicStreamer.ViewModelItems.Sections
{
    public class PinnedAlbumsSection : BaseSection
    {
        #region Properties

        public ObservableCollection<ListingItem> Albums { get; set; }

        private readonly List<string> _albumIds;


        #endregion

        public PinnedAlbumsSection()
        {
            ContentTemplateResourceKey = "PinnedAlbumsSectionTemplate";
            Albums = new ObservableCollection<ListingItem>();
            _albumIds = new List<string>();
        }

        public PinnedAlbumsSection(string title, IEnumerable<string> albumIds, int index) : this()
        {
            Index = index;
            Title = title;
            _albumIds = albumIds.ToList();
        }

        public override async Task LoadDataAsync()
        {
            var loadTasks = _albumIds.Select(albumId => SubsonicConnector.Current.CurrentConnection.GetAlbumAsync(albumId)).ToList();
            Albums.Clear();
            foreach (var album in await Task.WhenAll(loadTasks))
            {
                Albums.Add(new ListingItem(album));
            }
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
