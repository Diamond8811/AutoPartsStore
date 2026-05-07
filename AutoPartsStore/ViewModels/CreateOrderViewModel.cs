using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using AutoPartsStore.Infrastructure;
using AutoPartsStore.Models;
using AutoPartsStore.Services;

namespace AutoPartsStore.ViewModels
{
    public class CreateOrderViewModel : BaseViewModel
    {
        private readonly OrderService _orderService = new OrderService();
        private readonly Users _currentUser;

        public ObservableCollection<Customers> Customers { get; set; }
        public ObservableCollection<Products> Products { get; set; }
        public ObservableCollection<OrderCartItem> Cart { get; set; }

        private Customers _selectedClient;
        public Customers SelectedClient
        {
            get => _selectedClient;
            set { _selectedClient = value; OnPropertyChanged(); }
        }

        private Products _selectedProduct;
        public Products SelectedProduct
        {
            get => _selectedProduct;
            set { _selectedProduct = value; OnPropertyChanged(); }
        }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set { _quantity = value; OnPropertyChanged(); }
        }

        public RelayCommand AddToCartCommand { get; }
        public RelayCommand SaveOrderCommand { get; }

        public CreateOrderViewModel(Users currentUser)
        {
            _currentUser = currentUser;
            Customers = new ObservableCollection<Customers>();
            Products = new ObservableCollection<Products>();
            Cart = new ObservableCollection<OrderCartItem>();

            using (var db = new AutoPartsStoreDBEntities())
            {
                foreach (var c in db.Customers.ToList()) Customers.Add(c);
                foreach (var p in db.Products.ToList()) Products.Add(p);
            }

            AddToCartCommand = new RelayCommand(_ =>
            {
                if (SelectedProduct == null)
                {
                    SnackbarService.Show("Выберите товар");
                    return;
                }
                if (Quantity <= 0)
                {
                    SnackbarService.Show("Количество должно быть больше 0");
                    return;
                }
                Cart.Add(new OrderCartItem { Product = SelectedProduct, Quantity = Quantity });
                SnackbarService.Show("Товар добавлен в корзину");
                SelectedProduct = null;
                Quantity = 0;
            });

            SaveOrderCommand = new RelayCommand(_ =>
            {
                if (SelectedClient == null)
                {
                    SnackbarService.Show("Выберите клиента");
                    return;
                }
                if (Cart.Count == 0)
                {
                    SnackbarService.Show("Добавьте товары в заказ");
                    return;
                }
                var items = Cart.Select(i => (i.Product, i.Quantity)).ToList();
                bool success = _orderService.CreateOrder(SelectedClient.CustomerId, items, _currentUser.UserId);
                if (success)
                {
                    SnackbarService.Show("Заказ создан");
                    Application.Current.Windows.OfType<Views.CreateOrderWindow>().FirstOrDefault()?.Close();
                }
                else
                {
                    SnackbarService.Show("Ошибка при создании заказа");
                }
            });
        }
    }
}