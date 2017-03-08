using Windows.System.Profile;
using Windows.UI.Xaml;

namespace SonicStreamer.Common.System
{
    public class DeviceFamilyTrigger : StateTriggerBase
    {
        private string _currentDeviceFamily, _queriedDeviceFamily;

        public string DeviceFamily
        {
            get { return _queriedDeviceFamily; }
            set
            {
                _queriedDeviceFamily = value;
                _currentDeviceFamily = AnalyticsInfo.VersionInfo.DeviceFamily;
                SetActive(_queriedDeviceFamily == _currentDeviceFamily);
            }
        }
    }
}