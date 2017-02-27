using System.Collections.Generic;
using Windows.UI.Xaml;

namespace SonicStreamer.Common.System
{
    public class ResourceLoader
    {
        private static ResourceLoader _current;
        public static ResourceLoader Current => _current ?? (_current = new ResourceLoader());

        protected Dictionary<string, object> TempResources;

        protected ResourceLoader()
        {
            TempResources = new Dictionary<string, object>();
        }

        public bool GetResource<T>(ref T resource, string key)
        {
            try
            {
                if (Application.Current.Resources.ContainsKey(key))
                {
                    resource = (T)Application.Current.Resources[key];
                    return true;
                }
                return false;
            }
            catch
            {
                // Get or Set a temporary resource especially for unit tests
                if (TempResources.ContainsKey(key))
                {
                    resource = (T)TempResources[key];
                }
                else
                {
                    TempResources.Add(key, resource);
                }
                return false;
            }
        }

        public void SetNewTempResource<T>(T resource, string key)
        {
            if (TempResources.ContainsKey(key)) TempResources.Remove(key);
            TempResources.Add(key, resource);
        }
    }
}