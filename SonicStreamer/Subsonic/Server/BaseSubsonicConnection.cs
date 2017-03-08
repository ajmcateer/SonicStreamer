using SonicStreamer.Common.Extension;
using SonicStreamer.Common.System;
using SonicStreamer.Subsonic.Data;
using SonicStreamer.ViewModelItems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Windows.Foundation;
using Windows.Security.Cryptography.Certificates;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace SonicStreamer.Subsonic.Server
{
    public abstract class BaseSubsonicConnection : BaseXmlServerConnection
    {
        #region Properties

        public enum AlbumListType
        {
            Random,
            Recent,
            Frequent,
            Newest
        }

        public string User { get; protected set; }

        public string ServerUrl { get; protected set; }

        public string Password { get; protected set; }

        public string ServerApiVersion { get; protected set; }

        protected string FallbackServerApiVersion { get; set; }

        /// <summary>
        /// Die lokal von SubsonicConnection verwalteten Playlisten
        /// </summary>
        protected List<Playlist> Playlists;

        public event TypedEventHandler<object, SubsonicSyncEventArgs> AlbumLoaded;

        #endregion

        protected BaseSubsonicConnection() : base()
        {
            Playlists = new List<Playlist>();

            var filter = new HttpBaseProtocolFilter();
            filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.Expired);
            filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.InvalidName);
            filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.Untrusted);
            HttpClient = new HttpClient(filter);
        }

        /// <summary>
        /// Baut eine Verbindung zum Server auf
        /// </summary>
        public async Task<Tuple<bool, string>> Connect(string server, string user, string password,
            bool testMode = false)
        {
            if (testMode)
            {
                ServerUrl = server;
                User = user;
                Password = password;
                return new Tuple<bool, string>(true, string.Empty);
            }
            try
            {
                if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
                {
                    return Tuple.Create(false, "Wrong username or password.");
                }

                // Befehl an Server senden und Antwort empfangen
                var response = await HttpClient.GetAsync(new Uri(GetPingApiMethodUri(server, user, password)));
                var responseStream = await response.Content.ReadAsInputStreamAsync();

                using (var stream = responseStream.AsStreamForRead())
                {
                    var reader = CustomXmlReader.Create(XmlReader.Create(stream));
                    var result = CheckStatus(ref reader);
                    if (!result.Item1)
                        return Tuple.Create(false,
                            "Connection to server successful but error in response: " + result.Item2);
                    ServerUrl = server;
                    User = user;
                    Password = password;
                    ServerApiVersion = reader.GetAttribute("version", FallbackServerApiVersion);
                    return Tuple.Create(true, string.Empty);
                }
            }
            catch (System.Exception e)
            {
                return Tuple.Create(false, "Error: " + e.Message);
            }
        }

        protected abstract string GetPingApiMethodUri(string server, string user, string password);

        /// <summary>
        /// Trennt die Verbindung zum Server
        /// </summary>
        public void Disconnect()
        {
            ServerUrl = string.Empty;
            User = string.Empty;
            Password = string.Empty;
            ServerApiVersion = FallbackServerApiVersion;
            Playlists.Clear();
        }

        #region XML to Object

        protected virtual Track NewTrackFromXml(CustomXmlReader xml)
        {
            var newTrack = new Track
            {
                Id = xml.GetAttribute("id"),
                Name = xml.GetAttribute("title"),
                TrackNr = xml.GetAttribute("track"),
                Artist = xml.GetAttribute("artist"),
                ArtistId = xml.GetAttribute("artistId"),
                Album = xml.GetAttribute("album"),
                AlbumId = xml.GetAttribute("albumId"),
                Year = xml.GetAttribute("year"),
                Duration = xml.GetAttribute("duration"),
                Path = xml.GetAttribute("path").Replace('/', '\\')
            };
            var newBitRate = xml.GetAttribute("bitRate");
            if (newBitRate != string.Empty)
            {
                newTrack.BitRate = newBitRate + " kbit/s";
            }
            newTrack.Cover = new CoverArt(xml.GetAttribute("coverArt"));
            return newTrack;
        }

        protected virtual Album NewAlbumFromXml(CustomXmlReader xml)
        {
            return new Album
            {
                Id = xml.GetAttribute("id"),
                Name = xml.GetAttribute("name"),
                Duration = xml.GetAttribute("duration"),
                TrackCount = xml.GetAttribute("songCount"),
                Artist = xml.GetAttribute("artist"),
                ArtistId = xml.GetAttribute("artistId"),
                Year = xml.GetAttribute("year"),
                Cover = new CoverArt(xml.GetAttribute("coverArt"))
            };
        }

        protected virtual Artist NewArtistFromXml(CustomXmlReader xml)
        {
            return new Artist
            {
                Id = xml.GetAttribute("id"),
                Mbid = xml.GetAttribute("mbid"),
                Name = xml.GetAttribute("name"),
                Cover = new CoverArt(xml.GetAttribute("coverArt")),
                AlbumCount = xml.GetAttribute("albumCount")
            };
        }

        protected virtual Playlist NewPlaylistFromXml(CustomXmlReader xml)
        {
            return new Playlist
            {
                Id = xml.GetAttribute("id"),
                Name = xml.GetAttribute("name"),
                TrackCount = xml.GetAttribute("songCount"),
                Comment = xml.GetAttribute("comment"),
                Duration = xml.GetAttribute("duration"),
                Cover = new CoverArt(xml.GetAttribute("coverArt"))
            };
        }

        protected virtual Podcast NewPodcastFromXml(CustomXmlReader xml)
        {
            return new Podcast
            {
                Id = xml.GetAttribute("id"),
                Name = xml.GetAttribute("title"),
                Source = xml.GetAttribute("url"),
                Description = xml.GetAttribute("description"),
                Status = xml.GetAttribute("status"),
                Cover = new CoverArt(xml.GetAttribute("coverArt"))
            };
        }

        protected virtual PodcastEpisode NewPodcastEpisodeFromXml(CustomXmlReader xml)
        {
            var newEpisode = new PodcastEpisode
            {
                Id = xml.GetAttribute("streamId"),
                EpisodeId = xml.GetAttribute("id"),
                Name = xml.GetAttribute("title"),
                Description = xml.GetAttribute("description"),
                Artist = xml.GetAttribute("artist"),
                ArtistId = xml.GetAttribute("artistId"),
                Album = xml.GetAttribute("album"),
                AlbumId = xml.GetAttribute("albumId"),
                Year = xml.GetAttribute("year"),
                Duration = xml.GetAttribute("duration"),
                Released = xml.GetAttribute("publishDate").Substring(0, 10)
            };
            newEpisode.Path = $"Podcasts\\{newEpisode.Artist}\\{newEpisode.Album}\\{newEpisode.EpisodeId}.mp3";
            var newBitRate = xml.GetAttribute("bitRate");
            if (newBitRate != string.Empty)
            {
                newEpisode.BitRate = newBitRate + " kbit/s";
            }
            switch (xml.GetAttribute("status"))
            {
                case "completed":
                    newEpisode.EpisodeStatus = PodcastEpisode.DownloadStatus.Completed;
                    break;
                case "downloading":
                    newEpisode.EpisodeStatus = PodcastEpisode.DownloadStatus.Downloading;
                    break;
                case "skipped":
                    newEpisode.EpisodeStatus = PodcastEpisode.DownloadStatus.Skipped;
                    break;
                default:
                    newEpisode.EpisodeStatus = PodcastEpisode.DownloadStatus.Unkown;
                    break;
            }
            newEpisode.Cover = new CoverArt(xml.GetAttribute("coverArt"));
            return newEpisode;
        }

        #endregion

        #region Server Get Methods

        /// <summary>
        /// Ermittelt alle Interpreten vom Server.
        /// </summary>
        public virtual async Task<List<Artist>> GetArtistsAsync()
        {
            var results = new List<Artist>();
            var xml = (await GetXmlFromServer("getArtists")).Item1;
            if (xml != null && xml.ReadToDescendant("artists") && xml.ReadToDescendant("index"))
            {
                do
                {
                    if (!xml.ReadToDescendant("artist")) continue;
                    do
                    {
                        results.Add(NewArtistFromXml(xml));
                    } while (xml.ReadToNextSibling("artist"));
                } while (xml.ReadToNextSibling("index"));
            }
            return results;
        }

        /// <summary>
        /// Ermittelt alle Alben aus dem übergebenen Interpreten
        /// </summary>
        public virtual async Task<Artist> GetArtistAsync(string artistId)
        {
            var result = new Artist();
            var param = new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>("id", artistId)};
            var xml = (await GetXmlFromServer("getArtist", param)).Item1;
            if (xml != null && xml.ReadToDescendant("artist"))
            {
                result = NewArtistFromXml(xml);
                if (!xml.ReadToDescendant("album")) return result;
                do
                {
                    result.Albums.Add(NewAlbumFromXml(xml));
                } while (xml.ReadToNextSibling("album"));
            }
            return result;
        }

        /// <summary>
        /// Ermittel die last.fm Daten vom übergebenen Interpreten
        /// </summary>
        public virtual async Task<ArtistInfo> GetArtistInfoAsync(string artistId)
        {
            var result = new ArtistInfo();
            var param = new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>("id", artistId)};
            var xml = (await GetXmlFromServer("getArtistInfo2", param)).Item1;
            if (xml != null && xml.ReadToDescendant("artistInfo2"))
            {
                while (!xml.Reader.EOF)
                {
                    if (xml.Reader.Read() && xml.Reader.NodeType == XmlNodeType.Element)
                    {
                        switch (xml.Reader.Name)
                        {
                            case "biography":
                                var htmlBio = xml.ReadElementContentAsString();
                                if (!string.IsNullOrEmpty(htmlBio))
                                {
                                    var noHtmlBio = Regex.Replace(htmlBio, @"<[^>]+>|&nbsp;", string.Empty).Trim();
                                    var noHtmlNormalisedBio = Regex.Replace(noHtmlBio, @"\s{2,}", " ");

                                    noHtmlNormalisedBio = Regex.Replace(noHtmlNormalisedBio, @"&quot;", "\"");
                                    noHtmlNormalisedBio = Regex.Replace(noHtmlNormalisedBio, @"&amp;", "&");
                                    noHtmlNormalisedBio = Regex.Replace(noHtmlNormalisedBio, @"&lt;", "<");
                                    noHtmlNormalisedBio = Regex.Replace(noHtmlNormalisedBio, @"&gt;", ">");

                                    var readMorePassageStart = noHtmlNormalisedBio.IndexOf("Read more about",
                                        StringComparison.Ordinal);
                                    if (readMorePassageStart >= 0)
                                        noHtmlNormalisedBio = noHtmlNormalisedBio.Remove(readMorePassageStart);

                                    result.Biography = noHtmlNormalisedBio;
                                }
                                break;
                            case "musicBrainzId":
                                result.Mbid = xml.ReadElementContentAsString();
                                break;
                            case "lastFmUrl":
                                result.LastFmUrl = xml.ReadElementContentAsString();
                                break;
                            case "largeImageUrl":
                                result.Image = xml.ReadElementContentAsString();
                                break;
                            case "similarArtist":
                                var newArtist = NewArtistFromXml(xml);
                                result.SubsonicSimilarArtists.Add(new ListingItem(newArtist));
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Aktualisiert das Album in dem alle Tracks ermittelt werden
        /// </summary>
        public virtual async Task<Album> GetAlbumAsync(string albumId)
        {
            var result = new Album();
            var param = new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>("id", albumId)};
            var xml = (await GetXmlFromServer("getAlbum", param)).Item1;
            if (xml != null && xml.ReadToDescendant("album"))
            {
                result = NewAlbumFromXml(xml);
                if (xml.ReadToDescendant("song"))
                {
                    do
                    {
                        result.Tracks.Add(NewTrackFromXml(xml));
                    } while (xml.ReadToNextSibling("song"));
                }
            }
            AlbumLoaded?.Invoke(this, new SubsonicSyncEventArgs(albumId));
            return result;
        }

        /// <summary>
        /// Ermittelt alle Tracks des übergebenen Interpreten
        /// </summary>
        public virtual async Task<List<Track>> GetArtistTracksAsync(string artist)
        {
            var results = new List<Track>();
            var artistResult = await GetArtistAsync(artist);

            // Tracks asynchron ermitteln
            var trackdetermination = artistResult.Albums.Select(album => GetAlbumTracksAsync(album.Id)).ToList();
            // Auf Ergebnisse warten und zur Rückgabeliste hinzufügen
            foreach (var list in await Task.WhenAll(trackdetermination))
            {
                results.AddRange(list);
            }
            return results;
        }

        /// <summary>
        /// Ermittelt alle Tracks aus dem übergebenen Album
        /// </summary>
        public virtual async Task<List<Track>> GetAlbumTracksAsync(string album)
        {
            var albumResult = await GetAlbumAsync(album);
            var results = albumResult.Tracks.ToList();
            AlbumLoaded?.Invoke(this, new SubsonicSyncEventArgs(album));
            return results;
        }

        /// <summary>
        /// Ermittelt die Top Songs des übergebenen Interpreten
        /// </summary>
        public virtual async Task<List<Track>> GetTopSongsAsync(string artist)
        {
            var results = new List<Track>();
            var param = new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>("artist", artist)};
            var xml = (await GetXmlFromServer("getTopSongs", param)).Item1;
            if (xml != null && xml.ReadToDescendant("topSongs") && xml.ReadToDescendant("song"))
            {
                do
                {
                    results.Add(NewTrackFromXml(xml));
                } while (xml.ReadToNextSibling("song"));
            }
            return results;
        }

        /// <summary>
        /// Ermittelt alle Playlisten vom Server
        /// </summary>
        public virtual async Task<List<Playlist>> GetPlaylistsAsync()
        {
            var results = new List<Playlist>();
            var xml = (await GetXmlFromServer("getPlaylists")).Item1;
            if (xml != null && xml.ReadToDescendant("playlists") && xml.ReadToDescendant("playlist"))
            {
                do
                {
                    results.Add(NewPlaylistFromXml(xml));
                } while (xml.ReadToNextSibling("playlist"));
            }
            return results;
        }

        /// <summary>
        /// Ermittelt alle Tracks einer Playlist
        /// </summary>
        public virtual async Task<List<Track>> GetPlaylistTracksAsync(string playlistId)
        {
            var results = new List<Track>();
            var param = new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>("id", playlistId)};
            var xml = (await GetXmlFromServer("getPlaylist", param)).Item1;
            if (xml != null && xml.ReadToDescendant("playlist") && xml.ReadToDescendant("entry"))
            {
                do
                {
                    results.Add(NewTrackFromXml(xml));
                } while (xml.ReadToNextSibling("entry"));
            }
            return results;
        }

        /// <summary>
        /// Führt zunächsten einen Refresh aus und ermittelt dann alle Podcasts
        /// </summary>
        public virtual async Task<List<Podcast>> GetPodcastsAsync()
        {
            var results = new List<Podcast>();
            var refreshXml = (await GetXmlFromServer("refreshPodcasts")).Item1;
            if (refreshXml == null) return results;
            var param = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("includeEpisodes", "false")
            };
            var xml = (await GetXmlFromServer("getPodcasts", param)).Item1;
            if (xml != null && xml.ReadToDescendant("podcasts") && xml.ReadToDescendant("channel"))
            {
                do
                {
                    var newPodcast = NewPodcastFromXml(xml);
                    if (newPodcast.Status != "error")
                    {
                        results.Add(newPodcast);
                    }
                } while (xml.ReadToNextSibling("channel"));
            }
            return results;
        }

        /// <summary>
        /// Ermittelt alle Episoden eines Podacts
        /// </summary>
        public virtual async Task<List<PodcastEpisode>> GetPodcastEpisodesAsync(string podcastId)
        {
            var results = new List<PodcastEpisode>();
            var param = new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>("id", podcastId)};
            var xml = (await GetXmlFromServer("getPodcasts", param)).Item1;
            if (xml != null && xml.ReadToDescendant("podcasts") && xml.ReadToDescendant("channel") &&
                xml.ReadToDescendant("episode"))
            {
                do
                {
                    results.Add(NewPodcastEpisodeFromXml(xml));
                } while (xml.ReadToNextSibling("episode"));
            }
            return results;
        }

        /// <summary>
        /// Ermittelt die neusten Episoden aller Podacts
        /// </summary>
        public virtual async Task<List<PodcastEpisode>> GetNewestPodcastEpisodesAsync()
        {
            var results = new List<PodcastEpisode>();
            var xml = (await GetXmlFromServer("getNewestPodcasts")).Item1;
            if (xml != null && xml.ReadToDescendant("newestPodcasts") && xml.ReadToDescendant("episode"))
            {
                do
                {
                    results.Add(NewPodcastEpisodeFromXml(xml));
                } while (xml.ReadToNextSibling("episode"));
            }
            return results;
        }

        /// <summary>
        /// Lädt die übergebene Episode auf den Server
        /// </summary>
        public virtual async Task DownloadEpisodeToServerAsync(string episodeId)
        {
            var param = new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>("id", episodeId)};
            await GetXmlFromServer("downloadPodcastEpisode", param);
        }

        /// <summary>
        /// Ermittelt sämtliche Unterordner und Tracks des Top-Folders
        /// </summary>
        public virtual async Task<Folder> GetTopFolderAsync()
        {
            var result = new Folder();
            var xml = (await GetXmlFromServer("getIndexes")).Item1;
            result.Id = "0";
            result.Name = "Folders";
            if (xml != null && xml.ReadToDescendant("indexes") && xml.Read())
            {
                do
                {
                    switch (xml.Reader.Name)
                    {
                        case "shortcut":
                            // noch keine Verwendung für Shortcuts gefunden
                            break;
                        case "index":
                            do
                            {
                                if (xml.ReadToDescendant("artist"))
                                {
                                    do
                                    {
                                        var newFolder = new Folder
                                        {
                                            Id = xml.GetAttribute("id"),
                                            Name = xml.GetAttribute("name")
                                        };
                                        result.Folders.Add(newFolder);
                                    } while (xml.ReadToNextSibling("artist"));
                                }
                            } while (xml.ReadToNextSibling("index"));
                            break;
                        case "child":
                            do
                            {
                                result.Tracks.Add(NewTrackFromXml(xml));
                            } while (xml.ReadToNextSibling("child"));
                            break;
                        default:
                            break;
                    }
                } while (xml.Read());
            }
            return result;
        }

        /// <summary>
        /// Ermittelt sämtliche Unterordner und Tracks 
        /// </summary>
        public virtual async Task<Folder> GetFolderContentAsync(string targetId)
        {
            var result = new Folder();
            var param = new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>("id", targetId)};
            var xml = (await GetXmlFromServer("getMusicDirectory", param)).Item1;
            if (xml != null && xml.ReadToDescendant("directory"))
            {
                result.Id = xml.GetAttribute("id");
                result.Name = xml.GetAttribute("name");
                if (xml.ReadToDescendant("child"))
                {
                    do
                    {
                        if (xml.GetAttribute("isDir") == "true")
                        {
                            var subFolder = new Folder
                            {
                                Id = xml.GetAttribute("id"),
                                Name = xml.GetAttribute("title"),
                                Cover = new CoverArt(xml.GetAttribute("coverArt"))
                            };
                            result.Folders.Add(subFolder);
                        }
                        else if (xml.GetAttribute("isDir") == "false" && xml.GetAttribute("type") == "music")
                        {
                            result.Tracks.Add(NewTrackFromXml(xml));
                        }
                    } while (xml.ReadToNextSibling("child"));
                }
            }
            return result;
        }

        /// <summary>
        /// Sucht auf dem Server nach entsprechenden Interpreten, Alben und Tracks
        /// </summary>
        /// <param name="searchString">Suchparamter den die Objekte im Namen beinhalten sollen</param>
        /// <returns>
        /// Item1: Alle gefundenen Interpreten
        /// Item2: Alle gefundenen Alben
        /// Item3: Alle gefundenen Tracks
        /// </returns>
        public virtual async Task<Tuple<List<Artist>, List<Album>, List<Track>>> GetSearchResultsAsync(
            string searchString)
        {
            var artistResults = new List<Artist>();
            var albumResults = new List<Album>();
            var trackResults = new List<Track>();

            var param = new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>("query", searchString)};
            var xml = (await GetXmlFromServer("search3", param)).Item1;
            if (xml != null && xml.ReadToDescendant("searchResult3") && xml.Read())
            {
                do
                {
                    switch (xml.Reader.Name)
                    {
                        case "artist":
                            artistResults.Add(NewArtistFromXml(xml));
                            break;
                        case "album":
                            albumResults.Add(NewAlbumFromXml(xml));
                            break;
                        case "song":
                            trackResults.Add(NewTrackFromXml(xml));
                            break;
                        default:
                            break;
                    }
                } while (xml.Read());
            }

            var result = new Tuple<List<Artist>, List<Album>, List<Track>>(artistResults, albumResults, trackResults);
            return result;
        }

        /// <summary>
        /// Gibt je nach übergebenen Typ eine Auswahl an Alben zurück
        /// </summary>
        /// <param name="type">Art der Alben, die ermittelt werden sollen</param>
        /// <param name="count">Maximale Anzahl der Alben</param>
        public virtual async Task<List<Album>> GetAlbumSectionAsync(AlbumListType type, int count)
        {
            var results = new List<Album>();
            string typeValue;
            switch (type)
            {
                case AlbumListType.Random:
                    typeValue = "random";
                    break;
                case AlbumListType.Recent:
                    typeValue = "recent";
                    break;
                case AlbumListType.Frequent:
                    typeValue = "frequent";
                    break;
                case AlbumListType.Newest:
                    typeValue = "newest";
                    break;
                default:
                    typeValue = "random";
                    break;
            }

            var param = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("type", typeValue),
                new KeyValuePair<string, string>("size", count.ToString())
            };
            var xml = (await GetXmlFromServer("getAlbumList2", param)).Item1;

            if (xml == null || !xml.ReadToDescendant("albumList2") || !xml.ReadToDescendant("album")) return results;
            do
            {
                results.Add(NewAlbumFromXml(xml));
            } while (xml.ReadToNextSibling("album"));
            return results;
        }

        public virtual async Task<bool> ScrobbleAsync(string trackId)
        {
#if DEBUG
            return true;
#else
            var param = new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>("id", trackId)};
            var xml = (await GetXmlFromServer("scrobble", param)).Item1;

            return xml != null;
