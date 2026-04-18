using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SmartDebugger
{
    public static class SystemInfo
    {
        private static readonly List<int> Orders = new();
        private static readonly List<string> Titles = new();
        private static readonly List<Func<string>> Getters = new();

        static SystemInfo()
        {
            AddCommonInfo(100);
            AddDisplayInfo(200);
            AddBatteryInfo(300);
            AddPlatformSpecificInfo(400);
            AddAppInfo(500);
        }

        public static IReadOnlyDictionary<string, string> GetSystemInfoText()
        {
            var values = new Dictionary<string, string>();
            foreach (var order in Orders.Select((order, index) => (order, index)).OrderBy(x => x.order))
            {
                var index = order.index;
                var title = Titles[index];
                var value = Getters[index].Invoke();
                values.Add(title, value);
            }

            return values;
        }

        public static void AddSystemInfoText(int order, string title, Func<string> getter)
        {
            if (Titles.Contains(title)) return;
            Orders.Add(order);
            Titles.Add(title);
            Getters.Add(getter);
        }

        public static void RemoveSystemInfoText(string title)
        {
            var index = Titles.IndexOf(title);
            if (index < 0) return;
            Orders.RemoveAt(index);
            Titles.RemoveAt(index);
            Getters.RemoveAt(index);
        }

        // ================================
        // Common Info
        // ================================
        private static void AddCommonInfo(int sortOrder)
        {
            AddSystemInfoText(sortOrder++, "Device Info", () =>
                $"Device Model: {UnityEngine.SystemInfo.deviceModel}\n" +
                $"Device Name: {UnityEngine.SystemInfo.deviceName}\n" +
                $"Device Type: {UnityEngine.SystemInfo.deviceType}"
            );

            AddSystemInfoText(sortOrder++, "Operating System", () =>
                $"OS: {UnityEngine.SystemInfo.operatingSystem}\n" +
                $"OS Family: {UnityEngine.SystemInfo.operatingSystemFamily}\n" +
                $"Platform: {Application.platform}"
            );

            AddSystemInfoText(sortOrder++, "CPU Info", () =>
                $"CPU Type: {UnityEngine.SystemInfo.processorType}\n" +
                $"CPU Count: {UnityEngine.SystemInfo.processorCount}\n" +
                $"CPU Frequency: {UnityEngine.SystemInfo.processorFrequency} MHz"
            );

            AddSystemInfoText(sortOrder++, "Memory", () =>
                $"System Memory: {UnityEngine.SystemInfo.systemMemorySize} MB\n" +
                $"Graphics Memory: {UnityEngine.SystemInfo.graphicsMemorySize} MB"
            );

            AddSystemInfoText(sortOrder, "GPU Info", () =>
                $"Graphics Device: {UnityEngine.SystemInfo.graphicsDeviceName}\n" +
                $"Graphics Vendor: {UnityEngine.SystemInfo.graphicsDeviceVendor}\n" +
                $"Graphics API: {UnityEngine.SystemInfo.graphicsDeviceType}\n" +
                $"Max Texture Size: {UnityEngine.SystemInfo.maxTextureSize}\n" +
                $"Supports Instancing: {UnityEngine.SystemInfo.supportsInstancing}\n" +
                $"Supports Compute Shaders: {UnityEngine.SystemInfo.supportsComputeShaders}"
            );
        }

        // ================================
        // Display Info
        // ================================
        private static void AddDisplayInfo(int sortOrder)
        {
            var res = Screen.currentResolution;
            AddSystemInfoText(sortOrder, "Display Info", () =>
                $"Resolution: {res.width} x {res.height}\n" +
                $"Refresh Rate: {res.refreshRateRatio.value} Hz\n" +
                $"DPI: {Screen.dpi}\n" +
                $"Fullscreen Mode: {Screen.fullScreenMode}\n" +
                $"Orientation: {Screen.orientation}"
            );
        }

        // ================================
        // Battery
        // ================================
        private static void AddBatteryInfo(int sortOrder)
        {
            AddSystemInfoText(sortOrder, "Battery Info", () =>
                $"Battery Level: {UnityEngine.SystemInfo.batteryLevel}\n" +
                $"Battery Status: {UnityEngine.SystemInfo.batteryStatus}"
            );
        }

        // ================================
        // Platform Specific (Android/iOS)
        // ================================
        private static void AddPlatformSpecificInfo(int sortOrder)
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    AddAndroidInfo(sortOrder);
                    break;
                case RuntimePlatform.IPhonePlayer:
                    AddIOSInfo(sortOrder);
                    break;
            }
        }

        // ------------------------------
        // Android情報取得
        // ------------------------------
        private static void AddAndroidInfo(int sortOrder)
        {
#if UNITY_ANDROID
            using var build = new AndroidJavaClass("android.os.Build");
            var manufacturer = build.GetStatic<string>("MANUFACTURER");
            var model = build.GetStatic<string>("MODEL");
            var device = build.GetStatic<string>("DEVICE");
            var product = build.GetStatic<string>("PRODUCT");
            using var version = new AndroidJavaClass("android.os.Build$VERSION");
            var androidVersion = version.GetStatic<string>("RELEASE");
            var sdk = version.GetStatic<int>("SDK_INT");

            AndroidJavaObject context = null;
            AndroidJavaObject metrics = null;
            float xdpi = 0, ydpi = 0;
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                }

                metrics = new AndroidJavaObject("android.util.DisplayMetrics");
                var windowManager = context.Call<AndroidJavaObject>("getWindowManager");
                var display = windowManager.Call<AndroidJavaObject>("getDefaultDisplay");
                display.Call("getRealMetrics", metrics);

                xdpi = metrics.Get<float>("xdpi");
                ydpi = metrics.Get<float>("ydpi");
            }
            catch
            {
            }

            AddSystemInfoText(sortOrder, "Android Info", () =>
                $"Manufacturer: {manufacturer}\n" +
                $"Model: {model}\n" +
                $"Device: {device}\n" +
                $"Product: {product}\n" +
                $"Android Version: {androidVersion}\n" +
                $"API Level: {sdk}\n" +
                $"X DPI: {xdpi}\n" +
                $"Y DPI: {ydpi}"
            );
#endif
        }

        // ------------------------------
        // iOS情報取得
        // ------------------------------
        private static void AddIOSInfo(int sortOrder)
        {
#if UNITY_IOS
            var deviceModel = UnityEngine.iOS.Device.generation.ToString();
            var systemVersion = UnityEngine.iOS.Device.systemVersion;
            var vendorIdentifier = UnityEngine.iOS.Device.vendorIdentifier;
            AddSystemInfoText(sortOrder, "iOS Info", () =>
                    $"Device Model: {deviceModel}\n" +
                    $"iOS Version: {systemVersion}\n" +
                    $"Vendor Identifier: {vendorIdentifier}"
            );
#endif
        }

        // ================================
        // Application Info
        // ================================
        private static void AddAppInfo(int sortOrder)
        {
            AddSystemInfoText(sortOrder, "App Version", () =>
                $"App Version: {Application.version}\n" +
                $"App Identifier: {Application.identifier}\n" +
                $"Unity Version: {Application.unityVersion}"
            );
        }
    }
}