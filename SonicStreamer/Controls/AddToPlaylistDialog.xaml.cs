using Windows.UI.Xaml.Controls;
using SonicStreamer.Common.System;
using SonicStreamer.ViewModels;
using System.Threading.Tasks;

namespace SonicStreamer.Controls
{
    public sealed partial class AddToPlaylistDialog : UserControl
    {
        public readonly PlaylistViewModel PlaylistVm;

        public AddToPlaylistDialog()
        {
            this.InitializeComponent();

            if (ResourceLoader.Current.GetResource(ref PlaylistVm, Constants.ViewModelPlaylist) == false)
                PlaylistVm = new PlaylistViewModel();
        }
    }
}
