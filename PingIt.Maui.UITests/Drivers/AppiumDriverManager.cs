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

        // Use properties for standard W3C capabilities
        options.PlatformName = TestConfiguration.Android.PlatformName;
        options.PlatformVersion = TestConfiguration.Android.PlatformVersion;
        options.AutomationName = TestConfiguration.Android.AutomationName;
        options.DeviceName = TestConfiguration.Android.DeviceName;

        // Use additional options for Appium-specific capabilities
        options.AddAdditionalAppiumOption("appPackage", TestConfiguration.Android.AppPackage);
        options.AddAdditionalAppiumOption("appActivity", TestConfiguration.Android.AppActivity);
        options.AddAdditionalAppiumOption("noReset", true);
        options.AddAdditionalAppiumOption("newCommandTimeout", 300);
        options.AddAdditionalAppiumOption("autoGrantPermissions", true);
        options.AddAdditionalAppiumOption("ignoreHiddenApiPolicyError", true);

        _driver = new AndroidDriver(new Uri(TestConfiguration.Appium.ServerUrl), options);
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(TestConfiguration.Appium.ImplicitWaitSeconds);
    }

    private async Task StartAppiumServerAsync()
    {
        var psi = new ProcessStartInfo
        {
            FileName = "appium",
            Arguments = "--port 4723 --log-level error",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        _appiumProcess = Process.Start(psi);
    }

    private async Task WaitForAppiumToBeReady()
    {
        using var client = new HttpClient();
        for (int i = 0; i < 10; i++)
        {
            try
            {
                var res = await client.GetAsync($"{TestConfiguration.Appium.ServerUrl}/status");
                if (res.IsSuccessStatusCode) return;
            }
            catch { }

            await Task.Delay(500);
        }

        throw new Exception("Appium server did not become ready in time.");
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
