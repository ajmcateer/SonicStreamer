using SonicStreamer.Common.System;
using SonicStreamer.SampleData;
using SonicStreamer.Subsonic.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SonicStreamer.ViewModels
{
    /// <summary>
    /// Stellt die auf den Subsonic Server ablegeten Playlisten dar
    /// </summary>
    public class PlaylistViewModel : BaseViewModel, IViewModelSerializable
    {
        #region Properties

        private Playlist _selectedPlaylist;

        public Playlist SelectedPlaylist
        {
            get { return _selectedPlaylist; }
            set
            {
                Set(ref _selectedPlaylist, value);
                if (SelectedPlaylist != null)
                {
                    SelectedPlaylistTrackCount = SelectedPlaylist.TrackCount == "1"
                        ? "1 Track"
                        : string.Format("{0} Tracks", SelectedPlaylist.TrackCount);
                }
                else
                {
                    SelectedPlaylistTrackCount = string.Empty;
                }
                IsPlaylistHeaderVisible = _selectedPlaylist != null;
            }
        }

        private string _selectedPlaylistTrackCount;

        public string SelectedPlaylistTrackCount
        {
            get { return _selectedPlaylistTrackCount; }
            set { Set(ref _selectedPlaylistTrackCount, value); }
        }

        private string _newSelectedPlaylistName;

        [XmlIgnore]
        public string NewSelectedPlaylistName
        {
            get { return _newSelectedPlaylistName; }
            set { Set(ref _newSelectedPlaylistName, value); }
        }

        private bool _isRenamePopupOpen;

        public bool IsRenamePopupOpen
        {
            get { return _isRenamePopupOpen; }
            set { Set(ref _isRenamePopupOpen, value); }
        }

        private bool _isPlaylistHeaderVisible;

        [XmlIgnore]
        public bool IsPlaylistHeaderVisible
        {
            get { return _isPlaylistHeaderVisible; }
            set { Set(ref _isPlaylistHeaderVisible, value); }
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

        private ListViewSelectionMode _tracksSelectionMode;

        public ListViewSelectionMode TracksSelectionMode
        {
            get { return _tracksSelectionMode; }
            set { Set(ref _tracksSelectionMode, value); }
        }

        public ObservableCollection<Playlist> Playlists { get; set; }

        [XmlIgnore]
        public ObservableCollection<Track> SelectedItems { get; set; }

        #region Playlist Flyout Properties

        [XmlIgnore]
        public ObservableCollection<Playlist> AvailablePlaylists { get; set; }

        private Playlist _selectedAddToPlaylist;

        [XmlIgnore]
        public Playlist SelectedAddToPlaylist
        {
            get { return _selectedAddToPlaylist; }
            set
            {
                Set(ref _selectedAddToPlaylist, value);
                IsAddToPlaylistButtonEnabled = _selectedAddToPlaylist != null;
            }
        }

        private bool _isAddToPlaylistButtonEnabled;

        [XmlIgnore]
        public bool IsAddToPlaylistButtonEnabled
        {
            get { return _isAddToPlaylistButtonEnabled; }
            private set { Set(ref _isAddToPlaylistButtonEnabled, value); }
        }

        private string _newPlaylistName;

        [XmlIgnore]
        public string NewPlaylistName
        {
            get { return _newPlaylistName; }
            set { Set(ref _newPlaylistName, value); }
        }

        private bool _isNewPlaylistTextBoxVisible;

        [XmlIgnore]
        public bool IsNewPlaylistTextBoxVisible
        {
            get { return _isNewPlaylistTextBoxVisible; }
            set { Set(ref _isNewPlaylistTextBoxVisible, value); }
        }

        #endregion

        protected PlaybackViewModel PlaybackVm;
        protected ApplicationSettingsViewModel SettingsVm;

        #endregion

        #region Initialization

        public PlaylistViewModel()
        {
            Playlists = new ObservableCollection<Playlist>();
            SelectedItems = new ObservableCollection<Track>();
            AvailablePlaylists = new ObservableCollection<Playlist>();
            TracksSelectionMode = ListViewSelectionMode.Multiple;
            IsLoaded = false;

            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                foreach (var playlist in SampleDataCreator.Current.CreatePlaylists())
                {
                    Playlists.Add(playlist);
                }
                SelectedPlaylist = Playlists.First();
                NewSelectedPlaylistName = SelectedPlaylist.Name;
            }
        }

        /// <summary>
        /// Lädt alle Playlisten ohne Tracks
        /// </summary>
        public async Task LoadDataAsync()
        {
            IsRenamePopupOpen = false; // immer ausblenden, wenn Menthode aufgerufen wird
            if (!IsLoaded)
            {
                SelectedPlaylist = null;
                Playlists.Clear();
                SelectedItems.Clear();
                if (ResourceLoader.Current.GetResource(ref PlaybackVm, Constants.ViewModelPlayback) == false)
                    PlaybackVm = new PlaybackViewModel();
                if (ResourceLoader.Current.GetResource(ref SettingsVm, Constants.ViewModelApplicationSettings) == false)
                    SettingsVm = new ApplicationSettingsViewModel();
                foreach (var playlist in await SubsonicConnector.Current.CurrentConnection.GetPlaylistsAsync())
                {
                    Playlists.Add(playlist);
                }
                IsLoaded = true;
            }
        }

        /// <summary>
        /// Lädt für die übergebene Playlist die Tracks falls dies noch nicht geschehen ist
        /// </summary>
        public async Task LoadPlaylistTracksAsync(Playlist playlist)
        {
            if (playlist.Tracks.Count == 0)
            {
                foreach (
                    var track in await SubsonicConnector.Current.CurrentConnection.GetPlaylistTracksAsync(playlist.Id))
                {
                    track.CheckLocalFile();
                    playlist.Tracks.Add(track);
                }
            }
        }

        #endregion

        #region Save / Restore ViewModel

        public async Task SaveViewModelAsync(string savename)
        {
            try
            {
                var saveFolder = ApplicationData.Current.LocalFolder;
                var saveFile = await saveFolder.CreateFileAsync(savename + ".xml",
                    CreationCollisionOption.ReplaceExisting);
                using (var saveStream = await saveFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var serializer = new XmlSerializer(typeof(PlaylistViewModel));
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
                    var serializer = new XmlSerializer(typeof(PlaylistViewModel));
                    var newVm = serializer.Deserialize(loadStream.AsStreamForRead()) as PlaylistViewModel;

                    // Werte kopieren
                    if (newVm != null)
                    {
                        Playlists = newVm.Playlists;
                        SelectedPlaylist = newVm.SelectedPlaylist;
                    }
                    //TODO: Prüfen warum die ComboBox die Selektierung nicht hat
                    if (Playlists.Count > 0) IsLoaded = true;
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        #endregion

        #region Selection Handling

        public async void Playlist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count <= 0) return;
            var playlist = e.AddedItems.First() as Playlist;
            if (playlist != null)
            {
                await LoadPlaylistTracksAsync(playlist);
            }
        }

        public void PlaylistTracks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in e.RemovedItems)
            {
                SelectedItems.Remove(item as Track);
            }
            foreach (var item in e.AddedItems)
            {
                SelectedItems.Add(item as Track);
            }
            IsBottomAppBarVisible = SelectedItems.Count > 0;
        }

        /// <summary>
        /// Markiert alle Einträge
        /// </summary>
        public void SelectAll()
        {
            ClearSelection();
            foreach (var track in SelectedPlaylist.Tracks)
            {
                SelectedItems.Add(track);
            }
        }

        /// <summary>
        /// Setzt die Auswahl zurück
        /// </summary>
        public void ClearSelection()
        {
            SelectedItems.Clear();
        }

        #endregion

        #region Playlist Handling

        /// <summary>
        /// Erstellt eine neue Playlist mit den angegebenen Namen und den Tracks
        /// </summary>
        public async Task CreatePlaylistAsync(string name, IList<Track> tracks)
        {
            if (name == string.Empty)
            {
                name = DateTime.Now.ToString("yyyy-MMM-dd HH:mm:ss");
            }
            var result = await SubsonicConnector.Current.CurrentConnection.CreatePlaylistAsync(name, tracks);
            if (result.Item1)
            {
                IsLoaded = false;
            }
        }

        /// <summary>
        /// Fügt Tracks zu einer Playlist hinzu
        /// </summary>
        public async Task AddTracksToPlaylistAsync(string playlistId, IList<Track> tracks)
        {
            var result = await SubsonicConnector.Current.CurrentConnection.AddTracksToPlaylistAsync(playlistId, tracks);
            if (result.Item1)
            {
                IsLoaded = false;
            }
        }

        /// <summary>
        /// Benennt eine vorhandene Playlist um. Ist der neue Name leer, wird das aktuelle Datum
        /// im Format yyyy-MMM-dd HH:mm:ss als neuer Name gesetzt
        /// </summary>
        /// <returns>
        /// Tuple Item 1: true - Playlist erfolgreich umbenannt
        /// Tuple Item 2: Vom Server zurückgegebene Fehlermeldung
        /// </returns>
        public async Task<Tuple<bool, string>> RenamePlaylistAsync(string playlistId, string newName)
        {
            Tuple<bool, string> result;

            if (newName == string.Empty)
            {
                result = await SubsonicConnector.Current.CurrentConnection.UpdatePlaylistAsync(playlistId,
                    DateTime.Now.ToString("yyyy-MMM-dd HH:mm:ss"));
            }
            else
            {
                result = await SubsonicConnector.Current.CurrentConnection.UpdatePlaylistAsync(playlistId, newName);
            }

            if (result.Item1)
            {
                IsLoaded = false;
            }

            return result;
        }

        /// <summary>
        /// Bindable Methode um den <see cref="RenamePlaylistInputDialog"/> anzuzeigen
        /// </summary>
        public void RenamePlaylistClick()
        {
            if (SelectedPlaylist == null) return;
            NewSelectedPlaylistName = SelectedPlaylist.Name;
            IsRenamePopupOpen = true;
        }

        /// <summary>
        /// Bindable Methode um den <see cref="RenamePlaylistInputDialog"/> zu schließen und Änderungen zu speichern
        /// </summary>
        public async void SaveRenameClick()
        {
            if (NewSelectedPlaylistName != SelectedPlaylist.Name)
            {
                var result = await RenamePlaylistAsync(SelectedPlaylist.Id, NewSelectedPlaylistName);
                if (result.Item1)
                {
                    var selectedPlaylistId = SelectedPlaylist.Id;
                    await LoadDataAsync();
                    SelectedPlaylist = Playlists.FirstOrDefault(playlist => playlist.Id == selectedPlaylistId);
                }
            }
            IsRenamePopupOpen = false;
        }

        /// <summary>
        /// Bindable Methode um den <see cref="RenamePlaylistInputDialog"/> ohne Speichern zu schließen
        /// </summary>
        public void CancelRenameClick()
        {
            IsRenamePopupOpen = false;
        }

        /// <summary>
        /// Löscht eine Playlist
        /// </summary>
        /// <returns>
        /// Tuple Item 1: true - Playlist erfolgreich gelöscht
        /// Tuple Item 2: Vom Server zurückgegebene Fehlermeldung
        /// </returns>
        public async Task<Tuple<bool, string>> DeletePlaylistAsync(string playlistId)
        {
            var result = await SubsonicConnector.Current.CurrentConnection.DeletePlaylistAsync(playlistId);
            if (result.Item1)
            {
                IsLoaded = false;
            }

            return result;
        }

        /// <summary>
        /// Bindable Methode um die derzeit ausgewählte Playlist zu löschen
        /// </summary>
        public async void DeletePlaylistClick()
        {
            if (SelectedPlaylist == null) return;
            var messageDialog =
                new MessageDialog(string.Format("Do you really want to delete playlist {0}", SelectedPlaylist.Name));
            messageDialog.Commands.Add(new UICommand("Delete", new UICommandInvokedHandler(DeleteCommandHandler), true));
            messageDialog.Commands.Add(new UICommand("Cancel", new UICommandInvokedHandler(DeleteCommandHandler), false));
            messageDialog.DefaultCommandIndex = 0;
            messageDialog.CancelCommandIndex = 1;
            await messageDialog.ShowAsync();
        }

        /// <summary>
        /// Command Handler für den DeletePlaylist MessageDialog
        /// </summary>
        private async void DeleteCommandHandler(IUICommand command)
        {
            var deleteCommand = Convert.ToBoolean(command.Id);
            if (!deleteCommand) return;
            var result = await DeletePlaylistAsync(SelectedPlaylist.Id);
            if (result.Item1)
            {
                await LoadDataAsync();
            }
            else
            {
                var messageDialog = new MessageDialog(string.Format("Cannot delete playlist: {0}", result.Item2));
                await messageDialog.ShowAsync();
            }
        }

        /// <summary>
        /// Gibt die Playlist anhande der übergebenen ID zurück. Wurde die Playlist nicht
        /// gefunden wird null zurückgegeben.
        /// </summary>
        public async Task<Playlist> GetPlaylist(string playlistId)
        {
            var query = from playlist in Playlists where playlist.Id == playlistId select playlist;
            if (!query.Any()) return null;
            await LoadPlaylistTracksAsync(query.First());
            return query.First();
        }

        #endregion

        #region CommandBar Methods

        /// <summary>
        /// Spielt die markierten Tracks einer Playlist ab. Die existierende Wiedergabe wird ersetzt
        /// </summary>
        /// <remarks>
        /// Ist kein Track markiert werden alle Tracks abgespielt
        /// </remarks>
        public async Task PlaySelectionAsync()
        {
            if (SelectedItems.Count > 0)
            {
                await PlaybackService.Current.AddToPlaybackAsync(SelectedItems);
            }
            else
            {
                await PlaybackService.Current.AddToPlaybackAsync(SelectedPlaylist.Tracks);
            }
        }

        /// <summary>
        /// Fügt die markierten Tracks einer Playlist zur Wiedergabe hinzu
        /// </summary>
        /// <remarks>
        /// Ist kein Track markiert werden alle Tracks abgespielt
        /// </remarks>
        public async Task AddSelectionAsync()
        {
            if (SelectedItems.Count > 0)
            {
                await PlaybackService.Current.AddToPlaybackAsync(SelectedItems, false);
            }
            else
            {
                await PlaybackService.Current.AddToPlaybackAsync(SelectedPlaylist.Tracks, false);
            }
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
                SelectedItems.Clear();
                // Workaround, da SelectedItems kann nicht gebunden werden kann
                TracksSelectionMode = ListViewSelectionMode.None;
                TracksSelectionMode = ListViewSelectionMode.Multiple;
                IsBottomAppBarVisible = false;
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
                SelectedItems.Clear();
                // Workaround, da SelectedItems kann nicht gebunden werden kann
                TracksSelectionMode = ListViewSelectionMode.None;
                TracksSelectionMode = ListViewSelectionMode.Multiple;
                IsBottomAppBarVisible = false;
            }
        }

        /// <summary>
        /// Lädt die Daten für das AddToPlaylist-Flyout
        /// </summary>
        public async Task LoadFlyoutDataAsync()
        {
            await LoadDataAsync();
            foreach (var item in Playlists)
            {
                AvailablePlaylists.Add(item);
            }
            AvailablePlaylists.Add(new Playlist {Id = "0", Name = "Create Playlist"});
        }

        /// <summary>
        /// Setzt alle Eingaben für das AddToPlaylist-Flyout zurück
        /// </summary>
        public void ResetFlyoutInputs()
        {
            AvailablePlaylists.Clear();
            SelectedAddToPlaylist = null;
            IsNewPlaylistTextBoxVisible = false;
            NewPlaylistName = string.Empty;
        }

        /// <summary>
        /// Bindable Methode um auf den Wechsel der Auswahl für die AddToPlaylist zu reagieren
        /// </summary>
        public void AddToPlaylistComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedAddToPlaylist != null &&
                SelectedAddToPlaylist.Id == "0" &&
                SelectedAddToPlaylist.Name == "Create Playlist")
            {
                IsNewPlaylistTextBoxVisible = true;
            }
            else
            {
                IsNewPlaylistTextBoxVisible = false;
            }
        }

        /// <summary>
        /// Fügt die übergebenen Tracks zu einer Playlist hinzu. Erstellt ggf. eine neue Playlist
        /// </summary>
        public async Task AddTracksToPlaylistAsync(IList<Track> tracks)
        {
            if (tracks.Count > 0 && SelectedAddToPlaylist != null)
            {
                if (SelectedAddToPlaylist.Id == "0" && SelectedAddToPlaylist.Name == "Create Playlist")
                {
                    await CreatePlaylistAsync(_newPlaylistName, tracks);
                }
                else
                {
                    await AddTracksToPlaylistAsync(SelectedAddToPlaylist.Id, tracks);
                }
            }
        }

        /// <summary>
        /// Bindable Methode um die ausgewählten Tracks herunterzuladen
        /// </summary>
        public async void DownloadSelectedTracksClick()
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(string.Format("{0} - {1}", GetType().Name,
                "DownloadSelectedTracksClick"));
            var downloadTasks = SelectedItems.Select(item => item.DownloadAsync()).ToList();
            if (SettingsVm.IsSelectionCleared)
            {
                SelectedItems.Clear();
                // Workaround, da SelectedItems kann nicht gebunden werden kann
                TracksSelectionMode = ListViewSelectionMode.None;
                TracksSelectionMode = ListViewSelectionMode.Multiple;
                IsBottomAppBarVisible = false;
            }
            await Task.WhenAll(downloadTasks);
        }

        /// <summary>
        /// Bindable Methode um die Tracks der ausgewählten Playlist herunterzuladen
        /// </summary>
        public async void DownloadPlaylistTracksClick()
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(string.Format("{0} - {1}", GetType().Name,
                "DownloadPlaylistTracksClick"));
            var downloadTasks = SelectedPlaylist.Tracks.Select(item => item.DownloadAsync()).ToList();
            await Task.WhenAll(downloadTasks);
        }

        #endregion
    }
}