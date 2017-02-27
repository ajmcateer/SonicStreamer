using SonicStreamer.Common.System;
using SonicStreamer.SampleData;
using SonicStreamer.Subsonic.Data;
using SonicStreamer.ViewModelItems;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace SonicStreamer.ViewModels
{
    public class SearchViewModel : BaseViewModel, IViewModelSerializable
    {
        #region Properties

        private string _searchString;

        public string SearchString
        {
            get { return _searchString; }
            set { Set(ref _searchString, value); }
        }

        private ListViewSelectionMode _selectionMode;

        [XmlIgnore]
        public ListViewSelectionMode SelectionMode
        {
            get { return _selectionMode; }
            set
            {
                Set(ref _selectionMode, value);
                if (_selectionMode == ListViewSelectionMode.Multiple)
                {
                    IsItemClickEnabled = false;
                }
                else
                {
                    IsItemClickEnabled = true;
                    IsBottomAppBarVisible = false;
                    SelectedArtists.Clear();
                    SelectedAlbums.Clear();
                    SelectedTracks.Clear();
                    // Workaround, da SelectedItems kann nicht gebunden werden kann
                    TracksSelectionMode = ListViewSelectionMode.None;
                    TracksSelectionMode = ListViewSelectionMode.Multiple;
                }
            }
        }

        private ListViewSelectionMode _tracksSelectionMode;

        public ListViewSelectionMode TracksSelectionMode
        {
            get { return _tracksSelectionMode; }
            set { Set(ref _tracksSelectionMode, value); }
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
                        playbackViewModel.IsPlaybackPanelVisible = !value;
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        #region Result Visibility Properties

        private bool _isResultViewVisible;

        public bool IsResultViewVisible
        {
            get { return _isResultViewVisible; }
            set { Set(ref _isResultViewVisible, value); }
        }

        private bool _isArtistResultViewVisible;

        public bool IsArtistResultViewVisible
        {
            get { return _isArtistResultViewVisible; }
            set
            {
                Set(ref _isArtistResultViewVisible, value);
                IsNoArtistResultLabelVisible = !_isArtistResultViewVisible;
            }
        }

        private bool _isNoArtistResultLabelVisible;

        public bool IsNoArtistResultLabelVisible
        {
            get { return _isNoArtistResultLabelVisible; }
            private set { Set(ref _isNoArtistResultLabelVisible, value); }
        }

        private bool _isAlbumResultViewVisible;

        public bool IsAlbumResultViewVisible
        {
            get { return _isAlbumResultViewVisible; }
            set
            {
                Set(ref _isAlbumResultViewVisible, value);
                IsNoAlbumResultLabelVisible = !_isAlbumResultViewVisible;
            }
        }

        private bool _isNoAlbumResultLabelVisible;

        public bool IsNoAlbumResultLabelVisible
        {
            get { return _isNoAlbumResultLabelVisible; }
            private set { Set(ref _isNoAlbumResultLabelVisible, value); }
        }

        private bool _isTrackResultViewVisible;

        public bool IsTrackResultViewVisible
        {
            get { return _isTrackResultViewVisible; }
            set
            {
                Set(ref _isTrackResultViewVisible, value);
                IsNoTrackResultLabelVisible = !value;
            }
        }

        private bool _isNoTrackResultLabelVisible;

        public bool IsNoTrackResultLabelVisible
        {
            get { return _isNoTrackResultLabelVisible; }
            private set { Set(ref _isNoTrackResultLabelVisible, value); }
        }

        #endregion

        public ObservableCollection<ListingItem> ResultArtist { get; set; }
        public ObservableCollection<ListingItem> ResultAlbum { get; set; }
        public ObservableCollection<Track> ResultTrack { get; set; }

        [XmlIgnore]
        public ObservableCollection<ListingItem> SelectedArtists { get; set; }

        [XmlIgnore]
        public ObservableCollection<ListingItem> SelectedAlbums { get; set; }

        [XmlIgnore]
        public ObservableCollection<Track> SelectedTracks { get; set; }

        protected PlaybackViewModel PlaybackVm;
        protected PlaylistViewModel PlaylistVm;
        protected ApplicationSettingsViewModel SettingsVm;

        #endregion

        #region Initialization & Restoration

        public SearchViewModel()
        {
            ResultArtist = new ObservableCollection<ListingItem>();
            ResultAlbum = new ObservableCollection<ListingItem>();
            ResultTrack = new ObservableCollection<Track>();
            SelectedArtists = new ObservableCollection<ListingItem>();
            SelectedAlbums = new ObservableCollection<ListingItem>();
            SelectedTracks = new ObservableCollection<Track>();
            IsItemClickEnabled = true;
            TracksSelectionMode = ListViewSelectionMode.Multiple;

            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                SearchString = "Test Search";
                IsResultViewVisible = true;
                IsArtistResultViewVisible = true;
                foreach (var artist in SampleDataCreator.Current.CreateArtists())
                {
                    ResultArtist.Add(new ListingItem(artist));
                }
                ResultArtist.ElementAt(1).Name = "Artist with very long name";
                foreach (var album in SampleDataCreator.Current.CreateAlbums())
                {
                    ResultAlbum.Add(new ListingItem(album));
                }
                foreach (var track in SampleDataCreator.Current.CreateTracks())
                {
                    ResultTrack.Add(track);
                }
            }
        }

        public void LoadData()
        {
            ResultArtist.Clear();
            ResultAlbum.Clear();
            ResultTrack.Clear();
            SelectedArtists.Clear();
            SelectedAlbums.Clear();
            SelectedTracks.Clear();
            SelectionMode = ListViewSelectionMode.None;
            IsItemClickEnabled = true;
            IsResultViewVisible = false;
            IsArtistResultViewVisible = false;
            IsAlbumResultViewVisible = false;
            IsTrackResultViewVisible = false;
            if (ResourceLoader.Current.GetResource(ref PlaylistVm, Constants.ViewModelPlaylist) == false)
                PlaylistVm = new PlaylistViewModel();
            if (ResourceLoader.Current.GetResource(ref PlaybackVm, Constants.ViewModelPlayback) == false)
                PlaybackVm = new PlaybackViewModel();
            if (ResourceLoader.Current.GetResource(ref SettingsVm, Constants.ViewModelApplicationSettings) == false)
                SettingsVm = new ApplicationSettingsViewModel();
        }

        public async Task SaveViewModelAsync(string savename)
        {
            try
            {
                var saveFolder = ApplicationData.Current.LocalFolder;
                var saveFile = await saveFolder.CreateFileAsync(savename + ".xml",
                    CreationCollisionOption.ReplaceExisting);
                using (var saveStream = await saveFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var serializer = new XmlSerializer(typeof(SearchViewModel));
                    serializer.Serialize(saveStream.AsStreamForWrite(), this);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public async Task RestoreViewModelAsync(string loadname)
        {
            try
            {
                var loadFolder = ApplicationData.Current.LocalFolder;
                var loadFile = await loadFolder.GetFileAsync(loadname + ".xml");
                using (var loadStream = await loadFile.OpenAsync(FileAccessMode.Read))
                {
                    var serializer = new XmlSerializer(typeof(SearchViewModel));
                    var newVm = serializer.Deserialize(loadStream.AsStreamForRead()) as SearchViewModel;

                    // Werte kopieren
                    if (newVm != null)
                    {
                        SearchString = newVm.SearchString;
                        ResultAlbum = newVm.ResultAlbum;
                        ResultArtist = newVm.ResultArtist;
                        ResultTrack = newVm.ResultTrack;
                    }
                    DisplayResults();
                    IsLoaded = true;
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        #endregion

        #region Search Handling

        /// <summary>
        /// Such nach Interpreten, Alben und Tracks die den übergebenen String im Namen haben
        /// </summary>
        /// <param name="searchString">Text, welcher im Namen enthalten sein soll</param>
        public async Task SearchAsync(string searchString)
        {
            LoadData();
            if (!string.IsNullOrEmpty(searchString))
            {
                var results = await SubsonicConnector.Current.CurrentConnection.GetSearchResultsAsync(searchString);
                foreach (var artist in results.Item1)
                {
                    ResultArtist.Add(new ListingItem(artist));
                }
                foreach (var album in results.Item2)
                {
                    ResultAlbum.Add(new ListingItem(album));
                }
                foreach (var track in results.Item3)
                {
                    track.CheckLocalFile();
                    ResultTrack.Add(track);
                }
                DisplayResults();
            }
        }

        /// <summary>
        /// Ermittelt welche Bereiche Ergebnisse enthalten und macht diese Sichtbar
        /// </summary>
        private void DisplayResults()
        {
            IsResultViewVisible = (ResultArtist.Count + ResultAlbum.Count + ResultTrack.Count > 0);
            IsArtistResultViewVisible = (ResultArtist.Count > 0);
            IsAlbumResultViewVisible = (ResultAlbum.Count > 0);
            IsTrackResultViewVisible = (ResultTrack.Count > 0);
        }

        /// <summary>
        /// Bindable Methode um die Suche zu starten
        /// </summary>
        public async void SearchClick()
        {
            await SearchAsync(SearchString);
        }

        /// <summary>
        /// Bindable Methode um auf nach dem Drücken von <see cref="VirtualKey.Enter"/> die Suche zu starten
        /// </summary>
        public async void SearchField_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key != VirtualKey.Enter || !(e.OriginalSource is TextBox)) return;
            var source = (TextBox) e.OriginalSource;
            await SearchAsync(source.Text);
        }

        #endregion

        #region Selection Handling

        public void SearchResult_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in e.AddedItems)
            {
                if (item is ListingItem)
                {
                    var newItem = item as ListingItem;
                    if (newItem.MusicObject is Artist)
                    {
                        SelectedArtists.Add(newItem);
                    }
                    if (newItem.MusicObject is Album)
                    {
                        SelectedAlbums.Add(newItem);
                    }
                }
                else if (item is Track)
                {
                    SelectedTracks.Add(item as Track);
                }
            }
            foreach (var item in e.RemovedItems)
            {
                if (item is ListingItem)
                {
                    var newItem = item as ListingItem;
                    if (newItem.MusicObject is Artist)
                    {
                        SelectedArtists.Remove(newItem);
                    }
                    if (newItem.MusicObject is Album)
                    {
                        SelectedAlbums.Remove(newItem);
                    }
                }
                else if (item is Track)
                {
                    SelectedTracks.Remove(item as Track);
                }
            }
            IsBottomAppBarVisible = SelectedArtists.Count + SelectedAlbums.Count + SelectedTracks.Count > 0;
        }

        /// <summary>
        /// Markiert alle Einträge
        /// </summary>
        public void SelectAll()
        {
            ClearSelection();
            foreach (var artist in ResultArtist)
            {
                SelectedArtists.Add(artist);
            }
            foreach (var album in ResultAlbum)
            {
                SelectedAlbums.Add(album);
            }
            foreach (var track in ResultTrack)
            {
                SelectedTracks.Add(track);
            }
        }

        /// <summary>
        /// Setzt die Auswahl zurück
        /// </summary>
        public void ClearSelection()
        {
            SelectedArtists.Clear();
            SelectedAlbums.Clear();
            SelectedTracks.Clear();
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
        /// Bindable Methode um Tracks zu einer Playlist hinzuzufügen
        /// </summary>
        public async void AddToPlaylistClick()
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(string.Format("{0} - {1}", GetType().Name,
                "AddToPlaylistClick"));
            var playlistVm = new PlaylistViewModel();
            ResourceLoader.Current.GetResource(ref playlistVm, Constants.ViewModelPlaylist);
            await playlistVm.AddTracksToPlaylistAsync(await GetTracksAsync());
            if (SettingsVm.IsSelectionCleared)
            {
                SelectionMode = ListViewSelectionMode.None;
            }
        }

        /// <summary>
        /// Bindable Methode um die Tracks der markierten Objekte herunterzuladen
        /// </summary>
        public async void DownloadTracksClick()
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(string.Format("{0} - {1}", GetType().Name,
                "DownloadTracksClick"));
            var downloadTasks = (from item in await GetTracksAsync() select item.DownloadAsync()).ToList();
            if (SettingsVm.IsSelectionCleared)
            {
                IsBottomAppBarVisible = false;
            }
            await Task.WhenAll(downloadTasks);
        }

        /// <summary>
        /// Ermittelt alle Tracks von den markierten Objekten und gibt diese zurück
        /// </summary>
        public async Task<List<Track>> GetTracksAsync()
        {
            var results = new List<Track>();

            // Markierte Tracks hinzufügen
            results.AddRange(SelectedTracks);

            // Tracks aus markierten Alben ermitteln
            var trackDetermination =
                SelectedAlbums.Select(album => SubsonicConnector.Current.CurrentConnection.GetAlbumTracksAsync(album.Id))
                    .ToList();

            // Tracks aus markierten Interpreten ermitteln
            trackDetermination.AddRange(
                SelectedArtists.Select(
                    artist => SubsonicConnector.Current.CurrentConnection.GetArtistTracksAsync(artist.Id)));

            // Tracks aus den Tasks sammeln
            foreach (var tracks in await Task.WhenAll(trackDetermination))
            {
                results.AddRange(tracks);
            }

            return results;
        }

        #endregion
    }
}