using SonicStreamer.SampleData;
using SonicStreamer.Subsonic.Data;
using SonicStreamer.ViewModelItems;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SonicStreamer.Common.System;

namespace SonicStreamer.ViewModels
{
    /// <summary>
    /// Stellt eine Auflistung von Alben mit ihren Tracks dar
    /// </summary>
    public class TrackListingViewModel : BaseViewModel, IViewModelSerializable
    {
        #region Properties

        private string _pageTitle;

        public string PageTitle
        {
            get { return _pageTitle; }
            set { Set(ref _pageTitle, value); }
        }

        private ArtistInfo _artistInfo;

        public ArtistInfo ArtistInfo
        {
            get { return _artistInfo; }
            set { Set(ref _artistInfo, value); }
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

        private ListViewSelectionMode _topSongsSelectionMode;

        public ListViewSelectionMode TopSongsSelectionMode
        {
            get { return _topSongsSelectionMode; }
            set { Set(ref _topSongsSelectionMode, value); }
        }

        public ObservableCollection<TrackListingItem> Albums { get; set; }
        public ObservableCollection<Track> TopSongs { get; set; }

        [XmlIgnore]
        public ObservableCollection<Track> SelectedItems { get; set; }

        [XmlIgnore]
        public ObservableCollection<Track> TopSongsSelectedItems { get; set; }

        protected PlaybackViewModel PlaybackVm;
        protected PlaylistViewModel PlaylistVm;
        protected ApplicationSettingsViewModel SettingsVm;

        #endregion

        public TrackListingViewModel()
        {
            Albums = new ObservableCollection<TrackListingItem>();
            TopSongs = new ObservableCollection<Track>();
            SelectedItems = new ObservableCollection<Track>();
            TopSongsSelectedItems = new ObservableCollection<Track>();
            TracksSelectionMode = ListViewSelectionMode.Multiple;

            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                foreach (var album in SampleDataCreator.Current.CreateAlbums())
                {
                    var item = new TrackListingItem(album) {IsTrackListVisible = true};
                    Albums.Add(item);
                }
                foreach (var track in SampleDataCreator.Current.CreateTracks())
                {
                    TopSongs.Add(track);
                }
                PageTitle = Albums.First().Artist;
                ArtistInfo = SampleDataCreator.Current.CreateArtistInfo();
            }
        }

        #region Initialization and Restoration

        public async Task LoadDataAsync(SubsonicMusicObject param)
        {
            TopSongs.Clear();
            Albums.Clear();
            ArtistInfo = new ArtistInfo();
            ClearSelection();

            if (param is Artist)
            {
                var artist = param as Artist;
                await LoadArtistDataAsync(artist.Id);
            }
            else if (param is Album)
            {
                var album = param as Album;
                await LoadAlbumDataAsync(album.Id);
            }
            else if (param is ListingItem)
            {
                var item = param as ListingItem;
                await LoadDataAsync(item.MusicObject);
                return;
            }

            foreach (
                var item in await SubsonicConnector.Current.CurrentConnection.GetTopSongsAsync(Albums.First().Artist))
            {
                TopSongs.Add(item);
            }

            CheckTrackCacheStatus();

            if (ResourceLoader.Current.GetResource(ref PlaylistVm, Constants.ViewModelPlaylist) == false)
                PlaylistVm = new PlaylistViewModel();
            if (ResourceLoader.Current.GetResource(ref PlaybackVm, Constants.ViewModelPlayback) == false)
                PlaybackVm = new PlaybackViewModel();
            if (ResourceLoader.Current.GetResource(ref SettingsVm, Constants.ViewModelApplicationSettings) == false)
                SettingsVm = new ApplicationSettingsViewModel();
        }

        /// <summary>
        /// Prüft ob die Tracks lokal verfügbar sind
        /// </summary>
        private void CheckTrackCacheStatus()
        {
            foreach (var album in Albums)
            {
                foreach (var track in album.Tracks)
                {
                    track.SetStatus();
                }
            }
            foreach (var track in TopSongs)
            {
                track.SetStatus();
            }
        }

