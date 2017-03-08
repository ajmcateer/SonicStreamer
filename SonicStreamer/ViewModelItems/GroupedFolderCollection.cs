using SonicStreamer.Subsonic.Data;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SonicStreamer.ViewModelItems
{
    public class GroupedFolderColletion : List<Folder>
    {
        [XmlAttribute]
        public string Key { get; set; }

        public new IEnumerator<Folder> GetEnumerator()
        {
            return base.GetEnumerator();
        }
    }
}