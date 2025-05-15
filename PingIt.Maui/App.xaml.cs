using PingIt.Maui.Services;

namespace PingIt.Maui
{
    public partial class App : Application
    {
        private readonly TokenStorageService _tokenStorage;
        public App(TokenStorageService tokenStorage)
        {
            InitializeComponent();
            _tokenStorage = tokenStorage;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new AppShell());

            Application.Current!.Dispatcher.Dispatch(async () =>
            {
                await InitAsync();
            });

            return window;
        }

        private async Task InitAsync()
        {
            await _tokenStorage.LoadTokenAsync();

            if (_tokenStorage.IsAuthenticated)
            {
                await Shell.Current.GoToAsync("//AccountPage");
            }
            else
            {
                await Shell.Current.GoToAsync("//LoginPage");
            }
        }
    }

}