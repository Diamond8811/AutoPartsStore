using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using AutoPartsStore.Infrastructure;
using AutoPartsStore.Models;

namespace AutoPartsStore.ViewModels
{
    public class AdminViewModel : BaseViewModel
    {
        private readonly Users _currentUser;
        public ObservableCollection<Users> Users { get; set; }
        public ObservableCollection<Roles> Roles { get; set; }

        private Users _selectedUser;
        public Users SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (_selectedUser != value)
                {
                    _selectedUser = value;
                    OnPropertyChanged();
                    CommandManager.InvalidateRequerySuggested();
                    // Принудительно обновляем команды
                    ToggleBlockCommand?.RaiseCanExecuteChanged();
                    ResetPasswordCommand?.RaiseCanExecuteChanged();
                    SaveRoleCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        public RelayCommand RefreshCommand { get; }
        public RelayCommand ToggleBlockCommand { get; }
        public RelayCommand ResetPasswordCommand { get; }
        public RelayCommand SaveRoleCommand { get; }

        public AdminViewModel(Users currentUser)
        {
            _currentUser = currentUser;
            Users = new ObservableCollection<Users>();
            Roles = new ObservableCollection<Roles>();

            RefreshCommand = new RelayCommand(_ => LoadData());
            ToggleBlockCommand = new RelayCommand(_ => ToggleBlock(), _ => CanExecuteAction());
            ResetPasswordCommand = new RelayCommand(_ => ResetPassword(), _ => CanExecuteAction());
            SaveRoleCommand = new RelayCommand(_ => SaveRole(), _ => CanExecuteAction());

            LoadData();
        }

        private bool CanExecuteAction() => SelectedUser != null && SelectedUser.UserId != _currentUser.UserId;

        private void LoadData()
        {
            using (var db = new AutoPartsStoreDBEntities())
            {
                var users = db.Users.Include("Roles").ToList();
                var roles = db.Roles.ToList();
                Users.Clear();
                foreach (var u in users) Users.Add(u);
                Roles.Clear();
                foreach (var r in roles) Roles.Add(r);
            }
            // После загрузки данных обновляем команды
            CommandManager.InvalidateRequerySuggested();
            RefreshCommand.RaiseCanExecuteChanged();
            ToggleBlockCommand.RaiseCanExecuteChanged();
            ResetPasswordCommand.RaiseCanExecuteChanged();
            SaveRoleCommand.RaiseCanExecuteChanged();
        }

        private void ToggleBlock()
        {
            if (SelectedUser == null) return;
            using (var db = new AutoPartsStoreDBEntities())
            {
                var user = db.Users.Find(SelectedUser.UserId);
                if (user != null)
                {
                    user.IsActive = !user.IsActive;
                    db.SaveChanges();
                    SnackbarService.Show($"Пользователь {user.Login} {(user.IsActive ? "разблокирован" : "заблокирован")}");
                    LoadData();
                }
            }
        }

        private void ResetPassword()
        {
            if (SelectedUser == null) return;
            string newPassword = GenerateRandomPassword();
            string newHash = PasswordHelper.HashPassword(newPassword);
            using (var db = new AutoPartsStoreDBEntities())
            {
                var user = db.Users.Find(SelectedUser.UserId);
                if (user != null)
                {
                    user.PasswordHash = newHash;
                    db.SaveChanges();
                    SnackbarService.Show($"Новый пароль для {user.Login}: {newPassword}");
                }
            }
        }

        private void SaveRole()
        {
            if (SelectedUser == null) return;
            using (var db = new AutoPartsStoreDBEntities())
            {
                var user = db.Users.Find(SelectedUser.UserId);
                if (user != null && user.RoleId != SelectedUser.RoleId)
                {
                    user.RoleId = SelectedUser.RoleId;
                    db.SaveChanges();
                    SnackbarService.Show($"Роль пользователя {user.Login} изменена");
                    LoadData();
                }
            }
        }

        private string GenerateRandomPassword(int length = 8)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@#$%";
            var random = new System.Random();
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}