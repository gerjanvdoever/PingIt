namespace PingIt.Maui.UITests.Utilities;

public static class TestConfiguration
{
    public static class Android
    {
        public const string PlatformName = "Android";
        public const string AutomationName = "UiAutomator2";
        public const string DeviceName = "emulator-5554";
        public const string PlatformVersion = "15";

        public static string AppPath => Path.GetFullPath(Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "..", "..", "..", "..",
            "PingIt.Maui", "bin", "Debug", "net8.0-android",
            "com.companyname.pingit.maui-Signed.apk"));

        public const string AppPackage = "com.companyname.pingit.maui";
        public const string AppActivity = "crc64fcf28c0e24b4cc31.MainActivity";
    }

    public static class Appium
    {
        public const string ServerUrl = "http://127.0.0.1:4723";
        public const int ImplicitWaitSeconds = 10;
        public const int ExplicitWaitSeconds = 30;
    }
}
