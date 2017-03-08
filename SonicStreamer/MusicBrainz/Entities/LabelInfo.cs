using System.Xml.Serialization;

namespace SonicStreamer.MusicBrainz.Entities
{
    [XmlRoot("label-info")]
    public class LabelInfo
    {
        /// <summary>
        /// Gets or sets the catalog-number.
        /// </summary>
        [XmlElement("catalog-number")]
        public string CatalogNumber { get; set; }

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        [XmlElement("label")]
        public Label Label { get; set; }
    }
}