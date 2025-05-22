using System;
using Microsoft.Maui.Controls;
using PingIt.Maui.ViewModels;

namespace PingIt.Maui.Views
{
    public partial class AccountPage : ContentPage
    {
        public AccountPage(AccountViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is AccountViewModel vm)
                await vm.InitializeAsync();
        }

        // display logout action sheet without passing null buttons
        async void OnProfileClicked(object sender, EventArgs e)
        {
            if (BindingContext is AccountViewModel vm)
            {
                var action = await DisplayActionSheet(
                    title: null,
                    cancel: "Cancel",
                    destruction: "Logout");

                if (action == "Logout")
                    vm.LogoutCommand.Execute(null);
            }
        }
    }
}
