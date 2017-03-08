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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SonicStreamer.ViewModels
{
    public class PodcastViewModel : BaseViewModel, IViewModelSerializable
    {
        #region Properties

        private bool _areAvailableEpisodesAvailable;

        public bool AreAvailableEpisodesAvailable
        {
            get { return _areAvailableEpisodesAvailable; }
            set { Set(ref _areAvailableEpisodesAvailable, value); }
        }

        private bool _areDownloadingEpisodesAvailable;

        public bool AreDownloadingEpisodesAvailable
        {
            get { return _areDownloadingEpisodesAvailable; }
            set { Set(ref _areDownloadingEpisodesAvailable, value); }
        }

        private bool _areSkippedEpisodesAvailable;

        public bool AreSkippedEpisodesAvailable
        {
            get { return _areSkippedEpisodesAvailable; }
            set { Set(ref _areSkippedEpisodesAvailable, value); }
        }

        private Podcast _selectedPodcast;

        public Podcast SelectedPodcast
        {
            get { return _selectedPodcast; }
            set
            {
                Set(ref _selectedPodcast, value);
                IsBottomAppBarVisible = _selectedPodcast != null;
            }
        }

        private bool _isBottomAppBarVisible;

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

        public ObservableCollection<Podcast> Podcasts { get; private set; }
        public ObservableCollection<PodcastEpisodeListingItem> NewEpisodes { get; }

        [XmlIgnore]
        public ObservableCollection<PodcastEpisodeListingItem> AvailableEpisodes { get; }

        [XmlIgnore]
        public ObservableCollection<PodcastEpisodeListingItem> SkippedEpisodes { get; }

        [XmlIgnore]
        public ObservableCollection<PodcastEpisodeListingItem> DownloadingEpisodes { get; }

        protected PlaybackViewModel PlaybackVm;
        protected ApplicationSettingsViewModel SettingsVm;

        #endregion

        #region Initialization

        public PodcastViewModel()
        {
            Podcasts = new ObservableCollection<Podcast>();
            AvailableEpisodes = new ObservableCollection<PodcastEpisodeListingItem>();
            SkippedEpisodes = new ObservableCollection<PodcastEpisodeListingItem>();
            DownloadingEpisodes = new ObservableCollection<PodcastEpisodeListingItem>();
            NewEpisodes = new ObservableCollection<PodcastEpisodeListingItem>();

            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                foreach (var item in SampleDataCreator.Current.CreatePodcasts())
                {
                    foreach (var episode in SampleDataCreator.Current.CreatePodcastEpisodes())
                    {
                        item.Episodes.Add(episode);
                    }
                    Podcasts.Add(item);
                }
                foreach (var episode in SampleDataCreator.Current.CreatePodcastEpisodes())
                {
                    NewEpisodes.Add(new PodcastEpisodeListingItem(episode));
                    AvailableEpisodes.Add(new PodcastEpisodeListingItem(episode));
                }
                SelectedPodcast = Podcasts.First();
                AreAvailableEpisodesAvailable = true;
                AreDownloadingEpisodesAvailable = true;
                AreSkippedEpisodesAvailable = true;
            }
        }

        /// <summary>
        /// Lädt alle Podcasts ohne Episoden, führt allerdings zuvor einen Refresh auf dem Server aus.
        /// Allerdings werden zusätzlich die neusten Podcasts geladen.
        /// </summary>
        public async Task LoadDataAsync()
        {
            // isLoaded wird hier nicht genutzt, damit immer der Refresh ausgeführt wird
            Podcasts.Clear();
            NewEpisodes.Clear();
            SelectedPodcast = null;
            AreDownloadingEpisodesAvailable = false;
            AreSkippedEpisodesAvailable = false;
            AreAvailableEpisodesAvailable = false;
            DownloadingEpisodes.Clear();
            SkippedEpisodes.Clear();
            AvailableEpisodes.Clear();

            if (ResourceLoader.Current.GetResource(ref PlaybackVm, Constants.ViewModelPlayback) == false)
                PlaybackVm = new PlaybackViewModel();
            if (ResourceLoader.Current.GetResource(ref SettingsVm, Constants.ViewModelApplicationSettings) == false)
                SettingsVm = new ApplicationSettingsViewModel();
            foreach (var podcast in await SubsonicConnector.Current.CurrentConnection.GetPodcastsAsync())
            {
                Podcasts.Add(podcast);
            }
            foreach (var episode in await SubsonicConnector.Current.CurrentConnection.GetNewestPodcastEpisodesAsync())
            {
                if (episode.EpisodeStatus == PodcastEpisode.DownloadStatus.Completed)
                {
                    NewEpisodes.Add(new PodcastEpisodeListingItem(episode));
                }
            }
        }

        /// <summary>
        /// Lädt für den derzeit ausgewählten Podcast die Episoden
        /// </summary>
        public async Task LoadSelectedPodcastTracksAsync()
        {
            SelectedPodcast.Episodes.Clear();
            foreach (
                var episode in await SubsonicConnector.Current.CurrentConnection.GetPodcastEpisodesAsync(SelectedPodcast.Id))
            {
                episode.CheckLocalFile();
                SelectedPodcast.Episodes.Add(episode);
            }
            SetEpisodesForSelectedPodcast();
        }

        /// <summary>
        /// Bereitet die geladenen Episoden für die View vor
        /// </summary>
        private void SetEpisodesForSelectedPodcast()
        {
            DownloadingEpisodes.Clear();
            SkippedEpisodes.Clear();
            AvailableEpisodes.Clear();

            foreach (var episode in SelectedPodcast.Episodes)
            {
                switch (episode.EpisodeStatus)
                {
                    case PodcastEpisode.DownloadStatus.Completed:
                        AvailableEpisodes.Add(new PodcastEpisodeListingItem(episode));
                        break;
                    case PodcastEpisode.DownloadStatus.Downloading:
                        DownloadingEpisodes.Add(new PodcastEpisodeListingItem(episode));
                        break;
                    case PodcastEpisode.DownloadStatus.Skipped:
                        SkippedEpisodes.Add(new PodcastEpisodeListingItem(episode));
                        break;
                    case PodcastEpisode.DownloadStatus.Unkown:
                        break;
                    default:
                        break;
                }
            }

            AreDownloadingEpisodesAvailable = DownloadingEpisodes.Count > 0;
            AreSkippedEpisodesAvailable = SkippedEpisodes.Count > 0;
            AreAvailableEpisodesAvailable = AvailableEpisodes.Count > 0;
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
                    var serializer = new XmlSerializer(typeof(PodcastViewModel));
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
                    var serializer = new XmlSerializer(typeof(PodcastViewModel));
                    var newVm = serializer.Deserialize(loadStream.AsStreamForRead()) as PodcastViewModel;

                    // Werte kopieren
                    if (newVm != null)
                    {
                        Podcasts = newVm.Podcasts;
                        SelectedPodcast = newVm.SelectedPodcast;
                    }
                    SetEpisodesForSelectedPodcast();
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        #endregion

        #region Selection Handling

        public async void Podcast_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedPodcast != null) await LoadSelectedPodcastTracksAsync();
        }

        public void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count <= 0) return;
            var currentPivotItem = e.AddedItems.First() as PivotItem;
            var pivot = currentPivotItem?.Parent as Pivot;
            if (pivot != null)
            {
                IsBottomAppBarVisible = pivot.SelectedIndex == 1 && SelectedPodcast != null;
            }
        }

        #endregion

        #region CommandBar Methods

        /// <summary>
        /// Bindable Methode um alle verfügabren Episoden des ausgewählten Podcast abzuspielen. Die existierende Wiedergabe wird ersetzt
        /// </summary>
        public async void PlayPodcastClick()
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(string.Format("{0} - {1}", GetType().Name,
                "PlayPodcastClick"));
            var episodes = AvailableEpisodes.Select(item => item.Episode).ToList();
            await PlaybackService.Current.AddToPlaybackAsync(episodes);
        }

        /// <summary>
        /// Bindable Methode um alle verfügabren Episoden des ausgewählten Podcast zur Wiedergabe hinzuzufügen.
        /// </summary>
        public async void AddPodcastClick()
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(string.Format("{0} - {1}", GetType().Name,
                "AddPodcastClick"));
            var episodes = AvailableEpisodes.Select(item => item.Episode).ToList();
            await PlaybackService.Current.AddToPlaybackAsync(episodes, false);
        }

        /// <summary>
        /// Bindable Methode um alle offenen Episoden des ausgewählten Podcasts auf den Server herunterzuladen
        /// </summary>
        public async void DownloadSkippedEpisodesClick()
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(string.Format("{0} - {1}", GetType().Name,
                "DownloadSkippedEpisodesClick"));
            var downloadEpisodes =
                SkippedEpisodes.Select(
                        item =>
                            SubsonicConnector.Current.CurrentConnection.DownloadEpisodeToServerAsync(item.Episode.EpisodeId))
                    .ToList();
            await Task.WhenAll(downloadEpisodes);
            await Task.Delay(TimeSpan.FromSeconds(1));
            SetEpisodesForSelectedPodcast();
        }

        /// <summary>
        /// Bindable Methode um alle verfügabren Episoden des ausgewählten Podcasts lokal herunterzuladen
        /// </summary>
        public async void DownloadAvailableEpisodesClick()
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(string.Format("{0} - {1}", GetType().Name,
                "DownloadAvailableEpisodesClick"));
            var downloadEpisodes = AvailableEpisodes.Select(item => item.Episode.DownloadAsync()).ToList();
            await Task.WhenAll(downloadEpisodes);
        }

        /// <summary>
        /// Bindable Methode um nach neuen Episoden des ausgewählten Podcasts zu suchen
        /// </summary>
        public async void SearchforEpisodesClick()
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(string.Format("{0} - {1}", GetType().Name,
                "SearchforEpisodesClick"));
            SelectedPodcast.Episodes.Clear();
            foreach (
                var episode in
                await SubsonicConnector.Current.CurrentConnection.GetPodcastEpisodesAsync(SelectedPodcast.Id))
            {
                SelectedPodcast.Episodes.Add(episode);
            }
            SetEpisodesForSelectedPodcast();
        }

        #endregion
    }
}