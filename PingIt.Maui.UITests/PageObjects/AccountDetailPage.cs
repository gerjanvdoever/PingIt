using OpenQA.Selenium;

namespace PingIt.Maui.UITests.PageObjects;

public class AccountDetailPage : BasePage
{
    public AccountDetailPage(AndroidDriver driver) : base(driver) { }

    private By FirstNameField => By.XPath("//android.widget.EditText");

    private By SaveChangesButton => By.XPath("//android.widget.Button[@text='Save Changes']");

    private By BackButton => By.XPath("//android.widget.ImageView");

    public void EditFirstName(string newName)
    {
        var field = FindElement(FirstNameField);
        field.Clear();
        field.SendKeys(newName);
    }

    public void ScrollToAndTapSave()
    {
        ScrollToElement(SaveChangesButton);
        Tap(SaveChangesButton);
    }

    public void TapBack()
    {
        Tap(BackButton);
    }
}
