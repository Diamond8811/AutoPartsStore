using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AutoPartsStore.Infrastructure;
using AutoPartsStore.Models;
using AutoPartsStore.Services;

namespace AutoPartsStore.ViewModels
{
    public class OrdersViewModel : BaseViewModel
    {
        private readonly Users _currentUser;
        private Orders _selectedOrder;
        private OrderStatuses _selectedStatusFilter;
        private DateTime? _dateFrom;
        private DateTime? _dateTo;
        private OrderStatuses _selectedOrderStatus;

        public ObservableCollection<Orders> Orders { get; set; }
        public ObservableCollection<OrderStatuses> Statuses { get; set; }
        public ObservableCollection<OrderItems> OrderItems { get; set; }

        public Orders SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                if (_selectedOrder != value)
                {
                    _selectedOrder = value;
                    OnPropertyChanged();
                    CommandManager.InvalidateRequerySuggested();
                    ChangeStatusCommand?.RaiseCanExecuteChanged();
                    LoadOrderItemsAsync();
                }
            }
        }

        public OrderStatuses SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set { _selectedStatusFilter = value; OnPropertyChanged(); LoadOrdersAsync(); }
        }

        public DateTime? DateFrom
        {
            get => _dateFrom;
            set { _dateFrom = value; OnPropertyChanged(); }
        }

        public DateTime? DateTo
        {
            get => _dateTo;
            set { _dateTo = value; OnPropertyChanged(); }
        }

        public OrderStatuses SelectedOrderStatus
        {
            get => _selectedOrderStatus;
            set
            {
                if (_selectedOrderStatus != value)
                {
                    _selectedOrderStatus = value;
                    OnPropertyChanged();
                    CommandManager.InvalidateRequerySuggested();
                    ChangeStatusCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        public RelayCommand FilterCommand { get; }
        public RelayCommand ResetFilterCommand { get; }
        public RelayCommand ChangeStatusCommand { get; }
        public RelayCommand ExportOrderCommand { get; }
        public RelayCommand CreateOrderCommand { get; }

        public OrdersViewModel(Users user)
        {
            _currentUser = user;
            Orders = new ObservableCollection<Orders>();
            Statuses = new ObservableCollection<OrderStatuses>();
            OrderItems = new ObservableCollection<OrderItems>();

            FilterCommand = new RelayCommand(_ => LoadOrdersAsync());
            ResetFilterCommand = new RelayCommand(_ => ResetFilters());
            ChangeStatusCommand = new RelayCommand(_ => ChangeOrderStatus(), _ => SelectedOrder != null && SelectedOrderStatus != null);
            ExportOrderCommand = new RelayCommand(_ => ExportOrder(), _ => SelectedOrder != null);
            CreateOrderCommand = new RelayCommand(_ => OpenCreateOrderWindow());

            LoadStatusesAsync();
            LoadOrdersAsync();
        }


        private void OpenCreateOrderWindow()
        {
            var window = new Views.CreateOrderWindow(_currentUser);
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
            LoadOrdersAsync();
        }

        private async void LoadStatusesAsync()
        {
            await ExecuteAsync("загрузки статусов", () =>
            {
                using (var db = new AutoPartsStoreDBEntities())
                {
                    var list = db.OrderStatuses.ToList();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Statuses.Clear();
                        foreach (var s in list) Statuses.Add(s);
                    });
                }
            });
        }

        private async void LoadOrdersAsync()
        {
            await ExecuteAsync("загрузки заказов", () =>
            {
                using (var db = new AutoPartsStoreDBEntities())
                {
                    var query = db.Orders.Include("Customers").Include("OrderStatuses").AsQueryable();
                    if (SelectedStatusFilter != null) query = query.Where(o => o.StatusId == SelectedStatusFilter.StatusId);
                    if (DateFrom.HasValue) query = query.Where(o => o.OrderDate >= DateFrom.Value);
                    if (DateTo.HasValue) query = query.Where(o => o.OrderDate <= DateTo.Value);
                    var list = query.OrderByDescending(o => o.OrderDate).ToList();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Orders.Clear();
                        foreach (var o in list) Orders.Add(o);
                        CommandManager.InvalidateRequerySuggested();
                    });
                }
            });
        }

        private async void LoadOrderItemsAsync()
        {
            if (SelectedOrder == null) return;
            await ExecuteAsync("загрузки состава заказа", () =>
            {
                using (var db = new AutoPartsStoreDBEntities())
                {
                    var items = db.OrderItems.Include("Products").Where(oi => oi.OrderId == SelectedOrder.OrderId).ToList();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        OrderItems.Clear();
                        foreach (var i in items) OrderItems.Add(i);
                    });
                }
            });
        }

        private void ResetFilters()
        {
            SelectedStatusFilter = null; DateFrom = null; DateTo = null;
            LoadOrdersAsync();
        }

        private void ChangeOrderStatus()
        {
            if (SelectedOrder == null || SelectedOrderStatus == null)
            {
                SnackbarService.Show("Выберите новый статус");
                return;
            }
            using (var db = new AutoPartsStoreDBEntities())
            {
                var order = db.Orders.Find(SelectedOrder.OrderId);
                if (order != null)
                {
                    order.StatusId = SelectedOrderStatus.StatusId;
                    db.SaveChanges();
                    SnackbarService.Show($"Статус заказа №{order.OrderId} изменён на {SelectedOrderStatus.Name}");
                    LoadOrdersAsync();
                    SelectedOrderStatus = null;
                }
            }
        }

        private void ExportOrder()
        {
            if (SelectedOrder == null) return;
            using (var db = new AutoPartsStoreDBEntities())
            {
                var order = db.Orders.Include("Customers").Include("OrderStatuses").Include("OrderItems").Include("OrderItems.Products")
                    .FirstOrDefault(o => o.OrderId == SelectedOrder.OrderId);
                if (order != null)
                {
                    new ExcelService().ExportOrder(order);
                    SnackbarService.Show($"Заказ №{order.OrderId} экспортирован на рабочий стол");
                }
            }
        }
    }
}