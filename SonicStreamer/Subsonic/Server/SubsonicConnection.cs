using SonicStreamer.Common.Extension;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonicStreamer.Subsonic.Server
{
    public class SubsonicConnection : BaseSubsonicConnection
    {
        #region Constants

        private const string Appname = "SonicStreamer";

        #endregion

        public SubsonicConnection() : base()
        {
            ServerApiVersion = "1.0.0";
            FallbackServerApiVersion = "1.0.0";
        }

        #region BaseServerConnection Implementation

        /// <summary>
        /// Baut die URI Adresse für die Verbindung zu LastFM zusammen.
        /// </summary>
        /// <param name="method">LastFM API-Methodenname</param>
        /// <param name="param">Zu übergebene Parameter</param>
        public override string GetApiMethodUri(string method, List<KeyValuePair<string, string>> param)
        {
            var url = $"{ServerUrl}/rest/{method}.view?v={ServerApiVersion}&c={Appname}&u={User}&p={Password}";
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
            if (reader.ReadToFollowing("subsonic-response"))
            {
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
            else
            {
                return Tuple.Create(false, "No Subsonic response");
            }
        }

        #endregion

        #region BaseSubsonicConnection Implementation

        protected override string GetPingApiMethodUri(string server, string user, string password)
        {
            return $"{server}/rest/ping.view?v={ServerApiVersion}&c={Appname}&u={user}&p={password}";
        }

        #endregion
    }
}