using SonicStreamer.Subsonic.Data;
using SonicStreamer.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace SonicStreamer.SampleData
{
    public class PlaybackSampleData : BaseViewModel
    {
        #region Properties

        private string _snappedWidth;

        public string SnappedWidth
        {
            get { return _snappedWidth; }
            set
            {
                if (value != _snappedWidth)
                {
                    _snappedWidth = value;
                    NotifyPropertyChanged("SnappedWidth");
                }
            }
        }

        private bool _isSnapped;

        public bool IsSnapped
        {
            get { return _isSnapped; }
            set
            {
                if (value != _isSnapped)
                {
                    _isSnapped = value;
                    NotifyPropertyChanged("IsSnapped");
                }
            }
        }

        private string _playedDuration;

        public string PlayedDuration
        {
            get { return _playedDuration; }
            set
            {
                if (value != _playedDuration)
                {
                    _playedDuration = value;
                    NotifyPropertyChanged("PlayedDuration");
                }
            }
        }

        private string _remainingDuration;

        public string RemainingDuration
        {
            get { return _remainingDuration; }
            set
            {
                if (value != _remainingDuration)
                {
                    _remainingDuration = value;
                    NotifyPropertyChanged("RemainingDuration");
                }
            }
        }

        private bool _isShuffling;

        public bool IsShuffling
        {
            get { return _isShuffling; }
            set
            {
                if (value != _isShuffling)
                {
                    _isShuffling = value;
                    NotifyPropertyChanged("IsShuffling");
                }
            }
        }

        private bool _isRepeating;

        public bool IsRepeating
        {
            get { return _isRepeating; }
            set
            {
                if (value != _isRepeating)
                {
                    _isRepeating = value;
                    NotifyPropertyChanged("IsRepeating");
                }
            }
        }

        private ObservableCollection<Track> _tracks;

        public ObservableCollection<Track> Tracks
        {
            get { return _tracks; }
            set
            {
                if (value != _tracks)
                {
                    _tracks = value;
                    NotifyPropertyChanged("Tracks");
                }
            }
        }

        private Track _currentTrack;

        public Track CurrentTrack
        {
            get { return _currentTrack; }
            set
            {
                // Es wird nicht auf einen neuen Wert geprüft damit die Schleifenwiedergabe funktioniert
                _currentTrack = value;
                NotifyPropertyChanged("CurrentTrack");
                NotifyPropertyChanged("CurrentTrackUri");
            }
        }

        public Uri CurrentTrackUri
        {
            get { return CurrentTrack.GetStreamUri(); }
        }

        #endregion

        public PlaybackSampleData()
        {
            Tracks = new ObservableCollection<Track>();
            foreach (var track in SampleDataCreator.Current.CreateTracks())
            {
                Tracks.Add(track);
            }
            CurrentTrack = Tracks.First();
            SnappedWidth = "370";
            PlayedDuration = "02:15";
            RemainingDuration = "01:11";
            IsShuffling = true;
            IsRepeating = false;
            IsSnapped = false;
        }
    }
}