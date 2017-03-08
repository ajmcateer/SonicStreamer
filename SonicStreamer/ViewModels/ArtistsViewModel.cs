using SonicStreamer.Common.System;
using SonicStreamer.SampleData;
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
    public class ArtistsViewModel : AbstractListingViewModel, IViewModelSerializable
    {
        public ArtistsViewModel() : base()
        {
            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled) return;
            var sampleItems =
                SampleDataCreator.Current.CreateArtists().Select(artist => new ListingItem(artist)).ToList();
            var query = from artist in sampleItems
                orderby artist.MusicObject.Name
                group artist by artist.MusicObject.Name.Substring(0, 1).ToLower()
                into g
                select new {GroupName = g.Key, Items = g};
            foreach (var g in query)
            {
                var group = new GroupedListingItemColletion {Key = g.GroupName};
                @group.AddRange(g.Items);
                Items.Add(@group);
            }
            ListingPageTitle = "artists";
            IsLoading = true;
            LoadedItems = 10;
            LoadingCount = 25;
        }

        /// <summary>
        /// Lädt alle Interpreten vom Server falls die noch nicht erfolgt ist
        /// </summary>
        public override async Task LoadDataAsync()
        {
            if (!IsLoaded)
            {
                IsLoading = true;

                // Daten initalisieren
                InitializeStandardValues();

                var items =
                (from artist in await SubsonicConnector.Current.CurrentConnection.GetArtistsAsync()
                    select new ListingItem(artist)).ToList();

                // Interpreten sortieren und gruppieren
                var query = from artist in items
                    orderby artist.MusicObject.Name
                    group artist by artist.MusicObject.Name.Substring(0, 1).ToLower()
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
                        foreach (var item in nonAlphaGroup.Items)
                        {
                            nonAlphaGroupItem.Add(item);
                        }
                    }
                    Items.Add(nonAlphaGroupItem);
                }

                IsLoading = false;
                IsLoaded = true;
            }
        }

        /// <summary>
        /// Setzt Standardwerte beim Laden / Wiederherstellen des ViewModels
        /// </summary>
        protected override void InitializeStandardValues()
        {
            base.InitializeStandardValues();
            ListingPageTitle = "artists";
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
                    var serializer = new XmlSerializer(typeof(ArtistsViewModel));
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
                    var serializer = new XmlSerializer(typeof(ArtistsViewModel));
                    var newVm = serializer.Deserialize(loadStream.AsStreamForRead()) as ArtistsViewModel;

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
                        ListingPageTitle = newVm?.ListingPageTitle;
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