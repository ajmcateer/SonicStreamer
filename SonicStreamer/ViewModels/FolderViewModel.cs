using SonicStreamer.SampleData;
using SonicStreamer.Subsonic.Data;
using SonicStreamer.ViewModelItems;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SonicStreamer.Common.System;

namespace SonicStreamer.ViewModels
{
    /// <summary>
    /// Stellt die gesamte Subsonic Ordnerstruktur dar mit TopFolders, Folders und Tracks
    /// </summary>
    public class FolderViewModel : BaseViewModel, IViewModelSerializable
    {
        #region Properties

        private Folder _current;

        public Folder Current
        {
            get { return _current; }
            set { Set(ref _current, value); }
        }

        private ObservableCollection<Folder> _history;

        public ObservableCollection<Folder> History
        {
            get { return _history; }
            set { Set(ref _history, value); }
        }

        private int _selectedPivotItem;

        /// <summary>
        /// Steuert, ob die Ordner oder die Tracks angezeigt werden
        /// </summary>
        [XmlIgnore]
        public int SelectedPivotItem
        {
            get { return _selectedPivotItem; }
            set { Set(ref _selectedPivotItem, value); }
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
                    SelectedFolders.Clear();
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
                        playbackViewModel.IsPlaybackPanelVisible = !_isBottomAppBarVisible;
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        public ObservableCollection<GroupedFolderColletion> SubFolders { get; set; }

        [XmlIgnore]
        public ObservableCollection<Folder> SelectedFolders { get; set; }

        [XmlIgnore]
        public ObservableCollection<Track> SelectedTracks { get; set; }

        protected PlaybackViewModel PlaybackVm;
        protected PlaylistViewModel PlaylistVm;
        protected ApplicationSettingsViewModel SettingsVm;

        #endregion

        public FolderViewModel()
        {
            Current = new Folder();
            SubFolders = new ObservableCollection<GroupedFolderColletion>();
            History = new ObservableCollection<Folder>();
            SelectedFolders = new ObservableCollection<Folder>();
            SelectedTracks = new ObservableCollection<Track>();
            TracksSelectionMode = ListViewSelectionMode.Multiple;

            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                var newFolder = new Folder
                {
                    Name = "Current Folder"
                };
                foreach (var track in SampleDataCreator.Current.CreateTracks())
                {
                    newFolder.Tracks.Add(track);
                }

                var subFolders = new List<Folder>();
                foreach (var folder in SampleDataCreator.Current.CreateFolders())
                {
                    subFolders.Add(folder);
                    History.Add(folder);
                }
                var query = from folder in subFolders
                    orderby folder.Name
                    group folder by folder.Name.Substring(0, 1).ToLower()
                    into g
                    select new {GroupName = g.Key, Items = g};
                foreach (var g in query)
                {
                    var group = new GroupedFolderColletion();
                    group.Key = g.GroupName;
                    @group.AddRange(g.Items);
                    SubFolders.Add(group);
                }

                Current = newFolder;
                _selectedPivotItem = 0;
            }
        }

