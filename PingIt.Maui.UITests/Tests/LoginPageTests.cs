using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NUnit.Framework;
using PingIt.Maui.UITests.PageObjects;
namespace PingIt.Maui.UITests.Tests;

[TestFixture]
public class LoginPageTests : TestBase
{
    private LoginPage _loginPage = null!;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        _loginPage = new LoginPage(Driver);
        _loginPage.WaitForPageToLoad();
    }

    [Test]
    public void Login_WithValidCredentials_ShouldNavigateToAccountPage()
    {
        // Arrange  
        const string email = "admin@pingit.nl";
        const string password = "Welkom123";

        // Act  
        _loginPage.EnterEmail(email);
        _loginPage.EnterPassword(password);
        _loginPage.TapLogin();

        // Assert  
        // Replace FindElementByXPath with FindElement using By.XPath  <-- NOG DOEN
        var accountHeader = Driver.FindElement(By.XPath("//android.widget.TextView[@text=\"Account Dashboard\"]"));
        Assert.That(accountHeader.Displayed, Is.True);
    }
}
