using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using AutoPartsStore.Infrastructure;
using AutoPartsStore.Models;
using AutoPartsStore.Services;

namespace AutoPartsStore.ViewModels
{
    public class CreateStockReceivingViewModel : BaseViewModel
    {
        private readonly Users _currentUser;
        private readonly StockService _stockService = new StockService();

        public ObservableCollection<Products> Products { get; set; }
        public ObservableCollection<Suppliers> Suppliers { get; set; }
        public ObservableCollection<ReceivingItem> Cart { get; set; }

        private Products _selectedProduct;
        public Products SelectedProduct
        {
            get => _selectedProduct;
            set { _selectedProduct = value; OnPropertyChanged(); }
        }

        private Suppliers _selectedSupplier;
        public Suppliers SelectedSupplier
        {
            get => _selectedSupplier;
            set { _selectedSupplier = value; OnPropertyChanged(); }
        }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set { _quantity = value; OnPropertyChanged(); }
        }

        private decimal _unitPrice;
        public decimal UnitPrice
        {
            get => _unitPrice;
            set { _unitPrice = value; OnPropertyChanged(); }
        }

        public RelayCommand AddToCartCommand { get; }
        public RelayCommand SaveCommand { get; }

        public CreateStockReceivingViewModel(Users user)
        {
            _currentUser = user;
            Products = new ObservableCollection<Products>();
            Suppliers = new ObservableCollection<Suppliers>();
            Cart = new ObservableCollection<ReceivingItem>();

            using (var db = new AutoPartsStoreDBEntities())
            {
                foreach (var p in db.Products.ToList()) Products.Add(p);
                foreach (var s in db.Suppliers.ToList()) Suppliers.Add(s);
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
                if (UnitPrice <= 0)
                {
                    SnackbarService.Show("Цена должна быть больше 0");
                    return;
                }

                Cart.Add(new ReceivingItem
                {
                    Product = SelectedProduct,
                    Quantity = Quantity,
                    Price = UnitPrice
                });

                // Очищаем поля после добавления
                SelectedProduct = null;
                Quantity = 0;
                UnitPrice = 0;
                SnackbarService.Show("Товар добавлен в корзину");
            });

            SaveCommand = new RelayCommand(_ =>
            {
                if (Cart.Count == 0)
                {
                    SnackbarService.Show("Добавьте хотя бы один товар");
                    return;
                }
                if (SelectedSupplier == null)
                {
                    SnackbarService.Show("Выберите поставщика");
                    return;
                }

                bool success = _stockService.CreateStockReceiving(
                    Cart.Select(item => (item.Product, item.Quantity, item.Price)).ToList(),
                    SelectedSupplier.SupplierId,
                    _currentUser.UserId
                );

                if (success)
                {
                    SnackbarService.Show("Поступление создано");
                    Application.Current.Windows.OfType<Views.CreateStockReceivingWindow>().FirstOrDefault()?.Close();
                }
                else
                {
                    SnackbarService.Show("Ошибка создания поступления");
                }
            });
        }
    }
}