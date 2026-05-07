using System.Linq;
using AutoPartsStore.Infrastructure;
using AutoPartsStore.Models;

namespace AutoPartsStore.Services
{
    public class AuthService
    {
        public Users Authenticate(string login, string password)
        {
            using (var db = new AutoPartsStoreDBEntities())
            {
                var user = db.Users.Include("Roles").FirstOrDefault(u => u.Login == login && u.IsActive);
                if (user == null) return null;

                // Проверка на null хеша
                if (string.IsNullOrEmpty(user.PasswordHash))
                {
                    System.Diagnostics.Debug.WriteLine("PasswordHash is null or empty for user " + login);
                    return null;
                }

                bool isValid = PasswordHelper.VerifyPassword(password, user.PasswordHash);
                return isValid ? user : null;
            }
        }
    }
}