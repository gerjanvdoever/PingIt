using NUnit.Framework;
using PingIt.Maui.UITests.PageObjects;

namespace PingIt.Maui.UITests.Tests;

[TestFixture]
public class RegisterTest : TestBase
{
    // Register and verify new user registration
    [Test]
    public void Register_NewUser_ShouldShowAccountDashboard()
    {
        var loginPage = new LoginPage(Driver);
        loginPage.WaitForPageToLoad();
        loginPage.TapRegister();

        var registerPage = new RegisterPage(Driver);

        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var email = $"user{timestamp}@pingit.test";
        var password = "Welkom123";

        registerPage.FillForm(email, password);
        registerPage.Submit();

        var greetingElement = Driver.FindElement(By.XPath("//android.widget.TextView[starts-with(@text, 'Good')]"));
        Assert.That(greetingElement.Text, Does.Contain("Test"), "Greeting should include user's first name.");
    }
}
