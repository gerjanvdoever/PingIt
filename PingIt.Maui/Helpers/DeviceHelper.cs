using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PingIt.Maui.Helpers
{
    // Maui maps doesn't work on Windows, hence we use this helper to prevent the map component
    // from initialising whenever wer're on WinUI.
    public static class DeviceHelper
    {
        public static bool IsWindows => DeviceInfo.Platform == DevicePlatform.WinUI;
        public static bool IsMobile => DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS;
    }
}
