using PingIt.Maui.ViewModels;

namespace PingIt.Maui.Views;

public partial class MyIncidentDetail : ContentPage
{
	public MyIncidentDetail(MyIncidentDetailViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}