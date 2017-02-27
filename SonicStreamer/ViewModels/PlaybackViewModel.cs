using SonicStreamer.Common.System;
using SonicStreamer.SampleData;
using SonicStreamer.Subsonic.Data;
using SonicStreamer.ViewModelItems;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using SonicStreamer.Common.Extension;

namespace SonicStreamer.ViewModels
{
    /// <summary>
    /// Verwaltet die Wiedergabe Tracks
    /// </summary>
    public class PlaybackViewModel : BaseViewModel, IDisposable
    {
        #region Properties

        public enum PlaybackPanelStatus
        {
            Large,
            Small,
            Page
        }

        private string _playedDuration;

        public string PlayedDuration
        {
            get { return _playedDuration; }
            set { Set(ref _playedDuration, value); }
        }

        private string _remainingDuration;

        public string RemainingDuration
        {
            get { return _remainingDuration; }
            set { Set(ref _remainingDuration, value); }
        }

        private double _currentTrackDuration;

        public double CurrentTrackDuration
        {
            get { return _currentTrackDuration; }
            set { Set(ref _currentTrackDuration, value); }
        }

        private double _currentTrackPosition;

        public double CurrentTrackPosition
        {
            get { return _currentTrackPosition; }
            set { Set(ref _currentTrackPosition, value); }
        }

        private bool _isShuffling;

        public bool IsShuffling
        {
            get { return _isShuffling; }
            set
            {
                Set(ref _isShuffling, value);
                ApplicationData.Current.RoamingSettings.Values["IsShuffling"] = value;
            }
        }

        private bool _isRepeating;

        public bool IsRepeating
        {
            get { return _isRepeating; }
            set
            {
                Set(ref _isRepeating, value);
                ApplicationData.Current.RoamingSettings.Values["IsRepeating"] = value;
            }
        }

        private Symbol _playButtonIcon;

        public Symbol PlayButtonIcon
        {
            get { return _playButtonIcon; }
            private set { Set(ref _playButtonIcon, value); }
        }

        private ArtistInfo _currentArtistInfo;

        public ArtistInfo CurrentArtistInfo
        {
            get { return _currentArtistInfo; }
            set { Set(ref _currentArtistInfo, value); }
        }

        private bool _isPlaybackPanelVisible;

        public bool IsPlaybackPanelVisible
        {
            get { return _isPlaybackPanelVisible; }
            set
            {
                if (value == false || (CurrentTrack != null && PanelStaus != PlaybackPanelStatus.Page))
                {
                    Set(ref _isPlaybackPanelVisible, value);
                }
            }
        }

        private bool _isSmallPlaybackEnabled;

        public bool IsSmallPlaybackEnabled
        {
            get { return _isSmallPlaybackEnabled; }
            set
            {
                Set(ref _isSmallPlaybackEnabled, value);
                PanelStaus = _isSmallPlaybackEnabled ? PlaybackPanelStatus.Small : PlaybackPanelStatus.Large;
            }
        }

        public ObservableCollection<SubsonicPlayableObject> Tracks { get; set; }

        private SubsonicPlayableObject _currentTrack;

        public SubsonicPlayableObject CurrentTrack
        {
            get { return _currentTrack; }
            set { Set(ref _currentTrack, value); }
        }

        public PlaybackPanelStatus PanelStaus;

        private DispatcherTimer _dispatcherTimer;
        private readonly IDispatcherWrapper _dispatcherWrapper;
        private bool _isSliderPressed;
        private ApplicationSettingsViewModel _settingsVm;

        #endregion

        #region Initialization and Restoration

