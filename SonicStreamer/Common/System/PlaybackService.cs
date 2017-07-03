using SonicStreamer.Subsonic.Data;
using SonicStreamer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Newtonsoft.Json;

namespace SonicStreamer.Common.System
{
    public class PlaybackService
    {
        public MediaPlayer Player { get; }

        public MediaPlaybackList Playback { get; }


        private static PlaybackService _current;
        public static PlaybackService Current => _current ?? (_current = new PlaybackService());

        public Dictionary<string, string> CachedPlayableObjects { get; private set; }

        protected PlaybackService()
        {
            Player = new MediaPlayer
            {
                AutoPlay = false,
                IsLoopingEnabled = Convert.ToBoolean(ApplicationData.Current.RoamingSettings.Values["IsRepeating"])
            };
            Playback = new MediaPlaybackList
            {
                AutoRepeatEnabled = true,
                ShuffleEnabled = Convert.ToBoolean(ApplicationData.Current.RoamingSettings.Values["IsShuffling"])
            };
            CachedPlayableObjects = new Dictionary<string, string>();
        }

        /// <summary>
        /// Restores Cached Ids from a JSON file in the LocalFolder
        /// </summary>
        public async Task RestoreCachedObjectsAsync()
        {
            try
            {
                var file = await ApplicationData.Current.LocalFolder.GetFileAsync(Constants.CacheFileName);
                CachedPlayableObjects = JsonConvert.DeserializeObject<Dictionary<string, string>>(await FileIO.ReadTextAsync(file));
            }
            catch
            {
                CachedPlayableObjects = new Dictionary<string, string>();
            }
            // TODO check if local files really exists in background
        }

