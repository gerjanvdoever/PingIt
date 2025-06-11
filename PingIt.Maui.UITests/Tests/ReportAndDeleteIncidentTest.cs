using PingIt.Maui.UITests.PageObjects;

namespace PingIt.Maui.UITests.Tests;

[TestFixture]
public class ReportAndDeleteIncidentTest : TestBase
{
    private LoginPage _loginPage = null!;
    private AccountPage _accountPage = null!;
    private ReportIncidentPage _reportPage = null!;
    private MyIncidentDetailPage _incidentPage = null!;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        _loginPage = new LoginPage(Driver);
        _accountPage = new AccountPage(Driver);
        _reportPage = new ReportIncidentPage(Driver);
        _incidentPage = new MyIncidentDetailPage(Driver);
    }

    [Test]
    public void FullIncidentFlow_ShouldCreateAndDeleteIncident()
    {
        // Login
        _loginPage.WaitForPageToLoad();
        _loginPage.EnterEmail("admin@pingit.nl");
        _loginPage.EnterPassword("Welkom123");
        _loginPage.TapLogin();

        _accountPage.WaitForPageToLoad();
        Assert.That(_accountPage.IsDashboardVisible(), Is.True);

        // Report Incident
        _accountPage.TapAddIncident();
        _reportPage.FillTitle("Zwerfafval");
        _reportPage.FillDescription("Er liggen wat vuilniszakken met afval op het pleintje naast de jumbo");
        _reportPage.UseCurrentLocation();
        _reportPage.SubmitReport();

        // Select most recent incident
        var incidentCard = By.XPath("//androidx.viewpager.widget.ViewPager//androidx.recyclerview.widget.RecyclerView/android.view.ViewGroup");
        _accountPage.WaitForElement(incidentCard);
        _accountPage.Tap(incidentCard);

        // Delete Incident
        _incidentPage.TapDelete();

        // Assert back on dashboard
        Assert.That(_accountPage.IsDashboardVisible(), Is.True);
    }
}
