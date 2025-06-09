using NUnit.Framework;
using OpenQA.Selenium.Appium.Android;
using PingIt.Maui.UITests.Drivers;
using OpenQA.Selenium;
using NUnit.Framework.Interfaces;

namespace PingIt.Maui.UITests.Tests;

[TestFixture]
public abstract class TestBase
{
    protected AppiumDriverManager DriverManager = null!;
    protected AndroidDriver Driver => DriverManager.Driver;

    [OneTimeSetUp]
    public async Task GlobalSetup()
    {
        DriverManager = new AppiumDriverManager();
        await DriverManager.InitializeAsync();
    }

    [OneTimeTearDown]
    public void GlobalTeardown()
    {
        DriverManager.Quit();
    }

    [SetUp]
    public virtual void SetUp()
    {
        Driver.ActivateApp(Driver.CurrentPackage);
    }

    [TearDown]
    public virtual void TearDown()
    {
        if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
        {
            TakeScreenshot();
        }
    }

    private void TakeScreenshot()
    {
        try
        {
            var screenshot = Driver.GetScreenshot();
            var fileName = $"screenshot_{TestContext.CurrentContext.Test.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, fileName);
            screenshot.SaveAsFile(path);
            TestContext.AddTestAttachment(path);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Screenshot failed: {ex.Message}");
        }
    }
}
