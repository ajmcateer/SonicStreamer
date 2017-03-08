using SonicStreamer.Common.System;
using SonicStreamer.Subsonic.Data;
using SonicStreamer.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.UI.Xaml;

namespace SonicStreamer.ViewModelItems
{
    public class PodcastEpisodeListingItem : INotifyPropertyChanged
    {
        #region Properties

        private PodcastEpisode _episode;

        public PodcastEpisode Episode
        {
            get { return _episode; }
            set { Set(ref _episode, value); }
        }

        private bool _isDescriptionVisible;

        public bool IsDescriptionVisible
        {
            get { return _isDescriptionVisible; }
            set
            {
                Set(ref _isDescriptionVisible, value);
                // muss außerhalb sein, damit bei der Initialisierung auch das Symbol gesetzt wird
                ExpandButtonSymbol = value ? "\uE010" : "\uE011";
            }
        }

        private string _expandButtonSymbol;
        [XmlIgnore]
        public string ExpandButtonSymbol
        {
            get { return _expandButtonSymbol; }
            private set { Set(ref _expandButtonSymbol, value); }
        }

        #endregion

        public PodcastEpisodeListingItem() { }

        public PodcastEpisodeListingItem(PodcastEpisode episode) : this(episode, false) { }

        public PodcastEpisodeListingItem(PodcastEpisode episode, bool isExpand)
        {
            Episode = episode;
            IsDescriptionVisible = isExpand;
            Episode.CheckLocalFile();
        }

        #region Setter Implementation

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

        #endregion

        #region Binding Methods

        /// <summary>
        /// Bindable Methode um die Description der Episode ein- oder auszublenden
        /// </summary>
        public void ExpandCollapseButtonClick()
        {
            IsDescriptionVisible = !IsDescriptionVisible;
        }

        /// <summary>
        /// Bindable Methode um die Episode abzuspielen. Die existierende Wiedergabe wird ersetzt
        /// </summary>
        public async void PlayEpisodeClick()
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(string.Format("{0} - {1}", GetType().Name,
                "PlayEpisodeClick"));
            await PlaybackService.Current.AddToPlaybackAsync(Episode, true);
        }

        /// <summary>
        /// Bindable Methode um eine Epsiode zur Wiedergabe hinzuzufügen.
        /// </summary>
        public async void AddEpisodeClick()
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(string.Format("{0} - {1}", GetType().Name,
                "AddEpisodeClick"));
            await PlaybackService.Current.AddToPlaybackAsync(Episode, false);
        }

        /// <summary>
        /// Bindable Methode um die Episode lokal herunterzuladen
        /// </summary>
        public async void DownloadEpisodeClick()
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(string.Format("{0} - {1}", GetType().Name,
                "DownloadEpisodeClick"));
            await Episode.DownloadAsync();
        }

        /// <summary>
        /// Bindable Methode um die Episode auf den Server herunterzuladen
        /// </summary>
        public async void DownloadEpisodeToServerClick()
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(string.Format("{0} - {1}", GetType().Name,
                "DownloadEpisodeToServerClick"));
            await SubsonicConnector.Current.CurrentConnection.DownloadEpisodeToServerAsync(Episode.EpisodeId);
            await Task.Delay(TimeSpan.FromSeconds(1));
            var podcastVm = Application.Current.Resources[Constants.ViewModelPodcast] as PodcastViewModel;
            if (podcastVm != null) await podcastVm.LoadSelectedPodcastTracksAsync();
        }

        #endregion
    }
}