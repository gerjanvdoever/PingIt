using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace PingIt.Maui.UITests.PageObjects;

public abstract class BasePage
{
    protected readonly AndroidDriver Driver;
    protected readonly WebDriverWait Wait;

    protected BasePage(AndroidDriver driver)
    {
        Driver = driver;
        Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(TestConfiguration.Appium.ExplicitWaitSeconds));
    }

    protected IWebElement FindElement(By locator)
    {
        return Wait.Until(d => d.FindElement(locator));
    }

    protected IReadOnlyCollection<IWebElement> FindElements(By locator)
    {
        return Driver.FindElements(locator);
    }

    protected void WaitForElement(By locator)
    {
        Wait.Until(d => d.FindElement(locator).Displayed);
    }

    protected void Tap(By locator)
    {
        FindElement(locator).Click();
    }

    protected void EnterText(By locator, string text)
    {
        var el = FindElement(locator);
        el.Clear();
        el.SendKeys(text);
    }

    protected string GetText(By locator)
    {
        return FindElement(locator).Text;
    }

    protected bool IsElementDisplayed(By locator)
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
}
