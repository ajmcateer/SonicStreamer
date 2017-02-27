using SonicStreamer.Pages;
using System;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.UI.Xaml.Controls;

namespace SonicStreamer.ViewModels
{
    public class MainViewModel : BaseViewModel, IViewModelSerializable
    {
        #region Properties

        private Frame _mainFrame;

        [XmlIgnore]
        public Frame MainFrame
        {
            get { return _mainFrame; }
            set { Set(ref _mainFrame, value); }
        }

        private string _pageTitle;

        [XmlIgnore]
        public string PageTitle
        {
            get { return _pageTitle; }
            set { Set(ref _pageTitle, value); }
        }

        private bool _isPaneOpen;

        [XmlIgnore]
        public bool IsPaneOpen
        {
            get { return _isPaneOpen; }
            set { Set(ref _isPaneOpen, value); }
        }

        #endregion

        public MainViewModel()
        {
            MainFrame = new Frame();
        }

        public void LoadData()
        {
            PageTitle = "home";
            MainFrame.Navigate(typeof(HomePage));
            IsPaneOpen = false;
        }

        #region Hamburger Menu Methods

        public void HamburgerButtonClick()
        {
            IsPaneOpen = !IsPaneOpen;
        }

        public void HomeButtonClick()
        {
            MainFrame.Navigate(typeof(HomePage));
            PageTitle = "home";
            IsPaneOpen = false;
        }

        public void SearchButtonClick()
        {
            MainFrame.Navigate(typeof(SearchPage));
            PageTitle = "search";
            IsPaneOpen = false;
        }

        public void ArtistsButtonClick()
        {
            MainFrame.Navigate(typeof(ArtistsPage));
            PageTitle = "artists";
            IsPaneOpen = false;
        }

        public void AlbumsButtonClick()
        {
            MainFrame.Navigate(typeof(AlbumsPage));
            PageTitle = "albums";
            IsPaneOpen = false;
        }

        public void FolderButtonClick()
        {
            MainFrame.Navigate(typeof(FolderPage));
            PageTitle = "folders";
            IsPaneOpen = false;
        }

        public void PlaylistButtonClick()
        {
            MainFrame.Navigate(typeof(PlaylistPage));
            PageTitle = "playlists";
            IsPaneOpen = false;
        }

        public void PodcastButtonClick()
        {
            MainFrame.Navigate(typeof(PodcastPage));
            PageTitle = "podcasts";
            IsPaneOpen = false;
        }

        public void SettingsButtonClick()
        {
            MainFrame.Navigate(typeof(SettingsPage));
            PageTitle = "settings";
            IsPaneOpen = false;
        }

        #endregion

        public void PlaybackButtonClick()
        {
            MainFrame.Navigate(typeof(PlaybackPage));
        }

        public async Task SaveViewModelAsync(string savename)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    // es gibt bisher nichts zu Serialisieren, aber beim Wiederherstellen
                    // sollte das ViewModel einmal standardmäßig laden
                });
        }

        public async Task RestoreViewModelAsync(string loadname)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal, LoadData);
        }
    }
}