namespace PingIt.Maui.UITests.PageObjects;

public class MyIncidentDetailPage : BasePage
{
    public MyIncidentDetailPage(AndroidDriver driver) : base(driver) { }

    private By DeleteButton => By.XPath("//androidx.drawerlayout.widget.DrawerLayout/android.widget.FrameLayout/android.widget.LinearLayout/android.widget.FrameLayout/android.widget.ScrollView/android.view.ViewGroup/android.view.ViewGroup/android.view.ViewGroup/android.view.ViewGroup[2]/android.view.ViewGroup/android.view.ViewGroup/android.view.ViewGroup/android.widget.Button");

    public void TapDelete()
    {
        Tap(DeleteButton);
    }
}
