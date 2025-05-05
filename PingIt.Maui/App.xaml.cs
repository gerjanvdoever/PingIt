namespace PingIt.Maui
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new AppShell());

            // Optionally, defer InitAsync here if needed
            Application.Current!.Dispatcher.Dispatch(async () =>
            {
                await InitAsync();
            });

            return window;
        }

        private async Task InitAsync()
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }

}