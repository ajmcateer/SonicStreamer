using System.Threading.Tasks;
using Windows.UI.Core;

namespace SonicStreamer.Common.Extension
{
    public class FakeDispatcherWrapper : IDispatcherWrapper
    {
        public Task RunAsync(DispatchedHandler callback, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
        {
            callback();
            return Task.Delay(2000);
        }
    }
}