        public PlaybackViewModel()
        {
            PlayButtonIcon = Symbol.Play;
            PanelStaus = PlaybackPanelStatus.Large;
            if (ResourceLoader.Current.GetResource(ref _settingsVm, Constants.ViewModelApplicationSettings) == false)
                _settingsVm = new ApplicationSettingsViewModel();
            Tracks = new ObservableCollection<SubsonicPlayableObject>();
            IsShuffling = Convert.ToBoolean(ApplicationData.Current.RoamingSettings.Values["IsShuffling"]);
            IsRepeating = Convert.ToBoolean(ApplicationData.Current.RoamingSettings.Values["IsRepeating"]);
            PlaybackService.Current.Player.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
            PlaybackService.Current.Playback.CurrentItemChanged += Playback_CurrentItemChanged;
            try
            {
                _dispatcherWrapper = new DispatcherWrapper(CoreApplication.MainView.CoreWindow.Dispatcher);
            }
            catch (Exception)
            {
                _dispatcherWrapper = new FakeDispatcherWrapper();
            }
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                foreach (var item in SampleDataCreator.Current.CreateTracks())
                {
                    Tracks.Add(item);
                }
                CurrentTrack = Tracks.First();
            }
        }

        public async Task LoadDataAsync()
        {
            Tracks.Clear();
            foreach (var item in PlaybackService.Current.GetPlaybackObjects())
            {
                Tracks.Add(new SubsonicPlayableObject(item));
            }
            if (PlaybackService.Current.Playback.CurrentItem != null)
            {
                await SetCurrentTrackAsync(
                    PlaybackService.Current.Playback.CurrentItem.Source.CustomProperties["trackId"] as string);
            }
        }

        private async void Playback_CurrentItemChanged(MediaPlaybackList sender,
            CurrentMediaPlaybackItemChangedEventArgs args)
        {
            await _dispatcherWrapper.RunAsync(async () =>
            {
                if (sender.CurrentItem != null)
                {
                    await SetCurrentTrackAsync(sender.CurrentItem.Source.CustomProperties["trackId"] as string);
                }
            });
        }