        /// <summary>
        /// Lädt die verfügbaren Musikverzeichnisse
        /// </summary>
        public async Task LoadDataAsync()
        {
            Current = await SubsonicConnector.Current.CurrentConnection.GetTopFolderAsync();
            GroupSubFolder();
            History.Clear();
            SelectedFolders.Clear();
            SelectedTracks.Clear();
            IsItemClickEnabled = true;
            SelectionMode = ListViewSelectionMode.None;
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
                    var serializer = new XmlSerializer(typeof(FolderViewModel));
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
                    var serializer = new XmlSerializer(typeof(FolderViewModel));
                    var newVm = serializer.Deserialize(loadStream.AsStreamForRead()) as FolderViewModel;

                    // Werte kopieren
                    Current = newVm?.Current;
                    History = newVm?.History;
                    if (History != null && (Current != null && (Current.Folders.Count > 0 || History.Count > 0)))
                        IsLoaded = true;
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        #region Folder Navigation

        public async void Folder_Click(object sender, ItemClickEventArgs e)
        {
            var folder = e.ClickedItem as Folder;
            if (folder != null)
            {
                await Navigate(folder);
            }
        }

        /// <summary>
        /// Setzt den aktuellen Ordner auf den Parameter <paramref name="target"/> und aktualisiert die History
        /// </summary>
        /// <param name="target">Navigationsziel</param>
        public async Task Navigate(Folder target)
        {
            var index = History.IndexOf(target);

            if (index != -1)
            {
                // Rückwärtsnavigation
                while (History.Count > index)
                {
                    History.RemoveAt(index);
                }
            }
            else
            {
                // Vorwärtsnavigation
                History.Add(Current);
            }

            if (History.Count == 0)
            {
                await LoadDataAsync();
            }
            else
            {
                Current = await SubsonicConnector.Current.CurrentConnection.GetFolderContentAsync(target.Id);
                foreach (var track in Current.Tracks)
                {
                    track.CheckLocalFile();
                }
                GroupSubFolder();
            }

            // ggf. auf andere Tabs ausweichen
            if (SelectedPivotItem == 0 && Current.Folders.Count == 0 && Current.Tracks.Count > 0)
            {
                SelectedPivotItem = 1;
            }
            else if (SelectedPivotItem == 1 && Current.Tracks.Count == 0 && Current.Folders.Count > 0)
            {
                SelectedPivotItem = 0;
            }
        }

        /// <summary>
        /// Gruppiert die Unterordner für den SemanticZoom
        /// </summary>
        private void GroupSubFolder()
        {
            SubFolders.Clear();
            var alphabet = new List<string>
            {
                "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m",
                "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z",
                "0", "1", "2", "3", "4", "5", "6", "7", "8", "9"
            };

            // Ordner sortieren und gruppieren
            var query = from folder in Current.Folders
                orderby folder.Name
                group folder by folder.Name.Substring(0, 1).ToLower()
                into g
                select new {GroupName = g.Key, Items = g};
            var alphaQuery = from alphaGroup in query
                where alphabet.Contains(alphaGroup.GroupName)
                select alphaGroup;
            var nonAlphaQuery = from nonAlphaGroup in query
                where !alphabet.Contains(nonAlphaGroup.GroupName)
                select nonAlphaGroup;

            foreach (var alphaGroup in alphaQuery)
            {
                var alphaGroupItem = new GroupedFolderColletion {Key = alphaGroup.GroupName};
                alphaGroupItem.AddRange(alphaGroup.Items);
                SubFolders.Add(alphaGroupItem);
            }

            if (!nonAlphaQuery.Any()) return;
            var nonAlphaGroupItem = new GroupedFolderColletion();
            foreach (var nonAlphaGroup in nonAlphaQuery)
            {
                nonAlphaGroupItem.Key = "#";
                foreach (var item in nonAlphaGroup.Items)
                {
                    nonAlphaGroupItem.Add(item);
                }
            }
            SubFolders.Add(nonAlphaGroupItem);
        }

        /// <summary>
        /// Durchsucht rekursiv in allen Unterordnern nach Tracks
        /// </summary>
        /// <param name="folder">Zu durchsuchender Ordner</param>
        /// <returns>Tracks des Ordners und seiner Unterordner</returns>
        public async Task<List<Track>> GetFolderTracks(Folder folder)
        {
            var results = new List<Track>();
            folder = await SubsonicConnector.Current.CurrentConnection.GetFolderContentAsync(folder.Id);
            results.AddRange(folder.Tracks);
            foreach (var subFolder in folder.Folders)
            {
                results.AddRange(await GetFolderTracks(subFolder));
            }
            return results;
        }

        #endregion

        #region Selection Handling

        public void FolderItem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in e.AddedItems)
            {
                if (item is Folder)
                {
                    SelectedFolders.Add(item as Folder);
                }
                else if (item is Track)
                {
                    SelectedTracks.Add(item as Track);
                }
            }
            foreach (var item in e.RemovedItems)
            {
                if (item is Folder)
                {
                    SelectedFolders.Remove(item as Folder);
                }
                else if (item is Track)
                {
                    SelectedTracks.Remove(item as Track);
                }
            }
            IsBottomAppBarVisible = SelectedFolders.Count + SelectedTracks.Count > 0;
        }

