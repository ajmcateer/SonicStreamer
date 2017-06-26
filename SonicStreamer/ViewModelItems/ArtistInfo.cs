using SonicStreamer.Common.System;
using SonicStreamer.LastFM.Data;
using SonicStreamer.LastFM.Server;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Microsoft.Toolkit.Uwp.Services.Twitter;
using SonicStreamer.Common.Extension;

namespace SonicStreamer.ViewModelItems
{
    public class ArtistInfo : INotifyPropertyChanged
    {
        public struct ActiveServices
        {
            public bool LastFm, MusicBrainz, Twitter;
        }

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
        public ObservableCollection<Tweet> Tweets { get; set; }

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

        #region Initialization & Creation

        public ArtistInfo()
        {
            Infos = new ObservableCollection<Tuple<string, string>>();
            Tags = new ObservableCollection<LastFmTag>();
            SocialLinks = new ObservableCollection<Tuple<string, string>>();
            SubsonicSimilarArtists = new ObservableCollection<ListingItem>();
            LastFmSimilarArtists = new ObservableCollection<LastFmObject>();
            Tweets = new ObservableCollection<Tweet>();
        }

        /// <summary>
        /// Creates a new <see cref="ArtistInfo"/> object and loads several meta data from external provider like 
        /// last.fm or MusicBrainz and returns the results
        /// </summary>
        /// <param name="artistId">Subsonic-ID of the artist</param>
        /// <param name="name">Name of the artist for last.fm</param>
        /// <param name="enableServices">Indicates for which services data should be load</param>
        /// <param name="dispatcherWrapper">
        /// A custom <see cref="CoreDispatcher"/> can be used (e.g. for Unit Test). If not set the MainView Core 
        /// Dispatcher will be used
        /// </param>
        public static async Task<ArtistInfo> CreateAsync(string artistId, string name, ActiveServices enableServices,
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

            if (enableServices.LastFm)
            {
                tasks.Add(wrapper.RunAsync(async () =>
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
                })); 
            }
            if (enableServices.MusicBrainz)
            {
                tasks.Add(wrapper.RunAsync(async () =>
                {
                    Task<IEnumerable<Tweet>> loadTweetsTask = null;
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
                                        {
                                            result.SocialLinks.Add(
                                                new Tuple<string, string>("ms-appx:///Assets/SocialIcons/Twitter.png",
                                                    item.Target));
                                            if (enableServices.Twitter) loadTweetsTask = LoadTweetsAsync(item.Target);
                                        }
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

                            if (loadTweetsTask == null) return;
                            foreach (var tweet in await loadTweetsTask)
                            {
                                result.Tweets.Add(tweet);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                })); 
            }

            await Task.WhenAll(tasks);

            return result;
        }

        /// <summary>
        /// Creates a new <see cref="ArtistInfo"/> Object
        /// </summary>
        /// <param name="result">Instance reference to an <see cref="ArtistInfo"/> object which will be set up</param>
        /// <param name="description">Info Description for labeling</param>
        /// <param name="value">Content data</param>
        private static void CreateNewArtistInfoItem(ref ArtistInfo result, string description, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                result.Infos.Add(new Tuple<string, string>(description, value));
            }
        }

        /// <summary>
        /// Loads Tweets based on the passed Twitter URL and returns the results
        /// </summary>
        /// <param name="twitterUrl">URL to the Twitter Profile</param>
        private static async Task<IEnumerable<Tweet>> LoadTweetsAsync(string twitterUrl)
        {
            if (string.IsNullOrEmpty(twitterUrl)) return new List<Tweet>();
            try
            {
                var stringSplit = twitterUrl.Split('/');
                TwitterService.Instance.Initialize(Constants.Secrets.TwitterConsumerKey,
                    Constants.Secrets.TwitterConsumerSecret, Constants.Secrets.TwitterCallback);
                await TwitterService.Instance.LoginAsync();
                return await TwitterService.Instance.GetUserTimeLineAsync(stringSplit.Last());
            }
            catch
            {
                // ignore
                return new List<Tweet>();
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