        /// <summary>
        /// Lädt alle Tracks von allen Alben eines Interpreten
        /// </summary>
        /// <param name="artistId">Der zuvor ausgewählte Interpret</param>
        private async Task LoadArtistDataAsync(string artistId)
        {
            var artist = await SubsonicConnector.Current.CurrentConnection.GetArtistAsync(artistId);
            var albumDetermination =
                artist.Albums.Select(album => SubsonicConnector.Current.CurrentConnection.GetAlbumAsync(album.Id))
                    .ToList();
            foreach (var album in await Task.WhenAll(albumDetermination))
            {
                Albums.Add(new TrackListingItem(album));
            }
            PageTitle = artist.Name;
            var activeServices = new ArtistInfo.ActiveServices
            {
                LastFm = true,
                MusicBrainz = true,
                Twitter = true
            };
            ArtistInfo = await ArtistInfo.CreateAsync(artist.Id, artist.Name, activeServices);
        }

        /// <summary>
        /// Lädt alle Tracks eines Albums
        /// </summary>
        /// <param name="albumId">Das zuvor ausgewählte Album</param>
        private async Task LoadAlbumDataAsync(string albumId)
        {
            var album = await SubsonicConnector.Current.CurrentConnection.GetAlbumAsync(albumId);
            Albums.Add(new TrackListingItem(album, true));
            PageTitle = album.Name;
            var activeServices = new ArtistInfo.ActiveServices
            {
                LastFm = true,
                MusicBrainz = true,
                Twitter = true
            };
            ArtistInfo = await ArtistInfo.CreateAsync(album.ArtistId, album.Artist, activeServices);
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
                    var serializer = new XmlSerializer(typeof(TrackListingViewModel));
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
                    var serializer = new XmlSerializer(typeof(TrackListingViewModel));
                    var newVm = serializer.Deserialize(loadStream.AsStreamForRead()) as TrackListingViewModel;
                    // Werte kopieren
                    if (newVm != null)
                    {
                        Albums = newVm.Albums;
                        TopSongs = newVm.TopSongs;
                        ArtistInfo = newVm.ArtistInfo;
                        PageTitle = newVm.PageTitle;
                    }
                    // Offline Status der Tracks neu prüfen
                    CheckTrackCacheStatus();
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        #endregion

        #region Selection Handling

        public void Tracks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                TopSongsSelectedItems.Clear();
                // Workaround, da SelectedItems nicht gebunden werden kann
                TopSongsSelectionMode = ListViewSelectionMode.None;
                TopSongsSelectionMode = ListViewSelectionMode.Multiple;
            }
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

