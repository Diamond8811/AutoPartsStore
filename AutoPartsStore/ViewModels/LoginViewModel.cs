using System.Windows;
using AutoPartsStore.Infrastructure;
using AutoPartsStore.Services;

namespace AutoPartsStore.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly AuthService _authService = new AuthService();

        private string _login;
        public string Login
        {
            get => _login;
            set { _login = value; OnPropertyChanged(); }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasError)); }
        }
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public RelayCommand LoginCommand => new RelayCommand(LoginMethod);

        private void LoginMethod(object obj)
        {
            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Введите логин и пароль";
                return;
            }

            var user = _authService.Authenticate(Login, Password);

            if (user != null)
            {
                var main = new MainWindow(user);
                main.Show();
                Application.Current.Windows[0]?.Close();
            }
            else
            {
                ErrorMessage = "Неверный логин или пароль";
            }
        }
    }
}