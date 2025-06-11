using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Android;

namespace PingIt.Maui.UITests.PageObjects;

public class RegisterPage : BasePage
{
    public RegisterPage(AndroidDriver driver) : base(driver) { }

    private By FirstNameField => By.XPath("//android.widget.EditText[@text='First Name']");
    private By LastNameField => By.XPath("//android.widget.EditText[@text='Last Name']");
    private By EmailField => By.XPath("//android.widget.EditText[@text='E-mail']");
    private By PasswordField1 => By.XPath("(//android.widget.EditText[@text='Password'])[1]");
    private By PasswordField2 => By.XPath("//android.widget.EditText[@text=\"Confirm Password\"]");
    private By StreetField => By.XPath("//android.widget.EditText[@text='Street']");
    private By HouseNumberField => By.XPath("//android.widget.EditText[@text='House Number']");
    private By PostalCodeField => By.XPath("//android.widget.EditText[@text='Postal Code (e.g. 1234AB)']");
    private By CityField => By.XPath("//android.widget.EditText[@text='City']");
    private By RegisterButton => By.XPath("//android.widget.Button[@text='Register']");

    public void FillForm(string email, string password)
    {
        EnterText(FirstNameField, "Test");
        EnterText(LastNameField, "User");
        EnterText(EmailField, email);
        EnterText(PasswordField1, password);
        EnterText(PasswordField2, password);
        EnterText(StreetField, "Main Street");
        EnterText(HouseNumberField, "42");
        EnterText(PostalCodeField, "1234AB");
        EnterText(CityField, "Amsterdam");
    }

    public void Submit()
    {
        ScrollToElement(RegisterButton);
        Tap(RegisterButton);
    }
}
