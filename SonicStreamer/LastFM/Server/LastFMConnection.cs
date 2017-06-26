using SonicStreamer.Common.Extension;
using SonicStreamer.Common.System;
using SonicStreamer.LastFM.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace SonicStreamer.LastFM.Server
{
    public class LastFmConnection : BaseXmlServerConnection
    {
        #region BaseServerConnection Implementation

        /// <summary>
        /// Baut die URI Adresse für die Verbindung zu LastFM zusammen.
        /// </summary>
        /// <param name="method">LastFM API-Methodenname</param>
        /// <param name="param">Zu übergebene Parameter</param>
        public override string GetApiMethodUri(string method, List<KeyValuePair<string, string>> param)
        {
            var result = string.Format("http://ws.audioscrobbler.com/2.0/?api_key={0}&method={1}", Constants.Secrets.LastfmApiKey, method);
            return param.Aggregate(result, (current, item) => current + $"&{item.Key}={item.Value}");
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
            if (reader.ReadToFollowing("lfm"))
            {
                if (reader.GetAttribute("status").Equals("ok"))
                {
                    return Tuple.Create(true, string.Empty);
                }
                if (reader.GetAttribute("status").Equals("failed") && reader.ReadToDescendant("error"))
                {
                    return Tuple.Create(false, "Invalid Command");
                }
                return Tuple.Create(false, "Connection to server successful but error in response.");
            }
            else
            {
                return Tuple.Create(false, "No LastFM response");
            }
        }

        #endregion

        #region LastFMArtist Objekterstellung

        /// <summary>
        /// Ermittelt unter dem angegebenen Interpreten alle Informationen von LastFM
        /// und gibt diese in einem LastFM Objekt zurück
        /// </summary>
        /// <param name="name">Name des Interpreten</param>
        public async Task<LastFmArtist> GetLastFmArtistAsync(string name)
        {
            var result = new LastFmArtist();
            var param = new List<KeyValuePair<string, string>>();
            var failedReading = false;

            param.Add(new KeyValuePair<string, string>("artist", name));
            var xml = (await GetXmlFromServer("artist.getInfo", param)).Item1;
            if (xml != null)
            {
                if (xml.ReadToDescendant("artist") && xml.Read())
                {
                    do
                    {
                        if (xml.Reader.NodeType != XmlNodeType.Element || xml.Reader.Depth != 2) continue;
                        switch (xml.Reader.Name)
                        {
                            case "name":
                                result.Name = xml.ReadElementContentAsString();
                                break;
                            case "mbid":
                                result.MusicBrainzId = xml.ReadElementContentAsString();
                                break;
                            case "url":
                                result.LastFmUrl = xml.ReadElementContentAsString();
                                break;
                            case "image":
                                if (xml.Reader.HasAttributes)
                                {
                                    var newImage = GetLastFmCover(xml);
                                    if (newImage != null) result.Cover = newImage;
                                }
                                break;
                            case "bio":
                                var bioInfo = ReadBiographyFromXml(xml.ReadSubtree());
                                string bioInfoValue;
                                if (bioInfo.TryGetValue("summary", out bioInfoValue)) result.Biography = bioInfoValue;
                                break;
                            case "tags":
                                foreach (var item in ReadTagsFromXml(xml.ReadSubtree()))
                                {
                                    result.Tags.Add(item);
                                }
                                break;
                            case "similar":
                                foreach (var similarArtist in ReadSimilarArtists(xml.ReadSubtree()))
                                {
                                    result.SimilarArtists.Add(similarArtist);
                                }
                                break;
                            default:
                                break;
                        }
                    } while (xml.Read());
                }
                else
                {
                    failedReading = true;
                }
            }

            if (failedReading) result = new LastFmArtist();
            return result;
        }

        #region LastFMArtist Hilfsmethoden für das Auslesen von einzelnen Knoten

        /// <summary>
        /// Gibt die Url Adresse des LastFM Covers zurück. Gibt null zurück, wenn der Knoten
        /// nicht das Attribut für die übergebene Größe besitzt.
        /// </summary>
        /// <param name="xml">Image Knoten</param>
        /// <param name="size">Größe die das Cover haben muss</param>
        private static LastFmImage GetLastFmCover(CustomXmlReader xml)
        {
            while (xml.Reader.MoveToNextAttribute())
            {
                if (xml.Reader.Name != "size" || xml.Reader.Value != "mega") continue;
                if (xml.MoveToContent() == XmlNodeType.Element)
                {
                    return new LastFmImage(xml.ReadElementContentAsString());
                }
            }
            return null;
        }

        /// <summary>
        /// Gibt alle ähnlichen Interpreten vom übergebenen Knoten zurück
        /// </summary>
        private static IEnumerable<LastFmObject> ReadSimilarArtists(CustomXmlReader reader)
        {
            var results = new List<LastFmObject>();

            // Knoten "similar" auslesen
            while (reader.Read())
            {
                if (reader.Reader.NodeType != XmlNodeType.Element || reader.Reader.Depth != 1 ||
                    reader.Reader.Name != "artist") continue;

                #region Knoten "artist" auslesen, Objekt erzeugen und zurückgeben

                using (var artistReader = reader.ReadSubtree())
                {
                    var newSimilarArtist = new LastFmObject();
                    while (artistReader.Read())
                    {
                        if (artistReader.Reader.NodeType != XmlNodeType.Element) continue;
                        switch (artistReader.Reader.Name)
                        {
                            case "name":
                                newSimilarArtist.Name = artistReader.ReadElementContentAsString();
                                break;
                            case "url":
                                newSimilarArtist.LastFmUrl = artistReader.ReadElementContentAsString();
                                break;
                            case "image":
                                if (artistReader.Reader.HasAttributes)
                                {
                                    var newImage = GetLastFmCover(artistReader);
                                    if (newImage != null) newSimilarArtist.Cover = newImage;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    results.Add(newSimilarArtist);
                }

                #endregion
            }

            return results;
        }

        /// <summary>
        /// Ließt den Tags Knoten des übergebenen XML aus
        /// </summary>
        /// <param name="reader">XMLReader Instanz die den Tags Knoten beinhaltet</param>
        /// <returns>
        /// Eine Liste von Tags
        /// Item 1: Name des Tags
        /// Item 2: Url des Tags zu LastFM
        /// </returns>
        private static IEnumerable<LastFmTag> ReadTagsFromXml(CustomXmlReader reader)
        {
            var results = new List<LastFmTag>();

            while (reader.Read())
            {
                if (reader.Reader.NodeType != XmlNodeType.Element || reader.Reader.Depth != 1 ||
                    reader.Reader.Name != "tag") continue;
                var name = reader.ReadToDescendant("name") ? reader.ReadElementContentAsString() : null;
                var url = reader.ReadToNextSibling("url") ? reader.ReadElementContentAsString() : null;
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(url))
                {
                    results.Add(new LastFmTag {Name = name, Uri = url});
                }
            }

            return results;
        }

        /// <summary>
        /// Ließt den Bio Knoten des übergebenen XML aus
        /// </summary>
        /// <param name="reader">XMLReader Instanz die den Bio Knoten beinhaltet</param>
        private static Dictionary<string, string> ReadBiographyFromXml(CustomXmlReader reader)
        {
            var result = new Dictionary<string, string>();

            while (reader.Read())
            {
                if (reader.Reader.NodeType != XmlNodeType.Element || reader.Reader.Depth != 1) continue;
                switch (reader.Reader.Name)
                {
                    case "summary":
                        if (!result.ContainsKey("summary"))
                        {
                            var htmlBio = reader.ReadElementContentAsString();
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

                                result.Add("summary", noHtmlNormalisedBio);
                            }
                        }
                        break;
                    case "placeformed":
                        if (!result.ContainsKey("placeformed"))
                        {
                            result.Add("placeformed", reader.ReadElementContentAsString());
                        }
                        break;
                    case "yearformed":
                        if (!result.ContainsKey("yearformed"))
                        {
                            result.Add("yearformed", reader.ReadElementContentAsString());
                        }
                        break;
                    default:
                        break;
                }
            }

            return result;
        }

        #endregion

        #endregion
    }
}