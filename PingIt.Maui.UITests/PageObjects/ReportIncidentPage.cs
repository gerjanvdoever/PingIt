namespace PingIt.Maui.UITests.PageObjects;

public class ReportIncidentPage : BasePage
{
    public ReportIncidentPage(AndroidDriver driver) : base(driver) { }

    private By TitleField => By.XPath("//android.widget.EditText[@text='Brief title (e.g., Litter, Vandalism)']");
    private By DescriptionField => By.XPath("//android.widget.EditText[@text='What exactly did you observe?']");
    private By UseLocationButton => By.XPath("//android.widget.Button[@text='Use My Current Location']");
    private By SubmitReportButton => By.XPath("//android.widget.Button[@text='Submit Report']");
    private By CancelDialogButton => By.XPath("//android.widget.Button[@resource-id='android:id/button2']");

    public void FillTitle(string title)
    {
        EnterText(TitleField, title);
    }

    public void FillDescription(string description)
    {
        EnterText(DescriptionField, description);
    }

    public void UseCurrentLocation()
    {
        ScrollToElement(UseLocationButton);
        Tap(UseLocationButton);
    }

    public void SubmitReport()
    {
        Tap(SubmitReportButton);
        Tap(CancelDialogButton); // optional confirmation dismissal
    }
}
