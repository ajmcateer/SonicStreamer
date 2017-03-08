using System.Threading.Tasks;
using Windows.UI.Core;

namespace SonicStreamer.Common.Extension
{
    public interface IDispatcherWrapper
    {
        Task RunAsync(DispatchedHandler callback, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal);
    }
}
