using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Enums;
using System.Diagnostics;

namespace PingIt.Maui.UITests.Drivers;

public class AppiumDriverManager
{
    private AndroidDriver? _driver;
    private Process? _appiumProcess;

    public AndroidDriver Driver => _driver ?? throw new InvalidOperationException("Driver not initialized");

    public async Task InitializeAsync()
    {

        var options = new AppiumOptions();

        options.PlatformName = TestConfiguration.Android.PlatformName;
        options.PlatformVersion = TestConfiguration.Android.PlatformVersion;
        options.AutomationName = TestConfiguration.Android.AutomationName;
        options.DeviceName = TestConfiguration.Android.DeviceName;
        options.App = TestConfiguration.Android.AppPath;
        options.AddAdditionalAppiumOption("appPackage", TestConfiguration.Android.AppPackage);
        options.AddAdditionalAppiumOption("appWaitActivity", "*");
        options.AddAdditionalAppiumOption("noReset", false);
        options.AddAdditionalAppiumOption("fullReset", true);
        options.AddAdditionalAppiumOption("autoGrantPermissions", true);
        options.AddAdditionalAppiumOption("newCommandTimeout", 300);
        options.AddAdditionalAppiumOption("autoGrantPermissions", true);
        options.AddAdditionalAppiumOption("appWaitActivity", "*");
        options.AddAdditionalAppiumOption("ignoreHiddenApiPolicyError", true);

        _driver = new AndroidDriver(new Uri(TestConfiguration.Appium.ServerUrl), options);
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(TestConfiguration.Appium.ImplicitWaitSeconds);
    }

    public void Quit()
    {
        try
        {
            _driver?.Quit();
            _appiumProcess?.Kill(true);
            _appiumProcess?.WaitForExit(3000);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during cleanup: {ex.Message}");
        }
        finally
        {
            _driver?.Dispose();
            _appiumProcess?.Dispose();
        }
    }
}
