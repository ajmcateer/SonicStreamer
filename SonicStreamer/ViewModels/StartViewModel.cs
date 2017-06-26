using System.Collections.Generic;
using SonicStreamer.SampleData;
using SonicStreamer.Subsonic.Data;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using SonicStreamer.Pages;
using SonicStreamer.ViewModelItems.Sections;

namespace SonicStreamer.ViewModels
{
    /// <summary>
    /// ViewModel for <see cref="HomePage"/>
    /// </summary>
    public class StartViewModel : BaseViewModel
    {
        #region Properties

        public ObservableCollection<BaseSection> Sections { get; set; }

        #endregion

        public StartViewModel()
        {
            Sections = new ObservableCollection<BaseSection>();

            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                //foreach (var albumSection in SampleDataCreator.Current.CreateAlbumSections())
                //{
                //    Sections.Add(albumSection);
                //}
            }
        }

        /// <summary>
        /// Creates all Sections and loads the data
        /// </summary>
        public async Task LoadDataAsync()
        {
            Sections.Clear();

            // Pre-Initialize Sections
            //Sections.Add(new AlbumListsSection("album lists", 0));
            //Sections.Add(new ArtistSection("New Found Glory", "220", "New Found Glory", 1));
            Sections.Add(new ArtistSection("Rise Against", "214", "Rise Against", 2));
            //Sections.Add(new PinnedAlbumsSection("pinned albums", new List<string>{"1028", "1029", "1028", "1029", "1028", "1029", "1028", "1029"}, 3));

            // Load Data in parallel for better performance
            var loadTasks = Sections.Select(section => section.LoadDataAsync()).ToList();
            await Task.WhenAll(loadTasks);
        }
    }
}