using System;
using SonicStreamer.Common.System;
using SonicStreamer.Subsonic.Data;
using SonicStreamer.ViewModelItems;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SonicStreamer.ViewModels
{
    public abstract class AbstractListingViewModel : BaseViewModel
    {
        #region Properties

        private string _listingPageTitle;

        public string ListingPageTitle
        {
            get { return _listingPageTitle; }
            set { Set(ref _listingPageTitle, value); }
        }

        private bool _isLoading;

        [XmlIgnore]
        public bool IsLoading
        {
            get { return _isLoading; }
            set { Set(ref _isLoading, value); }
        }

        private int _loadingCount;

        [XmlIgnore]
        public int LoadingCount
        {
            get { return _loadingCount; }
            set { Set(ref _loadingCount, value); }
        }

        private int _loadedItems;

        [XmlIgnore]
        public int LoadedItems
        {
            get { return _loadedItems; }
            set { Set(ref _loadedItems, value); }
        }

        private ListViewSelectionMode _selectionMode;

        [XmlIgnore]
        public ListViewSelectionMode SelectionMode
        {
            get { return _selectionMode; }
            set
            {
                Set(ref _selectionMode, value);
                if (value == ListViewSelectionMode.Multiple)
                {
                    IsItemClickEnabled = false;
                }
                else
                {
                    IsItemClickEnabled = true;
                    IsBottomAppBarVisible = false;
                    SelectedItems.Clear();
                }
            }
        }

        private bool _isItemClickEnabled;

        [XmlIgnore]
        public bool IsItemClickEnabled
        {
            get { return _isItemClickEnabled; }
            private set { Set(ref _isItemClickEnabled, value); }
        }

        private bool _isBottomAppBarVisible;

        [XmlIgnore]
        public bool IsBottomAppBarVisible
        {
            get { return _isBottomAppBarVisible; }
            set
            {
                Set(ref _isBottomAppBarVisible, value);
                try
                {
                    var playbackViewModel =
                        Application.Current.Resources[Constants.ViewModelPlayback] as PlaybackViewModel;
                    if (playbackViewModel != null)
                        playbackViewModel.IsPlaybackPanelVisible = !_isBottomAppBarVisible;
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        [XmlArrayItem("GroupedListingItems")]
        public ObservableCollection<GroupedListingItemColletion> Items { get; set; }

        [XmlIgnore]
        public ObservableCollection<ListingItem> SelectedItems { get; set; }

        protected PlaybackViewModel PlaybackVm;
        protected PlaylistViewModel PlaylistVm;
        protected ApplicationSettingsViewModel SettingsVm;

        protected List<string> Alphabet = new List<string>
        {
            "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m",
            "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z",
            "0", "1", "2", "3", "4", "5", "6", "7", "8", "9"
        };

        #endregion

        protected AbstractListingViewModel()
        {
            Items = new ObservableCollection<GroupedListingItemColletion>();
            SelectedItems = new ObservableCollection<ListingItem>();
            IsLoaded = false;
            IsLoading = false;
            LoadedItems = 0;
            LoadingCount = 0;
        }

        /// <summary>
        /// Lädt alle Alben vom Server falls die noch nicht erfolgt ist
        /// </summary>
        public abstract Task LoadDataAsync();

        /// <summary>
        /// Setzt Standardwerte beim Laden / Wiederherstellen des ViewModels
        /// </summary>
        protected virtual void InitializeStandardValues()
        {
            Items.Clear();
            SelectedItems.Clear();
            SelectionMode = ListViewSelectionMode.None;
            IsItemClickEnabled = true;
            if (ResourceLoader.Current.GetResource(ref PlaylistVm, Constants.ViewModelPlaylist) == false)
                PlaylistVm = new PlaylistViewModel();
            if (ResourceLoader.Current.GetResource(ref PlaybackVm, Constants.ViewModelPlayback) == false)
                PlaybackVm = new PlaybackViewModel();
            if (ResourceLoader.Current.GetResource(ref SettingsVm, Constants.ViewModelApplicationSettings) == false)
                SettingsVm = new ApplicationSettingsViewModel();
        }

        #region Selection Handling

        public void ListingItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectionMode != ListViewSelectionMode.Multiple) return;
            foreach (var item in e.RemovedItems)
            {
                SelectedItems.Remove(item as ListingItem);
            }
            foreach (var item in e.AddedItems)
            {
                SelectedItems.Add(item as ListingItem);
            }
            IsBottomAppBarVisible = SelectedItems.Count > 0;
        }

        #endregion

        #region CommandBar Methods

        /// <summary>
        /// Spielt die markierten Elemente ab. Die existierende Wiedergabe wird ersetzt
        /// </summary>
        public async Task PlaySelectionAsync()
        {
            await PlaybackService.Current.AddToPlaybackAsync(await GetTracksAsync());
        }

        /// <summary>
        /// Fügt die markierten Elemente zur Wiedergabe hinzu
        /// </summary>
        public async Task AddSelectionAsync()
        {
            await PlaybackService.Current.AddToPlaybackAsync(await GetTracksAsync(), false);
        }

        /// <summary>
        /// Bindable Methode um die markierten Elemente abzuspielen. Die existierende Wiedergabe wird ersetzt
        /// </summary>
        public async void PlayClick()
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(string.Format("{0} - {1}", GetType().Name, "PlayClick"));
            await PlaySelectionAsync();
            if (SettingsVm.IsSelectionCleared)
            {
                SelectionMode = ListViewSelectionMode.None;
            }
        }

        /// <summary>
        /// Bindable Methode um die markierten Elemente zur Wiedergabe hinzuzufügen.
        /// </summary>
        public async void AddClick()
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(string.Format("{0} - {1}", GetType().Name, "AddClick"));
            await AddSelectionAsync();
            if (SettingsVm.IsSelectionCleared)
            {
                SelectionMode = ListViewSelectionMode.None;
            }
        }

        /// <summary>
        /// Fügt anhand der Auswahl die Tracks zu einer Playlist hinzu
        /// </summary>
        public async Task AddToPlaylistAsync()
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(string.Format("{0} - {1}", GetType().Name,
                "AddToPlaylistAsync"));
            var playlistVm = new PlaylistViewModel();
            ResourceLoader.Current.GetResource(ref playlistVm, Constants.ViewModelPlaylist);
            await playlistVm.AddTracksToPlaylistAsync(await GetTracksAsync());
            if (SettingsVm.IsSelectionCleared)
            {
                SelectionMode = ListViewSelectionMode.None;
            }
        }

        /// <summary>
        /// Bindable Methode um die ausgewählten Tracks herunterzuladen
        /// </summary>
        public async void DownloadTracksClick()
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(string.Format("{0} - {1}", GetType().Name,
                "DownloadTracksClick"));
            var downloadTasks = (from item in await GetTracksAsync() select item.DownloadAsync()).ToList();
            if (SettingsVm.IsSelectionCleared)
            {
                SelectionMode = ListViewSelectionMode.None;
            }
            await Task.WhenAll(downloadTasks);
        }

        /// <summary>
        /// Ermittelt alle Tracks von den markierten Objekten und gibt diese zurück
        /// </summary>
        protected async Task<List<Track>> GetTracksAsync()
        {
            var results = new List<Track>();
            // Tracks asynchron ermitteln
            var trackDetermination = SelectedItems.Select(item => item.GetTracksAsync()).ToList();

            // Auf Ergebnisse warten und zur Rückgabeliste hinzufügen
            foreach (var list in await Task.WhenAll(trackDetermination))
            {
                results.AddRange(list);
            }

            return results;
        }

        #endregion
    }
}