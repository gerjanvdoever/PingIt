using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Input;
using System.ComponentModel;
using PingIt.Shared.Enums;

namespace PingIt.Maui.ViewModels
{
    public class AccountViewModel : BaseViewModel
    {
        private string _firstName = "Demo";
        private string _lastName = "Gebruiker";
        private UserRole _role = UserRole.Resident;

        public string FirstName
        {
            get => _firstName;
            set
            {
                if (_firstName != value)
                {
                    _firstName = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FullName));
                }
            }
        }

        public string LastName
        {
            get => _lastName;
            set
            {
                if (_lastName != value)
                {
                    _lastName = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FullName));
                }
            }
        }

        public UserRole Role
        {
            get => _role;
            set
            {
                if (_role != value)
                {
                    _role = value;
                    OnPropertyChanged();
                }
            }
        }

        public string FullName => $"{FirstName} {LastName}";

        public ICommand LogoutCommand { get; }

        public AccountViewModel()
        {
            LogoutCommand = new Command(OnLogout);
        }

        private async void OnLogout()
        {
            // TODO: clear token
            await Shell.Current.GoToAsync("//login");
        }
    }
}

