using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace SonicStreamer.Common.Extension
{
    public class DispatcherWrapper : IDispatcherWrapper
    {
        private readonly CoreDispatcher _dispatcher;

        public DispatcherWrapper(CoreDispatcher dispatcher)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        public Task RunAsync(DispatchedHandler callback, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
        {
            return _dispatcher.RunAsync(priority, callback).AsTask();
        }
    }
}
