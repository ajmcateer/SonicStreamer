using SonicStreamer.Common.System;
using SonicStreamer.LastFM.Data;
using SonicStreamer.LastFM.Server;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using SonicStreamer.Common.Extension;

namespace SonicStreamer.ViewModelItems
{
    public class ArtistInfo : INotifyPropertyChanged
    {
        #region Properties

        public enum ArtistType
        {
            Person,
            Group,
            Orchestra,
            Choir,
            Character,
            Other
        }

        private string _name;

        public string Name
        {
            get { return _name; }
            set { Set(ref _name, value); }
        }

        private string _type;

        public string Type
        {
            get { return _type; }
            set { Set(ref _type, value); }
        }

        private string _mbid;

        public string Mbid
        {
            get { return _mbid; }
            set { Set(ref _mbid, value); }
        }

        private string _lastFmUrl;

        public string LastFmUrl
        {
            get { return _lastFmUrl; }
            set { Set(ref _lastFmUrl, value); }
        }

        private string _image;

        public string Image
        {
            get { return _image; }
            set { Set(ref _image, value); }
        }

        private string _biography;

        public string Biography
        {
            get { return _biography; }
            set { Set(ref _biography, value); }
        }

        public ObservableCollection<Tuple<string, string>> Infos { get; set; }
        public ObservableCollection<LastFmTag> Tags { get; set; }
        public ObservableCollection<Tuple<string, string>> SocialLinks { get; set; }
        public ObservableCollection<ListingItem> SubsonicSimilarArtists { get; set; }
        public ObservableCollection<LastFmObject> LastFmSimilarArtists { get; set; }

        #region XAML Object Handling

        private bool _hasGeneralInfo;

        public bool HasGeneralInfo
        {
            get { return _hasGeneralInfo; }
            set { Set(ref _hasGeneralInfo, value); }
        }

        private bool _hasTags;

        public bool HasTags
        {
            get { return _hasTags; }
            set { Set(ref _hasTags, value); }
        }

        private bool _hasSocialLinks;

        public bool HasSocialLinks
        {
            get { return _hasSocialLinks; }
            set { Set(ref _hasSocialLinks, value); }
        }

        private bool _hasBiography;

        public bool HasBiography
        {
            get { return _hasBiography; }
            set { Set(ref _hasBiography, value); }
        }

        #endregion

        #endregion

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

        #region Initalization & Creation

        public ArtistInfo()
        {
            Infos = new ObservableCollection<Tuple<string, string>>();
            Tags = new ObservableCollection<LastFmTag>();
            SocialLinks = new ObservableCollection<Tuple<string, string>>();
            SubsonicSimilarArtists = new ObservableCollection<ListingItem>();
            LastFmSimilarArtists = new ObservableCollection<LastFmObject>();
        }

