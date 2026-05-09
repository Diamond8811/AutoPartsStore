using System.Windows;
using AutoPartsStore.Infrastructure;
using AutoPartsStore.Models;
using AutoPartsStore.Services;

namespace AutoPartsStore.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public Users CurrentUser { get; }

        public bool IsAdmin => RoleService.IsAdmin(CurrentUser);
        public bool IsManager => RoleService.IsManager(CurrentUser);
        public bool IsWarehouse => RoleService.IsStock(CurrentUser);

        public bool CanSeeProducts => IsAdmin || IsManager;
        public bool CanSeeOrders => IsAdmin || IsManager;
        public bool CanSeeCustomers => IsAdmin || IsManager;
        public bool CanSeeSuppliers => IsAdmin || IsManager;
        public bool CanSeeReports => IsAdmin || IsManager;
        public bool CanSeeStock => IsAdmin || IsWarehouse;
        public bool CanSeeReturns => IsAdmin || IsWarehouse;
        public bool CanSeeAdmin => IsAdmin;
        public bool CanSeeLogs => IsAdmin;

        public RelayCommand ShowDashboardCommand { get; }
        public RelayCommand ShowProductsCommand { get; }
        public RelayCommand ShowOrdersCommand { get; }
        public RelayCommand ShowStockCommand { get; }
        public RelayCommand ShowReturnsCommand { get; }
        public RelayCommand ShowCustomersCommand { get; }
        public RelayCommand ShowSuppliersCommand { get; }
        public RelayCommand ShowReportsCommand { get; }
        public RelayCommand ShowAdminCommand { get; }
        public RelayCommand ShowLogsCommand { get; }
        public RelayCommand LogoutCommand { get; }

        public MainViewModel(Users user)
        {
            CurrentUser = user;

            ShowDashboardCommand = new RelayCommand(_ =>
                CurrentView = new DashboardViewModel());

            ShowProductsCommand = new RelayCommand(_ =>
                CurrentView = new ProductsViewModel(CurrentUser));

            ShowOrdersCommand = new RelayCommand(_ =>
                CurrentView = new OrdersViewModel(CurrentUser));

            ShowStockCommand = new RelayCommand(_ =>
                CurrentView = new StockReceivingsViewModel(CurrentUser));

            ShowReturnsCommand = new RelayCommand(_ =>
                CurrentView = new ReturnsViewModel(CurrentUser));

            ShowCustomersCommand = new RelayCommand(_ =>
                CurrentView = new CustomersViewModel(CurrentUser));

            ShowSuppliersCommand = new RelayCommand(_ =>
                CurrentView = new SuppliersViewModel(CurrentUser));

            ShowReportsCommand = new RelayCommand(_ =>
                CurrentView = new ReportsViewModel(CurrentUser));

            ShowAdminCommand = new RelayCommand(_ =>
                CurrentView = new AdminViewModel(CurrentUser));

            ShowLogsCommand = new RelayCommand(_ =>
                CurrentView = new LogsViewModel());

            LogoutCommand = new RelayCommand(_ => Logout());

            CurrentView = new DashboardViewModel();
        }

        private void Logout()
        {
            var loginWindow = new Views.LoginWindow();
            loginWindow.Show();

            foreach (Window window in Application.Current.Windows)
            {
                if (window is MainWindow)
                {
                    window.Close();
                    break;
                }
            }
        }
    }
}