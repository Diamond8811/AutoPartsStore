using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using AutoPartsStore.Infrastructure;
using AutoPartsStore.Models;
using AutoPartsStore.Services;

namespace AutoPartsStore.ViewModels
{
    public class CreateReturnViewModel : BaseViewModel
    {
        private readonly ReturnService _returnService = new ReturnService();
        private readonly Users _currentUser;

        public ObservableCollection<Orders> Orders { get; set; }
        public ObservableCollection<ReturnReasons> Reasons { get; set; }
        public ObservableCollection<OrderItems> OrderItems { get; set; }
        public ObservableCollection<ReturnCartItem> ReturnCart { get; set; }

        private Orders _selectedOrder;
        public Orders SelectedOrder
        {
            get => _selectedOrder;
            set { _selectedOrder = value; OnPropertyChanged(); LoadOrderItems(); }
        }

        private ReturnReasons _selectedReason;
        public ReturnReasons SelectedReason
        {
            get => _selectedReason;
            set { _selectedReason = value; OnPropertyChanged(); }
        }

        private OrderItems _selectedOrderItem;
        public OrderItems SelectedOrderItem
        {
            get => _selectedOrderItem;
            set { _selectedOrderItem = value; OnPropertyChanged(); }
        }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set { _quantity = value; OnPropertyChanged(); }
        }

        public RelayCommand AddToCartCommand { get; }
        public RelayCommand SaveCommand { get; }

        public CreateReturnViewModel(Users currentUser)
        {
            _currentUser = currentUser;
            Orders = new ObservableCollection<Orders>();
            Reasons = new ObservableCollection<ReturnReasons>();
            OrderItems = new ObservableCollection<OrderItems>();
            ReturnCart = new ObservableCollection<ReturnCartItem>();

            using (var db = new AutoPartsStoreDBEntities())
            {
                foreach (var o in db.Orders.ToList()) Orders.Add(o);
                foreach (var r in db.ReturnReasons.ToList()) Reasons.Add(r);
            }

            AddToCartCommand = new RelayCommand(_ =>
            {
                if (SelectedOrderItem == null)
                {
                    SnackbarService.Show("Выберите товар из заказа");
                    return;
                }
                if (Quantity <= 0)
                {
                    SnackbarService.Show("Количество должно быть больше 0");
                    return;
                }
                if (Quantity > SelectedOrderItem.Quantity)
                {
                    SnackbarService.Show($"Нельзя вернуть больше {SelectedOrderItem.Quantity} шт.");
                    return;
                }
                ReturnCart.Add(new ReturnCartItem { OrderItem = SelectedOrderItem, Quantity = Quantity });
                SnackbarService.Show("Позиция добавлена в возврат");
                SelectedOrderItem = null;
                Quantity = 0;
            });

            SaveCommand = new RelayCommand(_ =>
            {
                if (SelectedOrder == null)
                {
                    SnackbarService.Show("Выберите заказ");
                    return;
                }
                if (SelectedReason == null)
                {
                    SnackbarService.Show("Выберите причину возврата");
                    return;
                }
                if (ReturnCart.Count == 0)
                {
                    SnackbarService.Show("Добавьте хотя бы одну позицию");
                    return;
                }
                var items = ReturnCart.Select(i => (i.OrderItem, i.Quantity)).ToList();
                bool success = _returnService.CreateReturn(SelectedOrder.OrderId, SelectedReason.ReasonId, items, _currentUser.UserId);
                if (success)
                {
                    SnackbarService.Show("Возврат создан");
                    Application.Current.Windows.OfType<Views.CreateReturnWindow>().FirstOrDefault()?.Close();
                }
                else
                {
                    SnackbarService.Show("Ошибка при создании возврата");
                }
            });
        }

        private void LoadOrderItems()
        {
            if (SelectedOrder == null) return;
            using (var db = new AutoPartsStoreDBEntities())
            {
                var items = db.OrderItems.Include("Products").Where(oi => oi.OrderId == SelectedOrder.OrderId).ToList();
                OrderItems.Clear();
                foreach (var i in items) OrderItems.Add(i);
            }
        }
    }

    public class ReturnCartItem
    {
        public OrderItems OrderItem { get; set; }
        public int Quantity { get; set; }
    }
}