        /// <summary>
        /// Erstellt ein <see cref="ArtistInfo"/> Objekt und lädt weitere Infos zum Interpreten von last.fm und MusicBrainz
        /// und gibt dieses zurück
        /// </summary>
        /// <param name="artistId">Subsonic-ID des Interpreten</param>
        /// <param name="name">Name des Interpreten für last.fm</param>
        /// <param name="dispatcherWrapper"><see cref="CoreDispatcher"/> für MainView Dispatcher oder für Unit Test</param>
        /// <returns></returns>
        public static async Task<ArtistInfo> CreateAsync(string artistId, string name,
            IDispatcherWrapper dispatcherWrapper = null)
        {
            ArtistInfo result;
            var wrapper = dispatcherWrapper ?? new DispatcherWrapper(CoreApplication.MainView.CoreWindow.Dispatcher);
            var tasks = new List<Task>();
            LastFmArtist lastFmArtist;
            MusicBrainz.Entities.Artist mbArtist;
            result = await SubsonicConnector.Current.CurrentConnection.GetArtistInfoAsync(artistId);
            result.Name = name;
            result.SocialLinks.Add(new Tuple<string, string>("ms-appx:///Assets/SocialIcons/lastfm.png",
                result.LastFmUrl));
            result.HasBiography = !string.IsNullOrEmpty(result.Biography);
            result.HasSocialLinks = result.SocialLinks.Count > 0;

            var lastFmTaks = wrapper.RunAsync(async () =>
            {
                var lastFm = new LastFmConnection();
                lastFmArtist = await lastFm.GetLastFmArtistAsync(name);
                foreach (var item in lastFmArtist.Tags)
                {
                    result.Tags.Add(item);
                }
                foreach (var item in lastFmArtist.SimilarArtists)
                {
                    result.LastFmSimilarArtists.Add(item);
                }
                result.HasTags = result.Tags.Count > 0;
            });
            var mbTask = wrapper.RunAsync(async () =>
            {
                try
                {
                    if (!string.IsNullOrEmpty(result.Mbid))
                    {
                        mbArtist = await MusicBrainz.Entities.Artist.GetAsync(result.Mbid, "url-rels");
                        CreateNewArtistInfoItem(ref result, "Type:", mbArtist.Type);
                        CreateNewArtistInfoItem(ref result, "Country:", mbArtist.Country);
                        switch (mbArtist.Type)
                        {
                            case "Person":
                                CreateNewArtistInfoItem(ref result, "Gender:", mbArtist.Gender);
                                if (mbArtist.LifeSpan != null)
                                    CreateNewArtistInfoItem(ref result, "Born:", mbArtist.LifeSpan.Begin);
                                break;
                            case "Character":
                                CreateNewArtistInfoItem(ref result, "Gender:", mbArtist.Gender);
                                if (mbArtist.LifeSpan != null)
                                    CreateNewArtistInfoItem(ref result, "Created:", mbArtist.LifeSpan.Begin);
                                break;
                            case "Group":
                            case "Orchestra":
                            case "Choir":
                                if (mbArtist.LifeSpan != null)
                                {
                                    CreateNewArtistInfoItem(ref result, "Formed:", mbArtist.LifeSpan.Begin);
                                    if (mbArtist.LifeSpan.Ended)
                                        CreateNewArtistInfoItem(ref result, "Dissolved:", mbArtist.LifeSpan.End);
                                }
                                break;
                            default:
                                break;
                        }
                        foreach (var item in mbArtist.RelationLists.Items)
                        {
                            switch (item.Type)
                            {
                                case "bandcamp":
                                    result.SocialLinks.Add(
                                        new Tuple<string, string>("ms-appx:///Assets/SocialIcons/Bandcamp.png",
                                            item.Target));
                                    break;
                                case "myspace":
                                    result.SocialLinks.Add(
                                        new Tuple<string, string>("ms-appx:///Assets/SocialIcons/Myspace.png",
                                            item.Target));
                                    break;
                                case "official homepage":
                                    result.SocialLinks.Add(
                                        new Tuple<string, string>("ms-appx:///Assets/SocialIcons/Homepage.png",
                                            item.Target));
                                    break;
                                case "purchase for download":
                                    if (item.Target.Contains("itunes.apple"))
                                        result.SocialLinks.Add(
                                            new Tuple<string, string>("ms-appx:///Assets/SocialIcons/Apple.png",
                                                item.Target));
                                    break;
                                case "social network":
                                    if (item.Target.Contains("facebook"))
                                        result.SocialLinks.Add(
                                            new Tuple<string, string>("ms-appx:///Assets/SocialIcons/Facebook.png",
                                                item.Target));
                                    if (item.Target.Contains("instagram"))
                                        result.SocialLinks.Add(
                                            new Tuple<string, string>(
                                                "ms-appx:///Assets/SocialIcons/Instagram.png", item.Target));
                                    if (item.Target.Contains("twitter"))
                                        result.SocialLinks.Add(
                                            new Tuple<string, string>("ms-appx:///Assets/SocialIcons/Twitter.png",
                                                item.Target));
                                    break;
                                case "soundcloud":
                                    result.SocialLinks.Add(
                                        new Tuple<string, string>("ms-appx:///Assets/SocialIcons/Soundcloud.png",
                                            item.Target));
                                    break;
                                case "streaming music":
                                    if (item.Target.Contains("spotify"))
                                        result.SocialLinks.Add(
                                            new Tuple<string, string>("ms-appx:///Assets/SocialIcons/Spotify.png",
                                                item.Target));
                                    break;
                                case "wikipedia":
                                    result.SocialLinks.Add(
                                        new Tuple<string, string>("ms-appx:///Assets/SocialIcons/Wikipedia.png",
                                            item.Target));
                                    break;
                                case "youtube":
                                    result.SocialLinks.Add(
                                        new Tuple<string, string>("ms-appx:///Assets/SocialIcons/YouTube.png",
                                            item.Target));
                                    break;
                                default:
                                    break;
                            }
                        }
                        result.HasGeneralInfo = result.Infos.Count > 0;
                        result.HasSocialLinks = result.SocialLinks.Count > 0;
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            });

            tasks.Add(lastFmTaks);
            tasks.Add(mbTask);
            await Task.WhenAll(tasks);

            return result;
        }

        /// <summary>
        /// Erstellt ein neues ArtistInfo Objekt sofern die übergebene Info nicht null oder <see cref="string.Empty"/> ist
        /// </summary>
        /// <param name="result">Instanz von <see cref="ArtistInfo"/> die gerade zusammengestellt wird</param>
        /// <param name="description">Beschreibung der Info</param>
        /// <param name="value">Inhalt der Info</param>
        private static void CreateNewArtistInfoItem(ref ArtistInfo result, string description, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                result.Infos.Add(new Tuple<string, string>(description, value));
            }
        }

        #endregion

        #region Web Object Interactions

        public async void TagClick(object sender, ItemClickEventArgs e)
        {
            var clickedTag = e.ClickedItem as LastFmTag;
            if (clickedTag == null) return;
            var webAddress = new Uri(clickedTag.Uri);
            await Windows.System.Launcher.LaunchUriAsync(webAddress);
        }

        public async void SocialLinkClick(object sender, ItemClickEventArgs e)
        {
            var clickedLink = e.ClickedItem as Tuple<string, string>;
            if (clickedLink == null) return;
            var webAddress = new Uri(clickedLink.Item2);
            await Windows.System.Launcher.LaunchUriAsync(webAddress);
        }

        public async void SimilarArtistClick(object sender, ItemClickEventArgs e)
        {
            var clickedObject = e.ClickedItem as LastFmObject;
            if (clickedObject == null) return;
            var webAddress = new Uri(clickedObject.LastFmUrl);
            await Windows.System.Launcher.LaunchUriAsync(webAddress);
        }

        #endregion
    }
}