using OpenQA.Selenium;

namespace PingIt.Maui.UITests.PageObjects;

public class LoginPage : BasePage
{
    public LoginPage(AndroidDriver driver) : base(driver) { }

    private By EmailField => By.XPath("//android.widget.EditText[@text=\"Email\"]");
    private By PasswordField => By.XPath("//android.widget.EditText[@text=\"Password\"]");
    private By LoginButton => By.XPath("//android.widget.Button[@text=\"Login\"]");

    public void WaitForPageToLoad()
    {
        WaitForElement(EmailField);
    }

    public void EnterEmail(string email)
    {
        EnterText(EmailField, email);
    }

    public void EnterPassword(string password)
    {
        EnterText(PasswordField, password);
    }

    public void TapLogin()
    {
        Tap(LoginButton);
    }

    public bool IsLoginButtonVisible()
    {
        return IsElementDisplayed(LoginButton);
    }
}