        public void TopSongs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                SelectedItems.Clear();
                // Workaround, da SelectedItems nicht gebunden werden kann
                TracksSelectionMode = ListViewSelectionMode.None;
                TracksSelectionMode = ListViewSelectionMode.Multiple;
            }
            foreach (var item in e.RemovedItems)
            {
                TopSongsSelectedItems.Remove(item as Track);
            }
            foreach (var item in e.AddedItems)
            {
                TopSongsSelectedItems.Add(item as Track);
            }
            IsBottomAppBarVisible = TopSongsSelectedItems.Count > 0;
        }

        /// <summary>
        /// Setzt die Auswahl aller Listen zurück
        /// </summary>
        private void ClearSelection()
        {
            SelectedItems.Clear();
            TopSongsSelectedItems.Clear();
            // Workaround, da SelectedItems nicht gebunden werden kann
            TracksSelectionMode = ListViewSelectionMode.None;
            TracksSelectionMode = ListViewSelectionMode.Multiple;
            TopSongsSelectionMode = ListViewSelectionMode.None;
            TopSongsSelectionMode = ListViewSelectionMode.Multiple;
        }

        #endregion

        #region Playlist Handling

        /// <summary>
        /// Bindable Methode um Tracks zu einer Playlist hinzuzufügen
        /// </summary>
        public async Task AddToPlaylistAsync()
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(string.Format("{0} - {1}", GetType().Name,
                "AddToPlaylistAsync"));
            var playlistVm = Application.Current.Resources[Constants.ViewModelPlaylist] as PlaylistViewModel;
            await playlistVm?.AddTracksToPlaylistAsync(GetSelection());
            if (SettingsVm.IsSelectionCleared)
            {
                ClearSelection();
                IsBottomAppBarVisible = false;
            }
        }

        #endregion

        #region CommandBar Methods

        /// <summary>
        /// Gibt die markierten Elemente einer Liste zurück
        /// </summary>
        private ObservableCollection<Track> GetSelection()
        {
            return SelectedItems.Count > 0 ? SelectedItems : TopSongsSelectedItems;
        }

        /// <summary>
        /// Spielt die markierten Elemente ab. Die existierende Wiedergabe wird ersetzt
        /// </summary>
        public void PlaySelection()
        {
            PlaybackService.Current.AddToPlaybackAsync(GetSelection());
        }

        /// <summary>
        /// Fügt die markierten Elemente zur Wiedergabe hinzu
        /// </summary>
        public void AddSelection()
        {
            PlaybackService.Current.AddToPlaybackAsync(GetSelection(), false);
        }

        /// <summary>
        /// Bindable Methode um die markierten Elemente abzuspielen. Die existierende Wiedergabe wird ersetzt
        /// </summary>
        public void PlayClick()
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(string.Format("{0} - {1}", GetType().Name, "PlayClick"));
            PlaySelection();
            if (SettingsVm.IsSelectionCleared)
            {
                ClearSelection();
                IsBottomAppBarVisible = false;
            }
        }

        /// <summary>
        /// Bindable Methode um ein gesamtes Album abzuspielen. Die existierende Wiedergabe wird ersetzt
        /// </summary>
        public void PlayAlbumClick(object sender, RoutedEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;
            if (element?.DataContext is TrackListingItem)
            {
                Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(string.Format("{0} - {1}", GetType().Name,
                    "PlayAlbumClick"));
                var trackListingItem = (TrackListingItem) element.DataContext;
                PlaybackService.Current.AddToPlaybackAsync(trackListingItem.Tracks);
            }
        }

        /// <summary>
        /// Bindable Methode um die markierten Elemente zur Wiedergabe hinzuzufügen.
        /// </summary>
        public void AddClick()
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(string.Format("{0} - {1}", GetType().Name, "AddClick"));
            AddSelection();
            if (SettingsVm.IsSelectionCleared)
            {
                ClearSelection();
                IsBottomAppBarVisible = false;
            }
        }

        /// <summary>
        /// Bindable Methode um ein gesamtes Album zur Wiedergabe hinzuzufügen.
        /// </summary>
        public void AddAlbumClick(object sender, RoutedEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;
            if (element?.DataContext is TrackListingItem)
            {
                Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(string.Format("{0} - {1}", GetType().Name,
                    "AddAlbumClick"));
                var trackListingItem = (TrackListingItem) element.DataContext;
                PlaybackService.Current.AddToPlaybackAsync(trackListingItem.Tracks, false);
            }
        }

        /// <summary>
        /// Bindable Methode um die ausgewählten Tracks herunterzuladen
        /// </summary>
        public async void DownloadTracksClick()
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(string.Format("{0} - {1}", GetType().Name,
                "DownloadTracksClick"));
            var downloadTasks = GetSelection().Select(item => item.DownloadAsync()).ToList();
            if (SettingsVm.IsSelectionCleared)
            {
                ClearSelection();
                IsBottomAppBarVisible = false;
            }
            await Task.WhenAll(downloadTasks);
        }

        /// <summary>
        /// Bindable Methode um die Tracks des Albums herunterzuladen
        /// </summary>
        public async void DownloadAlbumClick(object sender, RoutedEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;
            if (element?.DataContext is Album)
            {
                Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(string.Format("{0} - {1}", GetType().Name,
                    "DownloadAlbumClick"));
                var album = (Album) element.DataContext;
                var downloadTasks = album.Tracks.Select(item => item.DownloadAsync()).ToList();
                await Task.WhenAll(downloadTasks);
            }
        }

        #endregion
    }
}