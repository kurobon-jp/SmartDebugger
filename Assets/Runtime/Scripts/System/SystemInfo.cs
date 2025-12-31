using System.Collections.Generic;
using UnityEngine;

namespace SmartDebugger
{
    public static class SystemInfo
    {
        public static Dictionary<string, string> GetSystemInfoText()
        {
            var values = new Dictionary<string, string>();
            GetCommonInfo(values);
            GetDisplayInfo(values);
            GetBatteryInfo(values);
            GetPlatformSpecificInfo(values);
            GetAppInfo(values);
            return values;
        }

        // ================================
        // Common Info
        // ================================
        private static void GetCommonInfo(Dictionary<string, string> values)
        {
            values.Add("Device Info",
                $"Device Model: {UnityEngine.SystemInfo.deviceModel}\n" +
                $"Device Name: {UnityEngine.SystemInfo.deviceName}\n" +
                $"Device Type: {UnityEngine.SystemInfo.deviceType}"
            );

            values.Add("Operating System",
                $"OS: {UnityEngine.SystemInfo.operatingSystem}\n" +
                $"OS Family: {UnityEngine.SystemInfo.operatingSystemFamily}\n" +
                $"Platform: {Application.platform}"
            );

            values.Add("CPU Info",
                $"CPU Type: {UnityEngine.SystemInfo.processorType}\n" +
                $"CPU Count: {UnityEngine.SystemInfo.processorCount}\n" +
                $"CPU Frequency: {UnityEngine.SystemInfo.processorFrequency} MHz"
            );

            values.Add("Memory",
                $"System Memory: {UnityEngine.SystemInfo.systemMemorySize} MB\n" +
                $"Graphics Memory: {UnityEngine.SystemInfo.graphicsMemorySize} MB"
            );

            values.Add("GPU Info",
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
        private static void GetDisplayInfo(Dictionary<string, string> values)
        {
            var res = Screen.currentResolution;
            values.Add("Display Info",
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
        private static void GetBatteryInfo(Dictionary<string, string> values)
        {
            values.Add("Battery Info",
                $"Battery Level: {UnityEngine.SystemInfo.batteryLevel}\n" +
                $"Battery Status: {UnityEngine.SystemInfo.batteryStatus}"
            );
        }

        // ================================
        // Platform Specific (Android/iOS)
        // ================================
        private static void GetPlatformSpecificInfo(Dictionary<string, string> values)
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    GetAndroidInfo(values);
                    break;
                case RuntimePlatform.IPhonePlayer:
                    GetIOSInfo(values);
                    break;
            }
        }

        // ------------------------------
        // Android情報取得
        // ------------------------------
        private static void GetAndroidInfo(Dictionary<string, string> values)
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

            values.Add("Android Info",
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
        private static void GetIOSInfo(Dictionary<string, string> values)
        {
#if UNITY_IOS
        var deviceModel = UnityEngine.iOS.Device.generation.ToString();
        var systemVersion = UnityEngine.iOS.Device.systemVersion;
        var batteryLevel = UnityEngine.iOS.Device.batteryLevel;
        var batteryStatus = UnityEngine.iOS.Device.batteryStatus;
        var memory = UnityEngine.iOS.Device.systemMemorySize;
        values.Add("iOS Info",
                $"Device Model: {deviceModel}\n" +
                $"iOS Version: {systemVersion}\n" +
                $"Battery Level: {batteryLevel}\n" +
                $"Battery Status: {batteryStatus}\n" +
                $"System Memory: {memory} MB"
        );
#endif
        }

        // ================================
        // Application Info
        // ================================
        private static void GetAppInfo(Dictionary<string, string> values)
        {
            values.Add("App Version",
                $"App Version: {Application.version}\n" +
                $"App Identifier: {Application.identifier}\n" +
                $"Unity Version: {Application.unityVersion}"
            );
        }
    }
}