        /// <summary>
        /// Markiert alle Tracks
        /// </summary>
        public void SelectAllTracks()
        {
            ClearTrackSelection();
            foreach (var track in Current.Tracks)
            {
                SelectedTracks.Add(track);
            }
        }

        /// <summary>
        /// Setzt die Auswahl der Tracks zurück
        /// </summary>
        public void ClearTrackSelection()
        {
            SelectedTracks.Clear();
        }

        /// <summary>
        /// Markiert alle Folder
        /// </summary>
        public void SelectAllFolder()
        {
            ClearFolderSelection();
            foreach (var folder in Current.Folders)
            {
                SelectedFolders.Add(folder);
            }
        }

        /// <summary>
        /// Setzt die Auswahl der Folder zurück
        /// </summary>
        public void ClearFolderSelection()
        {
            SelectedFolders.Clear();
        }

        /// <summary>
        /// Setzt die komplette Auswahl zurück
        /// </summary>
        public void ClearSelection()
        {
            ClearFolderSelection();
            ClearTrackSelection();
        }

        #endregion

        #region Playback and Playlist Handling

        /// <summary>
        /// Speichert die markierten Elemente in einer neuen Playlist
        /// </summary>
        /// <param name="newName">Name der neuen Playlist</param>
        public async Task SaveSelectionAsync(string newName)
        {
            var tracks = await GetTracksAsync();
            await PlaylistVm.CreatePlaylistAsync(newName, tracks);
        }

        /// <summary>
        /// Speichert die markierten Elemente in der übergebenen Playlist
        /// </summary>
        public async Task SaveSelectionAsync(Playlist playlist)
        {
            var tracks = await GetTracksAsync();
            await PlaylistVm.AddTracksToPlaylistAsync(playlist.Id, tracks);
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
        /// Bindable Methode um die Tracks der markierten Elementen herunterzuladen
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
        /// Gibt alle Tracks aus den markierten Elementen (Tracks und Ordner) zurück.
        /// </summary>
        private async Task<List<Track>> GetTracksAsync()
        {
            var results = new List<Track>();

            // Markierte Tracks hinzufügen
            results.AddRange(SelectedTracks);

            // Tracks aus markierten Ordnern hinzufügen
            var trackDetermination = SelectedFolders.Select(GetFolderTracksAsync).ToList();

            foreach (var tracks in await Task.WhenAll(trackDetermination))
            {
                results.AddRange(tracks);
            }

            return results;
        }

        /// <summary>
        /// Gibt alle Tracks aus dem übergebenen Ordner und dessen Unterordner zurück.
        /// </summary>
        /// <param name="folder">Ordner welcher durchushct werden soll</param>
        private async Task<List<Track>> GetFolderTracksAsync(Folder folder)
        {
            var results = new List<Track>();
            var folderWithContent = await SubsonicConnector.Current.CurrentConnection.GetFolderContentAsync(folder.Id);

            // Tracks zum Rückgabeobjekt hinzufügen
            results.AddRange(folderWithContent.Tracks);

            // Unterordner asynchon durchsuchen
            var trackDetermination = folderWithContent.Folders.Select(GetFolderTracksAsync).ToList();

            // Tracks sammeln und zum Rückgabeobjekt hinzufügen
            foreach (var tracks in await Task.WhenAll(trackDetermination))
            {
                results.AddRange(tracks);
            }

            return results;
        }

        #endregion
    }
}