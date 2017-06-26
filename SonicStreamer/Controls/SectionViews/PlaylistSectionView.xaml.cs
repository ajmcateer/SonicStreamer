using Windows.UI.Xaml.Controls;
using SonicStreamer.Common.System;
using SonicStreamer.ViewModels;

namespace SonicStreamer.Controls.SectionViews
{
    public sealed partial class PlaylistSectionView : UserControl
    {
        public readonly PlaylistViewModel PlaylistVm;

        public PlaylistSectionView()
        {
            this.InitializeComponent();

            if (ResourceLoader.Current.GetResource(ref PlaylistVm, Constants.ViewModelPlaylist) == false)
                PlaylistVm = new PlaylistViewModel();
        }
    }
}
