using System;
using System.Xml;

namespace SonicStreamer.Common.Extension
{
    public class CustomXmlReader : IDisposable
    {
        public XmlReader Reader { get; private set; }

        private CustomXmlReader()
        {
        }

        public static CustomXmlReader Create(XmlReader reader)
        {
            return new CustomXmlReader
            {
                Reader = reader
            };
        }

        /// <summary>
        /// Erweitert die <see cref="Read"/> Methode und 
        /// überspringt einen Knoten falls dieser ungültige Zeichen o.ä. enthält
        /// </summary>
        public bool Read()
        {
            try
            {
                return Reader.Read();
            }
            catch (Exception)
            {
                return Read();
            }
        }

        /// <summary>
        /// Erweitert die <see cref="ReadToNextSibling"/> Methode und 
        /// überspringt einen Knoten falls dieser ungültige Zeichen o.ä. enthält
        /// </summary>
        /// <param name="node">Name des Knoten</param>
        public bool ReadToNextSibling(string node)
        {
            try
            {
                return Reader.ReadToNextSibling(node);
            }
            catch (Exception)
            {
                return ReadToNextSibling(node);
            }
        }

        /// <summary>
        /// Erweitert die <see cref="ReadToDescendant"/> Methode und 
        /// überspringt einen Knoten falls dieser ungültige Zeichen o.ä. enthält
        /// </summary>
        /// <param name="node">Name des Knoten</param>
        public bool ReadToDescendant(string node)
        {
            try
            {
                return Reader.ReadToDescendant(node);
            }
            catch (Exception)
            {
                return ReadToDescendant(node);
            }
        }

        /// <summary>
        /// Erweitert die <see cref="ReadToFollowing"/> Methode und 
        /// überspringt einen Knoten falls dieser ungültige Zeichen o.ä. enthält
        /// </summary>
        /// <param name="node">Name des Knoten</param>
        public bool ReadToFollowing(string node)
        {
            try
            {
                return Reader.ReadToFollowing(node);
            }
            catch (Exception)
            {
                return ReadToFollowing(node);
            }
        }

        /// <summary>
        /// Erweitert die <see cref="GetAttribute"/> Methode und 
        /// übergibt <see cref="string.Empty"/> falls das Attribut 
        /// ungültige Zeichen o.ä. enthält
        /// </summary>
        /// <param name="attribute">Name des Attribut</param>
        public string GetAttribute(string attribute)
        {
            return GetAttribute(attribute, string.Empty);
        }

        /// <summary>
        /// Erweitert die <see cref="GetAttribute"/> Methode und 
        /// übergibt <paramref name="fallbackValue"/> falls das Attribut 
        /// ungültige Zeichen o.ä. enthält
        /// </summary>
        /// <param name="attribute">Name des Attribut</param>
        /// <param name="fallbackValue">Rückgabewert falls das Lesen des Attributs fehlschlägt</param>
        public string GetAttribute(string attribute, string fallbackValue)
        {
            try
            {
                var result = Reader.GetAttribute(attribute);
                return result ?? fallbackValue;
            }
            catch
            {
                return fallbackValue;
            }
        }

        /// <summary>
        /// Erweitert die <see cref="ReadElementContentAsString"/> Methode und 
        /// übergibt <see cref="string.Empty"/> falls das Attribut 
        /// ungültige Zeichen o.ä. enthält
        /// </summary>
        /// <param name="attribute">Name des Attribut</param>
        public string ReadElementContentAsString()
        {
            return ReadElementContentAsString(string.Empty);
        }

        /// <summary>
        /// Erweitert die <see cref="ReadElementContentAsString"/> Methode und 
        /// übergibt <paramref name="fallbackValue"/> falls das Attribut 
        /// ungültige Zeichen o.ä. enthält
        /// </summary>
        /// <param name="fallbackValue">Rückgabewert falls das Lesen des Attributs fehlschlägt</param>
        public string ReadElementContentAsString(string fallbackValue)
        {
            try
            {
                return Reader.ReadElementContentAsString();
            }
            catch
            {
                return fallbackValue;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Reader.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gibt eine neue <see cref="CustomXmlReader"/>-Instanz aus der <see cref="ReadSubtree"/>
        /// -Methode der <see cref="XmlReader"/>-Klasse zurück, die zum Lesen des aktuellen
        /// Knotens und aller Nachfolgerknoten verwendet werden kann.
        /// </summary>
        public CustomXmlReader ReadSubtree()
        {
            return new CustomXmlReader
            {
                Reader = Reader.ReadSubtree()
            };
        }

        /// <summary>
        /// Erweitert die <see cref="MoveToContent"/> Methode und Überprüft, ob der aktuelle Knoten 
        /// ein Inhaltsknoten ist. Im Falle einer Exception wird <see cref="XmlNodeType"/> None
        /// zurückgegeben
        /// </summary>
        public XmlNodeType MoveToContent()
        {
            try
            {
                return Reader.MoveToContent();
            }
            catch (Exception)
            {
                return XmlNodeType.None;
            }
        }
    }
}