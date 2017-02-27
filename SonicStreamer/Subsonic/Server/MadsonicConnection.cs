using SonicStreamer.Common.Extension;
using SonicStreamer.Subsonic.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SonicStreamer.Subsonic.Server
{
    public class MadsonicConnection : BaseSubsonicConnection
    {
        #region Constants

        private const string APPNAME = "SonicStreamer";

        #endregion

        public MadsonicConnection() : base()
        {
            ServerApiVersion = "2.0.0";
            FallbackServerApiVersion = "2.0.0";
        }

        #region BaseServerConnection Implementation

        /// <summary>
        /// Baut die URI Adresse für die Verbindung zu LastFM zusammen.
        /// </summary>
        /// <param name="method">LastFM API-Methodenname</param>
        /// <param name="param">Zu übergebene Parameter</param>
        public override string GetApiMethodUri(string method, List<KeyValuePair<string, string>> param)
        {
            var url = $"{ServerUrl}/rest2/{method}.view?v={ServerApiVersion}&c={APPNAME}&u={User}&p={Password}";
            return param.Aggregate(url, (current, item) => current + string.Format("&{0}={1}", item.Key, item.Value));
        }

        /// <summary>
        /// Prüft das XML Dokument auf einen gültigen LastFM Response
        /// </summary>
        /// <returns>
        /// Tuple Item 1: true - gültiges LastFM XML Dokument
        /// Tuple Item 2: Vom Server zurückgegebene Fehlermeldung
        /// </returns>
        protected override Tuple<bool, string> CheckStatus(ref CustomXmlReader reader)
        {
            if (!reader.ReadToFollowing("madsonic-response")) return Tuple.Create(false, "No Madsonic response");
            if (reader.GetAttribute("status").Equals("ok"))
            {
                return Tuple.Create(true, string.Empty);
            }
            if (reader.GetAttribute("status").Equals("failed") && reader.ReadToDescendant("error"))
            {
                return Tuple.Create(false, reader.GetAttribute("message"));
            }
            return Tuple.Create(false, "Connection to server successful but error in response.");
        }

        #endregion

        #region BaseSubsonicConnection Implementation

        protected override string GetPingApiMethodUri(string server, string user, string password)
        {
            return $"{server}/rest2/ping.view?v={ServerApiVersion}&c={APPNAME}&u={user}&p={password}";
        }

        /// <summary>
        /// Ermittelt die Top Songs des übergebenen Interpreten
        /// </summary>
        public override async Task<List<Track>> GetTopSongsAsync(string artist)
        {
            return new List<Track>();
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
        public override async Task<Tuple<List<Artist>, List<Album>, List<Track>>> GetSearchResultsAsync(
            string searchString)
        {
            var artistResults = new List<Artist>();
            var albumResults = new List<Album>();
            var trackResults = new List<Track>();

            var param = new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>("query", searchString)};
            var xml = (await GetXmlFromServer("searchID3", param)).Item1;

            if (xml != null)
            {
                if (xml.ReadToDescendant("searchResultID3") && xml.Read())
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
            }

            var result = new Tuple<List<Artist>, List<Album>, List<Track>>(artistResults, albumResults, trackResults);
            return result;
        }

        /// <summary>
        /// Gibt je nach übergebenen Typ eine Auswahl an Alben zurück
        /// </summary>
        /// <param name="type">Art der Alben, die ermittelt werden sollen</param>
        /// <param name="count">Maximale Anzahl der Alben</param>
        public override async Task<List<Album>> GetAlbumSectionAsync(AlbumListType type, int count)
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
            var xml = (await GetXmlFromServer("getAlbumListID3", param)).Item1;
            if (xml == null || !xml.ReadToDescendant("albumListID3") || !xml.ReadToDescendant("album")) return results;
            do
            {
                results.Add(NewAlbumFromXml(xml));
            } while (xml.ReadToNextSibling("album"));
            return results;
        }

        #endregion
    }
}