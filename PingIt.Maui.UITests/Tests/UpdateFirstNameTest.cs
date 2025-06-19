using NUnit.Framework;
using PingIt.Maui.UITests.PageObjects;

namespace PingIt.Maui.UITests.Tests;

// Testing logging in, changing first name, checking, changing it back, checking again.
[TestFixture]
public class UpdateFirstNameTest : TestBase
{
    private LoginPage _loginPage = null!;
    private AccountPage _accountPage = null!;
    private AccountDetailPage _accountDetailPage = null!;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();

        _loginPage = new LoginPage(Driver);
        _accountPage = new AccountPage(Driver);
        _accountDetailPage = new AccountDetailPage(Driver);

        _loginPage.WaitForPageToLoad();
        _loginPage.EnterEmail("admin@pingit.nl");
        _loginPage.EnterPassword("Welkom123");
        _loginPage.TapLogin();
    }

    [Test]
    public void UpdateFirstName_And_RevertBack_ShouldReflectInGreeting()
    {
        _accountPage.GoToAccountDetails();
        _accountDetailPage.EditFirstName("Pieter");
        _accountDetailPage.ScrollToAndTapSave();
        _accountDetailPage.TapBack();
        _accountPage.CloseAccountDetailsMenu();

        Assert.That(_accountPage.GreetingContains("Pieter"), Is.True);

        _accountPage.GoToAccountDetails();
        _accountDetailPage.EditFirstName("Piet");
        _accountDetailPage.ScrollToAndTapSave();
        _accountDetailPage.TapBack();

        Assert.That(_accountPage.GreetingContains("Piet"), Is.True);
    }
}
