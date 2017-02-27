using SonicStreamer.Common.System;
using SonicStreamer.SampleData;
using SonicStreamer.Subsonic.Data;
using SonicStreamer.Subsonic.Server;
using SonicStreamer.ViewModelItems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;

namespace SonicStreamer.ViewModels
{
    /// <summary>
    /// Stellt eine Auflistung von Artists oder Alben dar
    /// </summary>
    public class AlbumViewModel : AbstractListingViewModel, IViewModelSerializable
    {
        public AlbumViewModel() : base()
        {
            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled) return;
            var sampleItems = SampleDataCreator.Current.CreateAlbums().Select(album => new ListingItem(album)).ToList();
            var query = from album in sampleItems
                orderby album.MusicObject.Name
                group album by album.MusicObject.Name.Substring(0, 1).ToLower()
                into g
                select new {GroupName = g.Key, Items = g};
            foreach (var g in query)
            {
                var group = new GroupedListingItemColletion {Key = g.GroupName};
                @group.AddRange(g.Items);
                Items.Add(@group);
            }
            ListingPageTitle = "albums";
            IsLoading = true;
            LoadedItems = 10;
            LoadingCount = 25;
        }

        private void AlbumLoaded(object sender, SubsonicSyncEventArgs args)
        {
            LoadedItems++;
        }

        /// <summary>
        /// Lädt alle Alben vom Server falls dies noch nicht erfolgt ist
        /// </summary>
        public override async Task LoadDataAsync()
        {
            if (!IsLoaded)
            {
                var items = new List<ListingItem>();
                var artistdetermination = new List<Task<Artist>>();

                // Daten initalisieren
                InitializeStandardValues();

                // Laden vorbereiten
                LoadedItems = 0;
                LoadingCount = 1; // sorgt dafür, dass zu Beginn der Balken nicht sofort voll ist
                IsLoading = true;
                var artists = await SubsonicConnector.Current.CurrentConnection.GetArtistsAsync();
                LoadingCount = artists.Count;
                SubsonicConnector.Current.CurrentConnection.AlbumLoaded += AlbumLoaded;

                // Alben der Interpeten asynchron ermitteln
                foreach (var artist in artists)
                {
                    artistdetermination.Add(SubsonicConnector.Current.CurrentConnection.GetArtistAsync(artist.Id));

                    // Tasks abarbeiten um Speicher wieder zu leeren
                    if (artistdetermination.Count != 200) continue;
                    // Alben sammeln und entsprechende Items daraus erstellen
                    items.AddRange(from item in await Task.WhenAll(artistdetermination)
                        from album in item.Albums
                        select new ListingItem(album));
                    artistdetermination.Clear();
                }

                // Alben sammeln und entsprechende Items daraus erstellen
                items.AddRange(from artist in await Task.WhenAll(artistdetermination)
                    from album in artist.Albums
                    select new ListingItem(album));

                // Alben sortieren und gruppieren
                var query = from album in items
                    orderby album.MusicObject.Name
                    group album by album.MusicObject.Name.Substring(0, 1).ToLower()
                    into g
                    select new {GroupName = g.Key, Items = g};
                var alphaQuery = from alphaGroup in query
                    where Alphabet.Contains(alphaGroup.GroupName)
                    select alphaGroup;
                var nonAlphaQuery = from nonAlphaGroup in query
                    where !Alphabet.Contains(nonAlphaGroup.GroupName)
                    select nonAlphaGroup;

                foreach (var alphaGroup in alphaQuery)
                {
                    var alphaGroupItem = new GroupedListingItemColletion {Key = alphaGroup.GroupName};
                    alphaGroupItem.AddRange(alphaGroup.Items);
                    Items.Add(alphaGroupItem);
                }

                if (nonAlphaQuery.Any())
                {
                    var nonAlphaGroupItem = new GroupedListingItemColletion();
                    foreach (var nonAlphaGroup in nonAlphaQuery)
                    {
                        nonAlphaGroupItem.Key = "#";
                        nonAlphaGroupItem.AddRange(nonAlphaGroup.Items);
                    }
                    Items.Add(nonAlphaGroupItem);
                }

                // Laden abschließen
                SubsonicConnector.Current.CurrentConnection.AlbumLoaded -= AlbumLoaded;
                IsLoading = false;
                LoadedItems = 0;
                LoadingCount = 0;
                IsLoaded = true;
            }
        }

        /// <summary>
        /// Setzt Standardwerte beim Laden / Wiederherstellen des ViewModels
        /// </summary>
        protected override void InitializeStandardValues()
        {
            base.InitializeStandardValues();
            ListingPageTitle = "albums";
        }

        public async Task SaveViewModelAsync(string savename)
        {
            try
            {
                var saveFolder = ApplicationData.Current.LocalFolder;
                var saveFile = await saveFolder.CreateFileAsync(savename + ".xml",
                    CreationCollisionOption.ReplaceExisting);
                using (var saveStream = await saveFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var serializer = new XmlSerializer(typeof(AlbumViewModel));
                    serializer.Serialize(saveStream.AsStreamForWrite(), this);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public async Task RestoreViewModelAsync(string loadname)
        {
            try
            {
                var loadFolder = ApplicationData.Current.LocalFolder;
                var loadFile = await loadFolder.GetFileAsync(loadname + ".xml");
                using (var loadStream = await loadFile.OpenAsync(FileAccessMode.Read))
                {
                    var serializer = new XmlSerializer(typeof(AlbumViewModel));
                    var newVm = serializer.Deserialize(loadStream.AsStreamForRead()) as AlbumViewModel;

                    // Standardwerte setzen
                    InitializeStandardValues();

                    // Werte kopieren
                    Items = newVm?.Items;
                    if (Items != null && Items.Count > 0)
                    {
                        // Workaround: GroupKeys wiederherstellen
                        foreach (var itemGroup in Items)
                        {
                            var newGroupKey = itemGroup.First().MusicObject.Name.Substring(0, 1).ToLower();
                            itemGroup.Key = Alphabet.Contains(newGroupKey) ? newGroupKey : "#";
                        }
                        if (newVm != null) ListingPageTitle = newVm.ListingPageTitle;
                        IsLoaded = true;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}