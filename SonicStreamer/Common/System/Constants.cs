using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace SonicStreamer.Common.System
{
    public static class Constants
    {
        // ViewModels
        public const string ViewModelMain = "MainVM";
        public const string ViewModelLogin = "LoginVM";
        public const string ViewModelApplicationSettings = "SettingsVM";
        public const string ViewModelPlayback = "PlaybackVM";
        public const string ViewModelStart = "StartVM";
        public const string ViewModelSearch = "SearchVM";
        public const string ViewModelArtist = "ArtistVM";
        public const string ViewModelAlbum = "AlbumVM";
        public const string ViewModelTrackListing = "TrackListingVM";
        public const string ViewModelFolder = "FolderVM";
        public const string ViewModelPlaylist = "PlaylistVM";
        public const string ViewModelPodcast = "PodcastVM";

        // RoamingSetting
        public const string ContainerLogin = "LoginSettings";
        public const string SettingServerType = "ServerType";

        // LocalSettings
        public const string CacheFileName = "cache.json";
        public const string TrackCacheFolder = "tracks";

        // MusicBrainz

        public const string MusicBrainzArtistQueryParams =
            "-area-beginarea-endarea-arid-artist-artistaccent-alias-begin-comment-country-end-ended-gender-ipi-sortname-tag-type-";

        public const string MusicBrainzRecordingQueryParams =
            "-arid-artist-artistname-creditname-comment-country-date-dur-format-isrc-number-position-primarytype-puid-qdur-recording-recordingaccent-reid-release-rgid--rid-secondarytype-status-tid-tnum-tracks-tracksrelease-tag-type-video-";

        public const string MusicBrainzReleaseGroupQueryParams =
            "-arid-artist-artistname-comment-creditname-primarytype-rgid-releasegroup-releasegroupaccent-releases-release-reid-secondarytype-status-tag-type-";

        public const string MusicBrainzReleaseQueryParams =
            "-arid-artist-artistname-asin-barcode-catno-comment-country-creditname-date-discids-discidsmedium-format-laid-label-lang-mediums-primarytype-puid-quality-reid-release-releaseaccent-rgid-script-secondarytype-status-tag-tracks-tracksmedium-type-";

        public const string MusicBrainzEmptyStream = "Query returned an empty result.";
        public const string MusicBrainzInvalidQueryParameter = "Key not supported ({ 0}).";
        public const string MusicBrainzMissingParameter = "Attribute '{0}' must be specified.";
        public const string MusicBrainzWrongResponseFormat = "Webservice returned invalid response format.";


        public const string PlaybackTrackId = "trackId";
        public const string PlaybackName = "name";
        public const string PlaybackArtist = "artist";
        public const string PlaybackArtistId = "artistId";
        public const string PlaybackAlbum = "album";
        public const string PlaybackAlbumId = "albumId";
        public const string PlaybackCover = "cover";
        public const string PlaybackDuration = "duration";
        public const string PlaybackDurationOutput = "durationOutput";

        public static class Secrets
        {
            public static string TwitterConsumerKey;
            public static string TwitterConsumerSecret;
            public static string TwitterCallback;
            public static string LastfmApiKey;

            public static async Task LoadDataAsync()
            {
                var assetFolder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Assets");
                var secretFile = await assetFolder.GetFileAsync("Secrets.user");
                foreach (var line in await FileIO.ReadLinesAsync(secretFile))
                {
                    if (line.StartsWith("TWITTER_CONSUMER_KEY"))
                    {
                        var lineData = line.Split('=');
                        TwitterConsumerKey = lineData[1];
                        continue;
                    }
                    if (line.StartsWith("TWITTER_CONSUMER_SECRET"))
                    {
                        var lineData = line.Split('=');
                        TwitterConsumerSecret = lineData[1];
                        continue;
                    }
                    if (line.StartsWith("TWITTER_CALLBACK"))
                    {
                        var lineData = line.Split('=');
                        TwitterCallback = lineData[1];
                        continue;
                    }
                    if (line.StartsWith("LASTFM_API_KEY"))
                    {
                        var lineData = line.Split('=');
                        LastfmApiKey = lineData[1];
                        continue;
                    }
                }
            }
        }
    }
}