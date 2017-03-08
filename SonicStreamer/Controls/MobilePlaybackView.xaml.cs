using Windows.UI.Xaml.Controls;
using SonicStreamer.Common.System;
using SonicStreamer.ViewModels;

namespace SonicStreamer.Controls
{
    public sealed partial class MobilePlaybackView : UserControl
    {
        public PlaybackViewModel PlaybackVm;

        public MobilePlaybackView()
        {
            InitializeComponent();

            if (ResourceLoader.Current.GetResource(ref PlaybackVm, Constants.ViewModelPlayback) == false)
                PlaybackVm = new PlaybackViewModel();
        }
    }
}