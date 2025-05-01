using System.Windows.Input;
using PingIt.Maui.ViewModels;

namespace PingIt.Maui.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string email = string.Empty;
        public string Email
        {
            get => email;
            set => SetProperty(ref email, value);
        }

        private string password = string.Empty;
        public string Password
        {
            get => password;
            set => SetProperty(ref password, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand AnonymousCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new Command(async () => await OnLoginAsync(), () => !IsBusy);
            AnonymousCommand = new Command(OnAnonymous);
        }

        private async Task OnLoginAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            try
            {
                // TODO: Replace with actual login logic  
                await Task.Delay(1000); // Simulate loading  
                Console.WriteLine($"Logging in with {Email}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login failed: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void OnAnonymous()
        {
            // TODO: Navigate to anonymous report page  
            Console.WriteLine("Navigating to anonymous report screen...");
        }
    }
}