        /// <summary>
        /// Saves all Cached Ids in a JSON file in the LocalFolder
        /// </summary>
        /// <returns></returns>
        public async Task SaveCachedObjectsAsync()
        {
            try
            {
                if (CachedPlayableObjects.Count == 0) return;
                var json = JsonConvert.SerializeObject(CachedPlayableObjects);
                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("cache.json", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(file, json);
            }
            catch
            {
                // ignore
            }
        }

        #region Playback Manipulation

        /// <summary>
        /// Converts a <see cref="Track"/> into a <see cref="MediaPlaybackItem"/> 
        /// </summary>
        private MediaPlaybackItem CreatePlaybackItemAsync(SubsonicPlayableObject playableObject)
        {
            var source = Windows.Media.Core.MediaSource.CreateFromUri(playableObject.GetSource());
            source.CustomProperties[Constants.PlaybackTrackId] = playableObject.Id;
            source.CustomProperties[Constants.PlaybackName] = playableObject.Name;
            source.CustomProperties[Constants.PlaybackArtistId] = playableObject.ArtistId;
            source.CustomProperties[Constants.PlaybackArtist] = playableObject.Artist;
            source.CustomProperties[Constants.PlaybackAlbumId] = playableObject.AlbumId;
            source.CustomProperties[Constants.PlaybackAlbum] = playableObject.Album;
            source.CustomProperties[Constants.PlaybackCover] = playableObject.Cover.Uri;
            source.CustomProperties[Constants.PlaybackDuration] = playableObject.Duration;
            source.CustomProperties[Constants.PlaybackDurationOutput] = playableObject.DurationOutput;
            var result = new MediaPlaybackItem(source);
            var displayProperties = result.GetDisplayProperties();
            displayProperties.Type = MediaPlaybackType.Music;
            displayProperties.MusicProperties.Title = playableObject.Name;
            displayProperties.MusicProperties.Artist = playableObject.Artist;
            displayProperties.MusicProperties.AlbumTitle = playableObject.Album;
            displayProperties.Thumbnail = (playableObject.Cover != null)
                ? RandomAccessStreamReference.CreateFromUri(new Uri(playableObject.Cover.Uri))
                : null;
            var track = playableObject as Track;
            if (track != null)
            {
                displayProperties.MusicProperties.TrackNumber = track.TrackNr != string.Empty
                    ? Convert.ToUInt32(track.TrackNr)
                    : 0;
            }
            result.ApplyDisplayProperties(displayProperties);
            return result;
        }

        /// <summary>
        /// Checks if the passed <see cref="SubsonicPlayableObject"/> is already available in the playback
        /// </summary>
        private bool DuplicateCheck(SubsonicPlayableObject playbackObject)
        {
            var dupCheck = from playbackTrack in Playback.Items
                where playbackTrack.Source.CustomProperties[Constants.PlaybackTrackId] as string == playbackObject.Id
                select playbackTrack;
            return dupCheck.Count() > 0;
        }

        /// <summary>
        /// Adds a set of <see cref="SubsonicPlayableObject"/> to the playback
        /// </summary>
        /// <param name="playableObjects">Objects to be added</param>
        /// <param name="newPlayback">True - creates a new Playback replaces all available objects</param>
        public void AddToPlaybackAsync(IEnumerable<SubsonicPlayableObject> playableObjects, bool newPlayback = true)
        {
            if (newPlayback)
            {
                var playbackVm = new PlaybackViewModel();
                ResourceLoader.Current.GetResource(ref playbackVm, Constants.ViewModelPlayback);
                playbackVm.Clear();
                ResetPlayabck();
            }

            var wasPlaybackEmpty = Playback.Items.Count == 0;
            var cleanedList = playableObjects.Where(item => !DuplicateCheck(item)).ToList();
            foreach (var item in cleanedList)
            {
                Playback.Items.Add(CreatePlaybackItemAsync(item));
            }

            if (wasPlaybackEmpty && Playback.Items.Count > 0)
            {
                Player.Source = Playback;
                Player.Play();
            }
            UpdatePlaybackViewModel();
        }


        /// <summary>
        /// Adds a <see cref="SubsonicPlayableObject"/> to the playback
        /// </summary>
        /// <param name="playableObject">Object to be added</param>
        /// <param name="newPlayback">True - creates a new Playback replaces all available objects</param>
        public void AddToPlaybackAsync(SubsonicPlayableObject playableObject, bool newPlayback = true)
        {
            AddToPlaybackAsync(new List<SubsonicPlayableObject> {playableObject}, newPlayback);
        }

        /// <summary>
        /// Adds all Tracks of an Album to the playback
        /// </summary>
        /// <param name="album">Album to be added</param>
        /// <param name="newPlayback">True - creates a new Playback replaces all available objects</param>
        public void AddToPlaybackAsync(Album album, bool newPlayback = true)
        {
            AddToPlaybackAsync(album.Tracks, newPlayback);
        }

        /// <summary>
        /// Adds all Tracks of an Artist to the playback
        /// </summary>
        /// <param name="artist">Artist to be added</param>
        /// <param name="newPlayback">True - creates a new Playback replaces all available objects</param>
        public void AddToPlaybackAsync(Artist artist, bool newPlayback = true)
        {
            var tracks = new List<Track>();

            foreach (var album in artist.Albums)
            {
                tracks.AddRange(album.Tracks);
            }
            AddToPlaybackAsync(tracks, newPlayback);
        }

        /// <summary>
        /// Resets the complete playback
        /// </summary>
        public void ResetPlayabck()
        {
            Player.Pause();
            Playback.Items.Clear();
            var playbackVm = new PlaybackViewModel();
            ResourceLoader.Current.GetResource(ref playbackVm, Constants.ViewModelPlayback);
            playbackVm.Clear();
        }

        /// <summary>
        /// Activates or Deactivates Shuffle Mode and updates the <see cref="PlaybackViewModel"/>
        /// </summary>
        public void SetShuffleMode(bool value)
        {
            Playback.ShuffleEnabled = value;
            UpdatePlaybackViewModel();
        }

        /// <summary>
        /// Updates data of the <see cref="PlaybackViewModel"/> after changing the Shuffle Mode
        /// </summary>
        private void UpdatePlaybackViewModel()
        {
            var playbackVm = new PlaybackViewModel();
            ResourceLoader.Current.GetResource(ref playbackVm, Constants.ViewModelPlayback);
            playbackVm.PlaybackTracks.Clear();
            var currentItems = Playback.ShuffleEnabled ? Playback.ShuffledItems.ToList() : Playback.Items.ToList();
            foreach (var item in currentItems)
            {
                playbackVm.PlaybackTracks.Add(item);
            }
            /* Restore CurrentTrack
             * 
             * It could be possible that Playback.CurrentItem is null after adding Tracks for the first time. 
             * CurrentTrack will be set via PlaybackService.Current.Playback.CurrentItemChanged in the PlaybackViewModel
            */
            if (playbackVm.PlaybackTracks.Count > 0 && Playback.CurrentItem != null)
            {
                playbackVm.PlaybackCurrentTrack = Playback.CurrentItem;
            }
        }

        /// <summary>
        /// Activates or Deactivates Loop Mode
        /// </summary>
        public void SetLoopMode(bool value)
        {
            Player.IsLoopingEnabled = value;
        }

        #endregion

        #region Playback Handling

        /// <summary>
        /// Starts or pauses the playback
        /// </summary>
        public void PlayPause()
        {
            switch (Player.PlaybackSession.PlaybackState)
            {
                case MediaPlaybackState.Playing:
                    Player.Pause();
                    break;
                case MediaPlaybackState.Paused:
                    Player.Play();
                    break;
            }
        }

        /// <summary>
        /// Plays next element in the playback
        /// </summary>
        public void PlayNext()
        {
            Playback.MoveNext();
        }

        /// <summary>
        /// Plays previous element in the playback
        /// </summary>
        public void PlayPrevious()
        {
            Playback.MovePrevious();
        }

        /// <summary>
        /// Jumps to the passed <see cref="MediaPlaybackItem"/>
        /// </summary>
        public void Jump(MediaPlaybackItem playbackItem)
        {
            var index =
                Playback.Items.ToList()
                    .FindIndex(
                        i =>
                            i.Source.CustomProperties[Constants.PlaybackTrackId] as string ==
                            playbackItem.Source.CustomProperties[Constants.PlaybackTrackId] as string);
            if (index != -1)
            {
                Playback.MoveTo((uint) index);
            }
            else
            {
                PlayNext();
            }
        }

        #endregion

        /// <summary>
        /// Returns all <see cref="MediaPlaybackItem"/> depending on the current Shuffle Mode
        /// </summary>
        public IEnumerable<MediaPlaybackItem> GetPlaybackObjects()
        {
            return Playback.ShuffleEnabled ? Playback.ShuffledItems.ToList() : Playback.Items.ToList();
        }
    }
}