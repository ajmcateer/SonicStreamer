using SonicStreamer.Common.Extension;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.Web.Http;

namespace SonicStreamer.Common.System
{
    public abstract class BaseXmlServerConnection
    {
        /// <summary>
        /// Zentrale <see cref="Windows.Web.Http.HttpClient"/> Instanz, bspw. für <see cref="Windows.Web.Http.Filters.HttpBaseProtocolFilter"/> Einstellungen
        /// </summary>
        protected HttpClient HttpClient;

        protected BaseXmlServerConnection()
        {
            HttpClient = new HttpClient();
        }

        /// <summary>
        /// Baut eine Uri Adresse mit den Verbindungsdaten auf, um einen Request an den Server senden zu können.
        /// </summary>
        /// <param name="method">API-Methode</param>
        /// <param name="param">Parameter</param>
        /// <returns>Uri mit sämtlichen benötigten Daten für einen WebRequest</returns>
        public abstract string GetApiMethodUri(string method, List<KeyValuePair<string, string>> param);

        #region Server Communication and Validation

        /// <summary>
        /// Sendet einen Befehl an den Subsonic Server und gibt ein validiertes XML Dokument im Reader
        /// für die weitere Verarbeitung zurück. Der Pointer steht dabei auf dem erstem Knoten nach
        /// dem Status-Knoten.
        /// </summary>
        /// <param name="method">Subsonic API-Methodenname</param>
        /// <returns>
        /// Tupel Item 1: XMLReader mit dem validierten XML Dokument. Enhält null, wenn das XML
        /// Dokument ungültig ist
        /// Tupel Item 2: Vom Server zurückgegebene Fehlermeldung
        /// </returns>
        protected async Task<Tuple<CustomXmlReader, string>> GetXmlFromServer(string method)
        {
            return await GetXmlFromServer(method, new List<KeyValuePair<string, string>>());
        }

        /// <summary>
        /// Sendet einen Befehl an den Subsonic Server und gibt ein validiertes XML Dokument im Reader
        /// für die weitere Verarbeitung zurück. Der Pointer steht dabei auf dem erstem Knoten nach
        /// dem Status-Knoten.
        /// </summary>
        /// <param name="method">Subsonic API-Methodenname</param>
        /// <param name="param">Zu übergebene Parameter</param>
        /// <returns>
        /// Tupel Item 1: XMLReader mit dem validierten XML Dokument. Enhält null, wenn das XML
        /// Dokument ungültig ist
        /// Tupel Item 2: Vom Server zurückgegebene Fehlermeldung
        /// </returns>
        protected async Task<Tuple<CustomXmlReader, string>> GetXmlFromServer(string method,
            List<KeyValuePair<string, string>> param)
        {
            try
            {
                var uri = new Uri(GetApiMethodUri(method, param));
                var request = new HttpRequestMessage(HttpMethod.Get, uri);
                request.Headers.IfModifiedSince = new DateTimeOffset(DateTime.Now);
                var response = await HttpClient.SendRequestAsync(request);
                var responseStream = await response.Content.ReadAsInputStreamAsync();
                var stream = responseStream.AsStreamForRead();
                var streamReader = new StreamReader(stream, Encoding.GetEncoding("utf-8"));
                var reader = CustomXmlReader.Create(XmlReader.Create(streamReader));
                var checkResult = CheckStatus(ref reader);
                return Tuple.Create(reader, checkResult.Item1 ? string.Empty : checkResult.Item2);
            }
            catch (Exception e)
            {
                return Tuple.Create((CustomXmlReader) null, "Error during communication with server: " + e.Message);
            }
        }

        /// <summary>
        /// Sendet einen Befehl an den Subsonic Server und prüft, ob die Verarbeitung erfolgreich war. 
        /// </summary>
        /// <param name="method">Subsonic API-Methodenname</param>
        /// <returns>
        /// Tuple Item 1: true - Befehl erfolgreich durchgeführt
        /// Tuple Item 2: Vom Server zurückgegebene Fehlermeldung
        /// </returns>
        protected async Task<Tuple<bool, string>> SendToServer(string method)
        {
            return await SendToServer(method, new List<KeyValuePair<string, string>>());
        }

        /// <summary>
        /// Sendet einen Befehl an den Subsonic Server und prüft, ob die Verarbeitung erfolgreich war. 
        /// </summary>
        /// <param name="method">Subsonic API-Methodenname</param>
        /// <param name="param">Zu übergebene Parameter</param>
        /// <returns>
        /// Tuple Item 1: true - Befehl erfolgreich durchgeführt
        /// Tuple Item 2: Vom Server zurückgegebene Fehlermeldung
        /// </returns>
        protected async Task<Tuple<bool, string>> SendToServer(string method, List<KeyValuePair<string, string>> param)
        {
            var checkResult = await GetXmlFromServer(method, param);
            return checkResult.Item1 != null ? Tuple.Create(true, string.Empty) : Tuple.Create(false, checkResult.Item2);
        }

        /// <summary>
        /// Prüft das XML Dokument auf einen gültigen Subsonic Response
        /// </summary>
        /// <returns>
        /// Tuple Item 1: true - gültiges Subsonic XML Dokument
        /// Tuple Item 2: Vom Server zurückgegebene Fehlermeldung
        /// </returns>
        protected abstract Tuple<bool, string> CheckStatus(ref CustomXmlReader reader);

        #endregion
    }
}