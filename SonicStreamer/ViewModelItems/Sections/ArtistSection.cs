using System.Collections.ObjectModel;
using System.Threading.Tasks;
using SonicStreamer.Common.System;
using SonicStreamer.Subsonic.Data;
using Microsoft.Toolkit.Uwp.Services.Twitter;

namespace SonicStreamer.ViewModelItems.Sections
{
    public class ArtistSection : BaseSection
    {
        #region Properties & Fields

        private ArtistInfo _artistInfo;

        public ArtistInfo ArtistInfo
        {
            get { return _artistInfo; }
            set { Set(ref _artistInfo, value); }
        }

        public ObservableCollection<Album> Albums { get; set; }

        private readonly string _artistId, _artistName;

        #endregion

        public ArtistSection()
        {
            ContentTemplateResourceKey = "ArtistSectionTemplate";
            Albums = new ObservableCollection<Album>();
        }

        public ArtistSection(string title, string artistId, string artistName, int index) : this()
        {
            Index = index;
            Title = title;
            _artistId = artistId;
            _artistName = artistName;
        }

        public override async Task LoadDataAsync()
        {
            ArtistInfo = await ArtistInfo.CreateAsync(_artistId, _artistName, new ArtistInfo.ActiveServices
            {
                LastFm = true,
                MusicBrainz = true,
                Twitter = true
            });
            Albums = (await SubsonicConnector.Current.CurrentConnection.GetArtistAsync(_artistId)).Albums;
        }
    }
}