#endif
        }

        #endregion

        #region Playlist Handling

        /// <summary>
        /// Erstellt eine neue Playlist auf dem Server
        /// </summary>
        /// <param name="name">Name der Playliste</param>
        /// <param name="tracks">Tracks der Playliste</param>
        /// <returns>
        /// Tuple Item 1: true - Playlist erfolgreich angelegt
        /// Tuple Item 2: Vom Server zurückgegebene Fehlermeldung
        /// </returns>
        public virtual async Task<Tuple<bool, string>> CreatePlaylistAsync(string name, IList<Track> tracks)
        {
            var param = new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>("name", name)};
            param.AddRange(tracks.Select(track => new KeyValuePair<string, string>("songId", track.Id)));
            var result = await SendToServer("createPlaylist", param);
            return result.Item1 ? Tuple.Create(true, string.Empty) : result;
        }

        /// <summary>
        /// Fügt Tracks zur übergebenen Playlist hinzu
        /// </summary>
        /// <param name="playlistId">Playlist, die die neuen Tracks erhalten soll</param>
        /// <param name="tracks">Hinzuzufügende Tracks</param>
        /// <returns>
        /// Tuple Item 1: true - Tracks erfolgreich hinzugefügt
        /// Tuple Item 2: Vom Server zurückgegebene Fehlermeldung
        /// </returns>
        public virtual async Task<Tuple<bool, string>> AddTracksToPlaylistAsync(string playlistId, IList<Track> tracks)
        {
            return await UpdatePlaylistAsync(playlistId, null, tracks);
        }

        /// <summary>
        /// Aktualisiert die Playlist auf den Server
        /// </summary>
        /// <param name="playlistId">Playlist, die aktualisiert werden soll</param>
        /// <param name="newName">Neuer Name</param>
        /// <param name="tracksToAdd">Hinzuzufügende Tracks</param>
        /// <param name="tracksToRemove">Zu entfernende Tracks</param>
        /// <returns>
        /// Tuple Item 1: true - Update erfolgreich durchgeführt
        /// Tuple Item 2: Vom Server zurückgegebene Fehlermeldung
        /// </returns>
        public virtual async Task<Tuple<bool, string>> UpdatePlaylistAsync(string playlistId, string newName = null,
            IList<Track> tracksToAdd = null, IList<Track> tracksToRemove = null)
        {
            var param = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("playlistId", playlistId)
            };

            if (newName != null)
            {
                param.Add(new KeyValuePair<string, string>("name", newName));
            }
            if (tracksToAdd != null)
            {
                param.AddRange(tracksToAdd.Select(track => new KeyValuePair<string, string>("songIdToAdd", track.Id)));
            }
            if (tracksToRemove != null)
            {
                param.AddRange(
                    tracksToRemove.Select(track => new KeyValuePair<string, string>("songIndexToRemove", track.Id)));
            }
            var result = await SendToServer("updatePlaylist", param);
            return result.Item1 ? Tuple.Create(true, string.Empty) : result;
        }

        /// <summary>
        /// Löscht die Playlist vom Server
        /// </summary>
        /// <param name="playlistId"></param>
        /// <returns>
        /// Tuple Item 1: true - Playlist erfolgreich gelöscht
        /// Tuple Item 2: Vom Server zurückgegebene Fehlermeldung
        /// </returns>
        public virtual async Task<Tuple<bool, string>> DeletePlaylistAsync(string playlistId)
        {
            var param = new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>("id", playlistId)};
            var result = await SendToServer("deletePlaylist", param);
            return result.Item1 ? Tuple.Create(true, string.Empty) : result;
        }

        #endregion
    }
}