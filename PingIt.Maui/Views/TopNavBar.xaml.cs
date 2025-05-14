using Microsoft.Maui.Controls;

namespace PingIt.Maui.Views
{
    public partial class TopNavBar : ContentView
    {
        public TopNavBar()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(
                nameof(Title),
                typeof(string),
                typeof(TopNavBar),
                default(string));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly BindableProperty ShowBackButtonProperty =
            BindableProperty.Create(
                nameof(ShowBackButton),
                typeof(bool),
                typeof(TopNavBar),
                false);

        public bool ShowBackButton
        {
            get => (bool)GetValue(ShowBackButtonProperty);
            set => SetValue(ShowBackButtonProperty, value);
        }

        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            // Try Shell navigation first
            if (Shell.Current.Navigation.NavigationStack.Count > 1)
            {
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await Shell.Current.Navigation.PopAsync();
            }
        }
    }
}
