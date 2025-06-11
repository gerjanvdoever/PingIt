namespace PingIt.Maui.UITests.PageObjects;

public class AccountPage : BasePage
{
    public AccountPage(AndroidDriver driver) : base(driver) { }

    private By AddIncidentButton => By.XPath("//android.widget.TextView[@text='+']");
    private By AccountDetailsAvatar => By.XPath("//androidx.viewpager.widget.ViewPager/androidx.recyclerview.widget.RecyclerView/android.widget.FrameLayout/android.view.ViewGroup/android.view.ViewGroup/android.view.ViewGroup/android.view.ViewGroup[1]/android.view.ViewGroup/android.view.ViewGroup[3]/android.widget.ImageView");
    private By OpenedDetailsAvatar => By.XPath("//androidx.viewpager.widget.ViewPager/androidx.recyclerview.widget.RecyclerView/android.widget.FrameLayout/android.view.ViewGroup/android.view.ViewGroup/android.view.ViewGroup/android.view.ViewGroup[2]/android.view.ViewGroup/android.view.ViewGroup[3]/android.widget.ImageView");
    private By EditDetailsButton => By.XPath("//androidx.viewpager.widget.ViewPager/androidx.recyclerview.widget.RecyclerView/android.widget.FrameLayout/android.view.ViewGroup/android.view.ViewGroup/android.view.ViewGroup/android.view.ViewGroup[4]/android.view.ViewGroup/android.view.ViewGroup/android.view.ViewGroup[1]");
    private By GreetingText => By.XPath("//android.widget.TextView[contains(@text, 'Good')]");

    public void TapAddIncident()
    {
        Tap(AddIncidentButton);
    }

    public void WaitForPageToLoad()
    {
        WaitForElement(AddIncidentButton);
    }

    public bool IsDashboardVisible()
    {
        return IsElementDisplayed(By.XPath("//android.widget.TextView[@text='Account Dashboard']"));
    }
    public void GoToAccountDetails()
    {
        Tap(AccountDetailsAvatar);
        Thread.Sleep(1000);
        Tap(EditDetailsButton);
    }

    public void CloseAccountDetailsMenu()
    {
        Tap(OpenedDetailsAvatar);
    }

    public bool GreetingContains(string name)
    {
        var greeting = GetText(GreetingText);
        return greeting.Contains(name);
    }
}
