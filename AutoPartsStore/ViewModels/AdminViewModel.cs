using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using AutoPartsStore.Infrastructure;
using AutoPartsStore.Models;
using AutoPartsStore.Services;

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
                }
            }
        }

        public RelayCommand AddUserCommand { get; }
        public RelayCommand DeleteUserCommand { get; }
        public RelayCommand ToggleBlockCommand { get; }
        public RelayCommand ResetPasswordCommand { get; }
        public RelayCommand GeneratePasswordCommand { get; }
        public RelayCommand SaveUserCommand { get; }

        public AdminViewModel(Users currentUser)
        {
            _currentUser = currentUser;

            Users = new ObservableCollection<Users>();
            Roles = new ObservableCollection<Roles>();

            AddUserCommand = new RelayCommand(_ => AddUser());
            DeleteUserCommand = new RelayCommand(_ => DeleteUser(), _ => SelectedUser != null);
            ToggleBlockCommand = new RelayCommand(_ => ToggleBlock(), _ => SelectedUser != null);
            ResetPasswordCommand = new RelayCommand(_ => ResetPassword(), _ => SelectedUser != null);
            GeneratePasswordCommand = new RelayCommand(_ => GeneratePassword(), _ => SelectedUser != null);
            SaveUserCommand = new RelayCommand(_ => SaveUser(), _ => SelectedUser != null);

            LoadData();
        }

        private void LoadData()
        {
            using (var db = new AutoPartsStoreDBEntities())
            {
                Users.Clear();
                foreach (var u in db.Users.Include("Roles").ToList())
                    Users.Add(u);

                Roles.Clear();
                foreach (var r in db.Roles.ToList())
                    Roles.Add(r);
            }
        }

        private void AddUser()
        {
            using (var db = new AutoPartsStoreDBEntities())
            {
                if (db.Users.Any(u => u.Login == "new_user"))
                {
                    SnackbarService.Show("Пользователь new_user уже существует");
                    return;
                }

                var defaultRole = db.Roles.FirstOrDefault();

                var newUser = new Users
                {
                    Login = "new_user",
                    FullName = "Новый пользователь",
                    PasswordHash = PasswordHelper.HashPassword("1234"),
                    RoleId = defaultRole?.RoleId ?? 1,
                    IsActive = true
                };

                db.Users.Add(newUser);
                db.SaveChanges();

                new LogService().Log(_currentUser.UserId, "INSERT", "Users", newUser.UserId, "Создан новый пользователь");

                LoadData();
                SnackbarService.Show("Пользователь добавлен. Пароль: 1234");
            }
        }

        private void DeleteUser()
        {
            using (var db = new AutoPartsStoreDBEntities())
            {
                var user = db.Users.Find(SelectedUser.UserId);

                if (user == null) return;

                if (user.UserId == _currentUser.UserId)
                {
                    SnackbarService.Show("Нельзя удалить самого себя");
                    return;
                }

                db.Users.Remove(user);
                db.SaveChanges();

                new LogService().Log(_currentUser.UserId, "DELETE", "Users", user.UserId, "Удалён пользователь");

                LoadData();
                SnackbarService.Show("Пользователь удалён");
            }
        }

        private void ToggleBlock()
        {
            using (var db = new AutoPartsStoreDBEntities())
            {
                var user = db.Users.Find(SelectedUser.UserId);
                if (user == null) return;

                user.IsActive = !user.IsActive;
                db.SaveChanges();

                new LogService().Log(_currentUser.UserId, "UPDATE", "Users", user.UserId, "Изменён статус активности");

                SnackbarService.Show(user.IsActive ? "Пользователь разблокирован" : "Пользователь заблокирован");
                LoadData();
            }
        }

        private void ResetPassword()
        {
            using (var db = new AutoPartsStoreDBEntities())
            {
                var user = db.Users.Find(SelectedUser.UserId);
                if (user == null) return;

                user.PasswordHash = PasswordHelper.HashPassword("1234");
                db.SaveChanges();

                new LogService().Log(_currentUser.UserId, "UPDATE", "Users", user.UserId, "Сброс пароля");

                SnackbarService.Show("Пароль сброшен на 1234");
            }
        }

        private void GeneratePassword()
        {
            string newPassword = GenerateRandomPassword();

            using (var db = new AutoPartsStoreDBEntities())
            {
                var user = db.Users.Find(SelectedUser.UserId);
                if (user == null) return;

                user.PasswordHash = PasswordHelper.HashPassword(newPassword);
                db.SaveChanges();

                new LogService().Log(_currentUser.UserId, "UPDATE", "Users", user.UserId, "Сгенерирован новый пароль");

                SnackbarService.Show("Новый пароль: " + newPassword);
            }
        }

        private void SaveUser()
        {
            using (var db = new AutoPartsStoreDBEntities())
            {
                if (db.Users.Any(u => u.Login == SelectedUser.Login && u.UserId != SelectedUser.UserId))
                {
                    SnackbarService.Show("Логин уже занят");
                    return;
                }

                var user = db.Users.Find(SelectedUser.UserId);
                if (user == null) return;

                user.Login = SelectedUser.Login;
                user.FullName = SelectedUser.FullName;
                user.RoleId = SelectedUser.Roles.RoleId;

                db.SaveChanges();

                new LogService().Log(_currentUser.UserId, "UPDATE", "Users", user.UserId, "Обновлены данные пользователя");

                SnackbarService.Show("Изменения сохранены");
                LoadData();
            }
        }

        private string GenerateRandomPassword(int length = 8)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}