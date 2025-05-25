using PingIt.Maui.ViewModels;

namespace PingIt.Maui.Views;

public partial class MyAccountDetailPage : ContentPage
{
    private readonly MyAccountDetailViewModel _viewModel;

    public MyAccountDetailPage(MyAccountDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}
