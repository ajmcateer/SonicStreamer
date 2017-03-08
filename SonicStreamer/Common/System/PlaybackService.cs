using SonicStreamer.Subsonic.Data;
using SonicStreamer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;

namespace SonicStreamer.Common.System
{
    public class PlaybackService
    {
        public MediaPlayer Player { get; }

        public MediaPlaybackList Playback { get; }

        private static PlaybackService _current;
        public static PlaybackService Current => _current ?? (_current = new PlaybackService());

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
        }

        #region Playback Manipulation

        /// <summary>
        /// Konvertiert ein <see cref="Track"/> in ein <see cref="MediaPlaybackItem"/> 
        /// </summary>
        private async Task<MediaPlaybackItem> CreatePlaybackItemAsync(SubsonicPlayableObject playableObject)
        {
            var source = Windows.Media.Core.MediaSource.CreateFromUri(await playableObject.GetSource());
            source.CustomProperties[Constants.PlaybackTrackId] = playableObject.Id;
            source.CustomProperties[Constants.PlaybackArtistId] = playableObject.ArtistId;
            source.CustomProperties[Constants.PlaybackCover] = playableObject.Cover;
            source.CustomProperties[Constants.PlaybackDuration] = playableObject.Duration;
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
        /// Prüft ob der übergebene Track bereits in der Wiedergabeliste vorhanden ist
        /// </summary>
        private bool DuplicateCheck(SubsonicPlayableObject playbackObject)
        {
            var dupCheck = from playbackTrack in Playback.Items
                where playbackTrack.Source.CustomProperties[Constants.PlaybackTrackId] as string == playbackObject.Id
                select playbackTrack;
            return dupCheck.Count() > 0;
        }

        /// <summary>
        /// Fügt die Tracks zur Wiedergabeliste hinzu.
        /// </summary>
        /// <param name="playableObjects">Hinzuzufügende Tracks</param>
        /// <param name="newPlayback">True - erstellt ein neues Playback und ersetzt das alte</param>
        public async Task AddToPlaybackAsync(IEnumerable<SubsonicPlayableObject> playableObjects, bool newPlayback = true)
        {
            if (newPlayback)
            {
                var playbackVm = new PlaybackViewModel();
                ResourceLoader.Current.GetResource(ref playbackVm, Constants.ViewModelPlayback);
                playbackVm.Clear();
                ResetPlayabck();
            }
            var wasPlaybackEmpty = Playback.Items.Count == 0;
            var addedObjects = new List<SubsonicPlayableObject>();
            foreach (var item in playableObjects)
            {
                if (DuplicateCheck(item)) continue;
                Playback.Items.Add(await CreatePlaybackItemAsync(item));
                addedObjects.Add(item);
            }
            if (wasPlaybackEmpty && Playback.Items.Count > 0)
            {
                Player.Source = Playback;
                Player.Play();
            }
            UpdatePlaybackViewModelTracks();
        }


        /// <summary>
        /// Fügt den Track zur Wiedergabeliste hinzu.
        /// </summary>
        /// <param name="playableObject">Hinzuzufügender Track</param>
        /// <param name="newPlayback">True - erstellt ein neues Playback und ersetzt das alte</param>
        public async Task AddToPlaybackAsync(SubsonicPlayableObject playableObject, bool newPlayback = true)
        {
            await AddToPlaybackAsync(new List<SubsonicPlayableObject> {playableObject}, newPlayback);
        }

        /// <summary>
        /// Fügt alle Tracks eines Albums zur Wiedergabeliste hinzu.
        /// </summary>
        /// <param name="album">Hinzuzufügendes Album</param>
        /// <param name="newPlayback">True - erstellt ein neues Playback und ersetzt das alte</param>
        public async Task AddToPlaybackAsync(Album album, bool newPlayback = true)
        {
            await AddToPlaybackAsync(album.Tracks, newPlayback);
        }

        /// <summary>
        /// Fügt alle Tracks eines Interpreten zur Wiedergabeliste hinzu.
        /// </summary>
        /// <param name="artist">Hinzuzufügender Interpret</param>
        /// <param name="newPlayback">True - erstellt ein neues Playback und ersetzt das alte</param>
        public async Task AddToPlaybackAsync(Artist artist, bool newPlayback = true)
        {
            var tracks = new List<Track>();

            foreach (var album in artist.Albums)
            {
                tracks.AddRange(album.Tracks);
            }
            await AddToPlaybackAsync(tracks, newPlayback);
        }

        /// <summary>
        /// Setzt die komplette Wiedergabeliste zurück
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
        /// Aktiviert oder Deaktiviert die Zufallswiedergabe beim Playback und aktualisiert das PlaybackViewModel
        /// </summary>
        public void SetShuffleMode(bool value)
        {
            Playback.ShuffleEnabled = value;
            UpdatePlaybackViewModelTracks();
        }

        /// <summary>
        /// Aktualisiert die Tracks des PlaybackViewModel
        /// </summary>
        private void UpdatePlaybackViewModelTracks()
        {
            var playbackVm = new PlaybackViewModel();
            ResourceLoader.Current.GetResource(ref playbackVm, Constants.ViewModelPlayback);
            playbackVm.Tracks.Clear();
            foreach (var item in GetPlaybackObjects())
            {
                playbackVm.Tracks.Add(new SubsonicPlayableObject(item));
            }
            /* Restore CurrentTrack
             * 
             * Playback.CurrentItem könnte noch null sein, wenn zum ersten mal Tracks hinzugefügt werden. Der CurrentTrack wird dann
             * über PlaybackService.Current.Playback.CurrentItemChanged im PlaybackViewModel aktualisiert
            */
            if (playbackVm.Tracks.Count > 0 && Playback.CurrentItem != null)
            {
                var currentObject =
                    playbackVm.Tracks.FirstOrDefault(
                        item =>
                            item.Id == Playback.CurrentItem.Source.CustomProperties[Constants.PlaybackTrackId] as string);
                if (currentObject != null)
                {
                    playbackVm.CurrentTrack = currentObject;
                }
            }
        }

        /// <summary>
        /// Aktiviert oder Deaktiviert die Wiederholung beim Playback
        /// </summary>
        public void SetRepeatingMode(bool value)
        {
            Player.IsLoopingEnabled = value;
        }

        #endregion

        #region Playback Handling

        /// <summary>
        /// Startet oder Pausiert die Wiedergabe
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
        /// Spielt das nächste Element im Playback ab
        /// </summary>
        public void PlayNext()
        {
            Playback.MoveNext();
        }

        /// <summary>
        /// Spielt das vorherige Element im Playback ab
        /// </summary>
        public void PlayPrevious()
        {
            Playback.MovePrevious();
        }

        /// <summary>
        /// Springt im Playback zum übergebenen Wiedergabeobject
        /// </summary>
        public void Jump(SubsonicPlayableObject playbackItem)
        {
            var index =
                Playback.Items.ToList()
                    .FindIndex(i => (string) i.Source.CustomProperties[Constants.PlaybackTrackId] == playbackItem.Id);
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
        /// Gibt alle Wiedergabeobjekte in Abhängigkeit von der Zufallswiedergabe vom Playback zurück 
        /// </summary>
        public IEnumerable<MediaPlaybackItem> GetPlaybackObjects()
        {
            return Playback.ShuffleEnabled ? Playback.ShuffledItems.ToList() : Playback.Items.ToList();
        }
    }
}