using SonicStreamer.Common.System;
using SonicStreamer.SampleData;
using SonicStreamer.Subsonic.Data;
using SonicStreamer.Subsonic.Server;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SonicStreamer.ViewModels
{
    /// <summary>
    /// Startoberfläche mit einer zufälligen Auswahl von Alben
    /// </summary>
    public class StartViewModel : BaseViewModel
    {
        #region Properties

        public ObservableCollection<Album> RandomAlbums { get; set; }
        public ObservableCollection<Album> RecentAlbums { get; set; }
        public ObservableCollection<Album> FrequentAlbums { get; set; }
        public ObservableCollection<Album> NewestAlbums { get; set; }

        #endregion

        public StartViewModel()
        {
            RandomAlbums = new ObservableCollection<Album>();
            RecentAlbums = new ObservableCollection<Album>();
            FrequentAlbums = new ObservableCollection<Album>();
            NewestAlbums = new ObservableCollection<Album>();

            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                foreach (var album in SampleDataCreator.Current.CreateAlbums())
                {
                    RandomAlbums.Add(album);
                    RecentAlbums.Add(album);
                    FrequentAlbums.Add(album);
                    NewestAlbums.Add(album);
                }
            }
        }

        /// <summary>
        /// Lädt Alben für die einzelnen Sections der StartPage
        /// </summary>
        public async Task LoadDataAsync()
        {
            RandomAlbums.Clear();
            RecentAlbums.Clear();
            FrequentAlbums.Clear();
            NewestAlbums.Clear();

            var albumDetermination = new List<Task<List<Album>>>();
            var albumLists = new List<List<Album>>();

            albumDetermination.Add(
                SubsonicConnector.Current.CurrentConnection.GetAlbumSectionAsync(
                    BaseSubsonicConnection.AlbumListType.Random, 12));
            albumDetermination.Add(
                SubsonicConnector.Current.CurrentConnection.GetAlbumSectionAsync(
                    BaseSubsonicConnection.AlbumListType.Recent, 12));
            albumDetermination.Add(
                SubsonicConnector.Current.CurrentConnection.GetAlbumSectionAsync(
                    BaseSubsonicConnection.AlbumListType.Frequent, 12));
            albumDetermination.Add(
                SubsonicConnector.Current.CurrentConnection.GetAlbumSectionAsync(
                    BaseSubsonicConnection.AlbumListType.Newest, 12));

            albumLists.AddRange(await Task.WhenAll(albumDetermination));

            foreach (var album in albumLists[0])
            {
                RandomAlbums.Add(album);
            }

            foreach (var album in albumLists[1])
            {
                RecentAlbums.Add(album);
            }

            foreach (var album in albumLists[2])
            {
                FrequentAlbums.Add(album);
            }

            foreach (var album in albumLists[3])
            {
                NewestAlbums.Add(album);
            }
        }
    }
}