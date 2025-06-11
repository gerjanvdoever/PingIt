using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace PingIt.Maui.UITests.PageObjects;

public abstract class BasePage
{
    protected readonly AndroidDriver Driver;
    protected readonly WebDriverWait Wait;

    public BasePage(AndroidDriver driver)
    {
        Driver = driver;
        Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(TestConfiguration.Appium.ExplicitWaitSeconds));
    }

    public IWebElement FindElement(By locator)
    {
        return Wait.Until(d => d.FindElement(locator));
    }

    public IReadOnlyCollection<IWebElement> FindElements(By locator)
    {
        return Driver.FindElements(locator);
    }

    public void WaitForElement(By locator)
    {
        Wait.Until(d => d.FindElement(locator).Displayed);
    }

    public void Tap(By locator)
    {
        FindElement(locator).Click();
    }

    protected void EnterText(By locator, string text)
    {
        var el = FindElement(locator);
        el.Clear();
        el.SendKeys(text);
    }

    public string GetText(By locator)
    {
        return FindElement(locator).Text;
    }

    public bool IsElementDisplayed(By locator)
    {
        try
        {
            return Driver.FindElement(locator).Displayed;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    public void ScrollToElement(By locator)
    {
        const int maxScrolls = 5;
        for (int i = 0; i < maxScrolls; i++)
        {
            if (IsElementDisplayed(locator)) return;

            var js = (IJavaScriptExecutor)Driver;
            js.ExecuteScript("mobile: scrollGesture", new Dictionary<string, object>
        {
            { "left", 100 },
            { "top", 100 },
            { "width", 800 },
            { "height", 1000 },
            { "direction", "down" },
            { "percent", 0.8 }
        });

            Thread.Sleep(500); // Allow some time for scroll to complete
        }

        throw new Exception("Element not found after scrolling.");
    }
}