        /// <summary>
        /// Setzt den CurrentTrack anhand der übergebenen ID
        /// </summary>
        private async Task SetCurrentTrackAsync(string trackId)
        {
            var index = Tracks.ToList().FindIndex(i => i.Id == trackId);
            if (index != -1)
            {
                CurrentTrack = Tracks.ElementAt(index);
                CurrentArtistInfo = await ArtistInfo.CreateAsync(CurrentTrack.ArtistId, CurrentTrack.Artist);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            PlaybackService.Current.Player.CurrentStateChanged -= MediaPlayer_CurrentStateChanged;
            PlaybackService.Current.Playback.CurrentItemChanged -= Playback_CurrentItemChanged;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region BackgroundMediaPlayer Handling

        /// <summary>
        /// MediaPlayer state changed event handlers. 
        /// Note that we can subscribe to events even if Media Player is playing media in background
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void MediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {
            await _dispatcherWrapper.RunAsync(() =>
            {
                switch (sender.PlaybackSession.PlaybackState)
                {
                    case MediaPlaybackState.Playing:
                        IsPlaybackPanelVisible = true;
                        PlayButtonIcon = Symbol.Pause;
                        dispatcherTimer_Start();
                        break;
                    case MediaPlaybackState.Paused:
                        IsPlaybackPanelVisible = true;
                        PlayButtonIcon = Symbol.Play;
                        dispatcherTimer_Stop();
                        break;
                    default:
                        IsPlaybackPanelVisible = false;
                        PlayButtonIcon = Symbol.Play;
                        dispatcherTimer_Stop();
                        break;
                }
            });
        }

        #endregion

        #region Media Handling

        /// <summary>
        /// Lässt über den PlaybackService die Wiedergabe starten oder stoppen
        /// </summary>
        public void Play()
        {
            PlaybackService.Current.PlayPause();
        }

        /// <summary>
        /// Lässt über den PlaybackService den nächsten Track wiederzugeben
        /// </summary>
        public void PlayNext()
        {
            PlaybackService.Current.PlayNext();
        }

        /// <summary>
        /// Lässt über den PlaybackService den vorherigen Track wiederzugeben
        /// </summary>
        public void PlayPrevious()
        {
            PlaybackService.Current.PlayPrevious();
        }

        /// <summary>
        /// Lässt den PlaybackService zum übergebenen Wiedergabeobjekt springen
        /// </summary>
        public void Jump(SubsonicPlayableObject playbackItem)
        {
            PlaybackService.Current.Jump(playbackItem);
        }

        /// <summary>
        /// Bindable Methode um zum angeclickten Widergabeobjekt zu springen
        /// </summary>
        public void PlaybackTracks_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as SubsonicPlayableObject;
            if (item != null)
            {
                Jump(item);
            }
        }

        /// <summary>
        /// Setzt die komplette Wiedergabeliste zurück
        /// </summary>
        public void Clear()
        {
            Tracks.Clear();
            IsPlaybackPanelVisible = false;
            PlayedDuration = string.Empty;
            RemainingDuration = string.Empty;
            CurrentTrackPosition = 0.0;
            CurrentTrackDuration = 0.0;
            CurrentTrack = null;
        }

        /// <summary>
        /// Bindable Methode um in den Shuffle Mode zu wechseln
        /// </summary>
        public void SwitchOnShuffleMode()
        {
            IsShuffling = true;
            PlaybackService.Current.SetShuffleMode(true);
        }

        /// <summary>
        /// Bindable Methode um wieder aus den Shuffle Mode zu wechseln
        /// </summary>
        public void SwitchOffShuffleMode()
        {
            IsShuffling = false;
            PlaybackService.Current.SetShuffleMode(false);
        }

        /// <summary>
        /// Bindable Methode um dem Repeat Mode zu wechseln
        /// </summary>
        public void SwitchOnRepeatingMode()
        {
            IsRepeating = true;
            PlaybackService.Current.SetRepeatingMode(true);
        }

        /// <summary>
        /// Bindable Methode um wieder aus dem Repeat Mode zu wechseln
        /// </summary>
        public void SwitchOffRepeatingMode()
        {
            IsRepeating = false;
            PlaybackService.Current.SetRepeatingMode(false);
        }

        #endregion

        #region MediaProgressSlider Handling

        private void dispatcherTimer_Tick(object sender, object e)
        {
            // Prüfung notwendig um kurzzeitigen Trackwechsel abzufangen, sofern der Tick noch läuft
            if (CurrentTrack != null)
            {
                var player = PlaybackService.Current.Player;
                CurrentTrackDuration = CurrentTrack.DurationTime.TotalSeconds;
                if (!_isSliderPressed)
                {
                    CurrentTrackPosition = player.PlaybackSession.Position.TotalSeconds;
                }

                var remainingDuration = CurrentTrack.DurationTime - player.PlaybackSession.Position;

                PlayedDuration = string.Format("{0}.{1}:{2}",
                    player.PlaybackSession.Position.Hours.ToString("D2"),
                    player.PlaybackSession.Position.Minutes.ToString("D2"),
                    player.PlaybackSession.Position.Seconds.ToString("D2"));
                RemainingDuration = string.Format("{0}.{1}:{2}",
                    remainingDuration.Hours.ToString("D2"),
                    remainingDuration.Minutes.ToString("D2"),
                    remainingDuration.Seconds.ToString("D2"));

                // verhindert negative remainingDuration beim Repeat oder beim langen Laden
                if (remainingDuration.TotalSeconds == 0)
                {
                    dispatcherTimer_Stop();
                }
            }
        }

        private void dispatcherTimer_Start()
        {
            _dispatcherTimer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(1)};
            _dispatcherTimer.Tick += dispatcherTimer_Tick;
            _dispatcherTimer.Start();
        }

        private void dispatcherTimer_Stop()
        {
            try
            {
                _dispatcherTimer.Stop();
                _dispatcherTimer.Tick -= dispatcherTimer_Tick;
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public void PlaybackSlider_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            PlaybackService.Current.Player.PlaybackSession.Position = TimeSpan.FromSeconds(CurrentTrackPosition);
            _isSliderPressed = false;
        }

        public void PlaybackSlider_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            _isSliderPressed = true;
        }

        public void PlaybackSlider_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            _isSliderPressed = false;
        }

        #endregion
    }
}