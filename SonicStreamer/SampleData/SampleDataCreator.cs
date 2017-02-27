using SonicStreamer.Subsonic.Data;
using SonicStreamer.LastFM.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SonicStreamer.ViewModelItems;

namespace SonicStreamer.SampleData
{
    public class SampleDataCreator
    {
        private static SampleDataCreator _current;

        internal static SampleDataCreator Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new SampleDataCreator();
                }
                return _current;
            }
        }

        internal List<Artist> CreateArtists()
        {
            var results = new List<Artist>();

            for (var i = 1; i <= 5; i++)
            {
                var newArtist = new Artist
                {
                    Id = i.ToString(),
                    Name = "Artist " + i.ToString(),
                };
                foreach (var album in CreateAlbums())
                {
                    newArtist.Albums.Add(album);
                }

                results.Add(newArtist);
            }

            return results;
        }

        internal List<Album> CreateAlbums()
        {
            var results = new List<Album>();

            for (var i = 1; i <= 8; i++)
            {
                var newAlbum = new Album
                {
                    Id = i.ToString(),
                    Name = "Album " + i.ToString(),
                    Artist = "Artist",
                    Duration = "123",
                    Year = "2014"
                };
                foreach (var track in CreateTracks())
                {
                    newAlbum.Tracks.Add(track);
                }
                if (i == 1) newAlbum.Name = "Album mit einem überaus sehr sehr langen Namen";

                results.Add(newAlbum);
            }

            return results;
        }

        internal List<Track> CreateTracks()
        {
            var results = new List<Track>();

            for (var i = 1; i <= 10; i++)
            {
                results.Add(new Track
                {
                    Id = i.ToString(),
                    Name = "Track " + i.ToString(),
                    Artist = "Artist",
                    Album = "Album",
                    BitRate = "128 kbit/s",
                    Duration = "268",
                    TrackNr = i.ToString(),
                    Year = "2014"
                });
            }

            return results;
        }

        internal List<Playlist> CreatePlaylists()
        {
            var results = new List<Playlist>();

            for (var i = 1; i <= 5; i++)
            {
                var newPlaylist = new Playlist
                {
                    Id = i.ToString(),
                    Name = "Playlist " + i.ToString(),
                    TrackCount = "5",
                    Comment = "This is a comment",
                    Duration = "1391",
                };
                foreach (var track in CreateTracks())
                {
                    newPlaylist.Tracks.Add(track);
                }

                results.Add(newPlaylist);
            }

            return results;
        }

        internal List<Podcast> CreatePodcasts()
        {
            var results = new List<Podcast>();
            for (var i = 1; i <= 5; i++)
            {
                var newPodcast = new Podcast
                {
                    Id = i.ToString(),
                    Name = "Podcast " + i.ToString(),
                    Description =
                        "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore " +
                        "et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. " +
                        "Stet clita kasd gubergren, no sea takimata sanctus est Lorem"
                };
                results.Add(newPodcast);
            }
            return results;
        }

        internal List<PodcastEpisode> CreatePodcastEpisodes()
        {
            var results = new List<PodcastEpisode>();
            for (var i = 1; i <= 5; i++)
            {
                var newEpisode = new PodcastEpisode
                {
                    Id = i.ToString(),
                    Name = "Podcast " + i.ToString(),
                    Artist = "Artist",
                    Album = "Album",
                    BitRate = "128 kbit/s",
                    Duration = "268",
                    Released = "2015-09-07T11:40:00.000Z",
                    Year = "2014",
                    Description =
                        "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore " +
                        "et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. " +
                        "Stet clita kasd gubergren, no sea takimata sanctus est Lorem"
                };
                results.Add(newEpisode);
            }
            return results;
        }

        internal List<Folder> CreateFolders()
        {
            var results = new List<Folder>();

            for (var i = 1; i <= 10; i++)
            {
                results.Add(new Folder
                {
                    Id = i.ToString(),
                    Name = "Folder " + i.ToString(),
                });
            }

            return results;
        }

        internal LastFmArtist CreateLastFmArtist()
        {
            var result = new LastFmArtist
            {
                Name = "LastFM Artist",
                MusicBrainzId = "12345",
                LastFmUrl = "www.lastfm.de",
                Cover = new LastFmImage {Uri = "ms-appx:///Assets/cover.png"},
                Tags = new ObservableCollection<LastFmTag>(),
                Biography =
                    "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore " +
                    "et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. " +
                    "Stet clita kasd gubergren, no sea takimata sanctus est Lorem"
            };

            result.Tags.Add(new LastFmTag {Name = "rock", Uri = "http://www.last.fm/tag/rock"});
            result.Tags.Add(new LastFmTag {Name = "alternative rock", Uri = "http://www.last.fm/tag/alternative%20rock"});
            result.Tags.Add(new LastFmTag {Name = "alternative", Uri = "http://www.last.fm/tag/alternative"});
            result.Tags.Add(new LastFmTag {Name = "hard rock", Uri = "http://www.last.fm/tag/hard%20rock"});

            result.SimilarArtists.Add(new LastFmObject
            {
                Name = "Similar Artist 1",
                LastFmUrl = "www.lastfm.de",
                Cover = new LastFmImage {Uri = "ms-appx:///Assets/cover.png"}
            });
            result.SimilarArtists.Add(new LastFmObject
            {
                Name = "Similar Artist 2",
                LastFmUrl = "www.lastfm.de",
                Cover = new LastFmImage {Uri = "ms-appx:///Assets/cover.png"}
            });
            result.SimilarArtists.Add(new LastFmObject
            {
                Name = "Similar Artist 3",
                LastFmUrl = "www.lastfm.de",
                Cover = new LastFmImage {Uri = "ms-appx:///Assets/cover.png"}
            });
            result.SimilarArtists.Add(new LastFmObject
            {
                Name = "Similar Artist 4",
                LastFmUrl = "www.lastfm.de",
                Cover = new LastFmImage {Uri = "ms-appx:///Assets/cover.png"}
            });

            return result;
        }

        internal ArtistInfo CreateArtistInfo()
        {
            var lastFmArtist = CreateLastFmArtist();
            var result = new ArtistInfo
            {
                Biography = lastFmArtist.Biography,
                Image = lastFmArtist.Cover.Uri,
                LastFmSimilarArtists = lastFmArtist.SimilarArtists,
                LastFmUrl = lastFmArtist.LastFmUrl,
                Mbid = "606bf117-494f-4864-891f-09d63ff6aa4b", // Rise Against MBID
                Name = "Rise Against",
                Tags = lastFmArtist.Tags,
                Type = "Group",
                Infos = new ObservableCollection<Tuple<string, string>>
                {
                    new Tuple<string, string>("Gender:", "Male"),
                    new Tuple<string, string>("Founded:", "2017-01-01"),
                    new Tuple<string, string>("Area:", "United States")
                },
                SocialLinks = new ObservableCollection<Tuple<string, string>>
                {
                    new Tuple<string, string>("ms-appx:///Assets/SocialIcons/lastfm.png", lastFmArtist.LastFmUrl),
                    new Tuple<string, string>("ms-appx:///Assets/SocialIcons/Bandcamp.png", string.Empty),
                    new Tuple<string, string>("ms-appx:///Assets/SocialIcons/Myspace.png", string.Empty),
                    new Tuple<string, string>("ms-appx:///Assets/SocialIcons/Homepage.png", string.Empty),
                    new Tuple<string, string>("ms-appx:///Assets/SocialIcons/Apple.png", string.Empty),
                    new Tuple<string, string>("ms-appx:///Assets/SocialIcons/Facebook.png", string.Empty),
                    new Tuple<string, string>("ms-appx:///Assets/SocialIcons/Instagram.png", string.Empty),
                    new Tuple<string, string>("ms-appx:///Assets/SocialIcons/Twitter.png", string.Empty),
                    new Tuple<string, string>("ms-appx:///Assets/SocialIcons/Soundcloud.png", string.Empty),
                    new Tuple<string, string>("ms-appx:///Assets/SocialIcons/Spotify.png", string.Empty),
                    new Tuple<string, string>("ms-appx:///Assets/SocialIcons/Wikipedia.png", string.Empty),
                    new Tuple<string, string>("ms-appx:///Assets/SocialIcons/YouTube.png", string.Empty),
                },
                HasBiography = true,
                HasGeneralInfo = true,
                HasSocialLinks = true,
                HasTags = true
            };

            return result;
        }
    }
}