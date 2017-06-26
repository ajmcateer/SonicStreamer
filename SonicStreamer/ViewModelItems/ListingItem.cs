using SonicStreamer.Common.System;
using SonicStreamer.Subsonic.Data;
using SonicStreamer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.UI.Xaml;

namespace SonicStreamer.ViewModelItems
{
    public enum ListingItemType
    {
        Artist,
        Album
    }

    [XmlInclude(typeof(Album))]
    [XmlInclude(typeof(Artist))]
    public class ListingItem : SubsonicMusicObject
    {
        #region Properties

        private string _infRow1;

        public string InfRow1
        {
            get { return _infRow1; }
            set { Set(ref _infRow1, value); }
        }

        private string _infRow2;

        public string InfRow2
        {
            get { return _infRow2; }
            set { Set(ref _infRow2, value); }
        }

        private string _infRow3;

        public string InfRow3
        {
            get { return _infRow3; }
            set { Set(ref _infRow3, value); }
        }

        private SubsonicMusicObject _musicObject;

        public SubsonicMusicObject MusicObject
        {
            get { return _musicObject; }
            set
            {
                Set(ref _musicObject, value);
                if (_musicObject.GetType() == typeof(Album))
                {
                    ItemType = ListingItemType.Album;
                }
                else if (_musicObject.GetType() == typeof(Artist))
                {
                    ItemType = ListingItemType.Artist;
                }
            }
        }

        protected ListingItemType ItemType;
        protected PlaybackViewModel PlaybackVm;
        protected PlaylistViewModel PlaylistVm;

        #endregion

        public ListingItem()
        {
        }

        public ListingItem(Album album)
        {
            MusicObject = album;

            Id = album.Id;
            Name = album.Name;
            Cover = album.Cover;
            InfRow1 = album.Artist;
            InfRow2 = album.TrackCount + " Tracks";

            int duration;
            if (int.TryParse(album.Duration, out duration))
            {
                var albumDuration = new TimeSpan(0, 0, duration);
                InfRow3 = albumDuration.ToString();
            }

            if (ResourceLoader.Current.GetResource(ref PlaylistVm, Constants.ViewModelPlaylist) == false)
                PlaylistVm = new PlaylistViewModel();
            if (ResourceLoader.Current.GetResource(ref PlaybackVm, Constants.ViewModelPlayback) == false)
                PlaybackVm = new PlaybackViewModel();
        }

        public ListingItem(Artist artist)
        {
            MusicObject = artist;

            Id = artist.Id;
            Name = artist.Name;
            Cover = artist.Cover;
            InfRow1 = artist.AlbumCount + " Albums";
            InfRow2 = string.Empty;
            InfRow3 = string.Empty;

            if (ResourceLoader.Current.GetResource(ref PlaylistVm, Constants.ViewModelPlaylist) == false)
                PlaylistVm = new PlaylistViewModel();
            if (ResourceLoader.Current.GetResource(ref PlaybackVm, Constants.ViewModelPlayback) == false)
                PlaybackVm = new PlaybackViewModel();
        }

        /// <summary>
        /// Lädt alle Tracks des Objektes nach.
        /// </summary>
        public async Task LoadTracks()
        {
            switch (ItemType)
            {
                case ListingItemType.Artist:
                    var artist = MusicObject as Artist;
                    if (artist != null)
                    {
                        if (artist.Albums.Count == 0)
                        {
                            var loadedArtist =
                                await SubsonicConnector.Current.CurrentConnection.GetArtistAsync(artist.Id);
                            artist.Albums = loadedArtist.Albums;
                        }
                        foreach (var album in ((Artist) MusicObject).Albums)
                        {
                            // umgeht ggf. doppeltes Laden der Tracks
                            if (album.Tracks.Count > 0) continue;
                            foreach (var track in
                                await SubsonicConnector.Current.CurrentConnection.GetAlbumTracksAsync(album.Id))
                            {
                                album.Tracks.Add(track);
                                //var musicObject = MusicObject as Album;
                                //musicObject?.Tracks.Add(track);
                            }
                        }
                    }
                    break;
                case ListingItemType.Album:
                    // umgeht ggf. doppeltes Laden der Tracks
                    if (((Album) MusicObject).Tracks.Count == 0)
                    {
                        foreach (
                            var track in
                            await SubsonicConnector.Current.CurrentConnection.GetAlbumTracksAsync(
                                ((Album) MusicObject).Id))
                        {
                            ((Album) MusicObject).Tracks.Add(track);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Gibt alle Tracks des Objektes zurück. Tracks werden ggf. vom Server geladen.
        /// </summary>
        public async Task<List<Track>> GetTracksAsync()
        {
            await LoadTracks();
            switch (ItemType)
            {
                case ListingItemType.Artist:
                    var results = new List<Track>();
                    var albums = ((Artist) MusicObject).Albums;
                    if (albums != null)
                    {
                        foreach (var album in albums)
                        {
                            results.AddRange(album.Tracks.ToList());
                        }
                    }
                    return results;
                case ListingItemType.Album:
                    return ((Album) MusicObject).Tracks.ToList();
                default:
                    return null;
            }
        }

        /// <summary>
        /// Play Methode, die an die View gebunden werden kann
        /// </summary>
        public async void PlayClick(object sender, RoutedEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;
            var item = element?.DataContext as ListingItem;
            if (item == null) return;
            Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(string.Format("{0} - {1}", GetType().Name, 
                "PlayClick"));
            await item.PlayAsync();
        }

        /// <summary>
        /// AddToPlayback Methode, die an die View gebunden werden kann
        /// </summary>
        public async void AddToPlaybackClick(object sender, RoutedEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;
            var item = element?.DataContext as ListingItem;
            if (item == null) return;
            Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(string.Format("{0} - {1}", GetType().Name,
                "AddToPlaybackClick"));
            await item.AddAsync();
        }

        /// <summary>
        /// Fügt alle Tracks des Objekts zur Wiedergabeliste hinzu. Die bisherige Wiedergabeliste wid ersetzt
        /// </summary>
        public async Task PlayAsync()
        {
             PlaybackService.Current.AddToPlaybackAsync(await GetTracksAsync());
        }

        /// <summary>
        /// Fügt alle Tracks des Objekts zur Wiedergabeliste hinzu.
        /// </summary>
        public async Task AddAsync()
        {
            PlaybackService.Current.AddToPlaybackAsync(await GetTracksAsync(), false);
        }

        /// <summary>
        /// Bindable Methode um die Tracks herunterzuladen
        /// </summary>
        public async void DownloadListingItemClick()
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(string.Format("{0} - {1}", GetType().Name,
                "DownloadListingItemClick"));
            var downloadTasks = (from item in await GetTracksAsync() select item.DownloadAsync()).ToList();
            await Task.WhenAll(downloadTasks);
        }
    }

    public class GroupedListingItemColletion : List<ListingItem>
    {
        [XmlAttribute]
        public string Key { get; set; }

        public new IEnumerator<ListingItem> GetEnumerator()
        {
            return base.GetEnumerator();
        }
    }
}