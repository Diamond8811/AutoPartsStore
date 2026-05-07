using AutoPartsStore.Models;

namespace AutoPartsStore.Services
{
    public static class RoleService
    {
        public static bool IsAdmin(Users user) => user?.RoleId == 1;
        public static bool IsManager(Users user) => user?.RoleId == 2;
        public static bool IsStock(Users user) => user?.RoleId == 3;

        public static bool CanEditProducts(Users user) => IsAdmin(user) || IsManager(user);
        public static bool CanDeleteProducts(Users user) => IsAdmin(user);
        public static bool CanCreateOrders(Users user) => IsAdmin(user) || IsManager(user);
        public static bool CanManageStock(Users user) => IsAdmin(user) || IsStock(user);
        public static bool CanViewReports(Users user) => IsAdmin(user) || IsManager(user);
    }
}