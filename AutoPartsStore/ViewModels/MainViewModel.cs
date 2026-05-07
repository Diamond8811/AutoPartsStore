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

        public RelayCommand ShowDashboardCommand { get; }
        public RelayCommand ShowProductsCommand { get; }
        public RelayCommand ShowOrdersCommand { get; }
        public RelayCommand ShowStockCommand { get; }
        public RelayCommand ShowReturnsCommand { get; }
        public RelayCommand ShowCustomersCommand { get; }
        public RelayCommand ShowSuppliersCommand { get; }
        public RelayCommand ShowReportsCommand { get; }
        public RelayCommand ShowAdminCommand { get; }

        public MainViewModel(Users user)
        {
            CurrentUser = user;
            ShowDashboardCommand = new RelayCommand(_ => CurrentView = new DashboardViewModel());
            ShowProductsCommand = new RelayCommand(_ => CurrentView = new ProductsViewModel(CurrentUser));
            ShowOrdersCommand = new RelayCommand(_ => CurrentView = new OrdersViewModel(CurrentUser));
            ShowStockCommand = new RelayCommand(_ => CurrentView = new StockReceivingsViewModel(CurrentUser));
            ShowReturnsCommand = new RelayCommand(_ => CurrentView = new ReturnsViewModel(CurrentUser));
            ShowCustomersCommand = new RelayCommand(_ => CurrentView = new CustomersViewModel(CurrentUser));
            ShowSuppliersCommand = new RelayCommand(_ => CurrentView = new SuppliersViewModel(CurrentUser));
            ShowReportsCommand = new RelayCommand(_ => CurrentView = new ReportsViewModel(CurrentUser));
            ShowAdminCommand = new RelayCommand(_ => CurrentView = new AdminViewModel(CurrentUser));

            CurrentView = new DashboardViewModel();
        }
    }
}