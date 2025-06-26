namespace PingIt.Maui.UITests.Utilities;

public static class TestConfiguration
{
    public static class Android
    {
        public const string PlatformName = "Android";
        public const string AutomationName = "UiAutomator2";
        public const string DeviceName = "emulator-5556";
        public const string PlatformVersion = "15";

        public static string AppPath => Path.GetFullPath(Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "..", "..", "..", "..",
            "PingIt.Maui", "bin", "Release", "net9.0-android",
            "com.companyname.pingit.maui-Signed.apk")); // points to release build, adb install -r PingIt.Maui\bin\Release\net9.0-android\com.companyname.pingit.maui-Signed.apk

        public const string AppPackage = "com.companyname.pingit.maui";
    }

    public static class Appium
    {
        public const string ServerUrl = "http://127.0.0.1:4723";
        public const int ImplicitWaitSeconds = 10;
        public const int ExplicitWaitSeconds = 30;
    }
}
