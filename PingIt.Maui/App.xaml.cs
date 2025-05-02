namespace PingIt.Maui
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();

            // Defer initialization until the app is fully loaded
            Application.Current.Dispatcher.Dispatch(async () =>
            {
                await InitAsync();
            });
        }

        //protected override Window CreateWindow(IActivationState? activationState)
        //{
        //    return new Window(new AppShell());
        //}

        private async Task InitAsync()
        {

            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}