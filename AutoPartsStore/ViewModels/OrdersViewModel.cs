using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using AutoPartsStore.Infrastructure;
using AutoPartsStore.Models;
using AutoPartsStore.Services;

namespace AutoPartsStore.ViewModels
{
    public class OrdersViewModel : BaseViewModel
    {
        private readonly Users _currentUser;

        public ObservableCollection<Orders> Orders { get; set; }
        public ObservableCollection<OrderStatuses> Statuses { get; set; }
        public ObservableCollection<OrderItems> OrderItems { get; set; }

        private Orders _selectedOrder;
        public Orders SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                if (_selectedOrder != value)
                {
                    _selectedOrder = value;
                    OnPropertyChanged();
                    LoadOrderItems();
                }
            }
        }

        public OrderStatuses SelectedStatusFilter { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

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

            FilterCommand = new RelayCommand(_ => LoadOrders());
            ResetFilterCommand = new RelayCommand(_ => ResetFilters());
            ChangeStatusCommand = new RelayCommand(_ => ChangeOrderStatus());
            ExportOrderCommand = new RelayCommand(_ => ExportOrder());
            CreateOrderCommand = new RelayCommand(_ => OpenCreateOrderWindow());

            LoadStatuses();
            LoadOrders();
        }

        private void LoadStatuses()
        {
            using (var db = new AutoPartsStoreDBEntities())
            {
                Statuses.Clear();
                foreach (var s in db.OrderStatuses.ToList())
                    Statuses.Add(s);
            }
        }

        private void LoadOrders()
        {
            using (var db = new AutoPartsStoreDBEntities())
            {
                var query = db.Orders
                    .Include(o => o.Customers)
                    .Include(o => o.OrderStatuses)
                    .AsQueryable();

                if (SelectedStatusFilter != null)
                    query = query.Where(o => o.StatusId == SelectedStatusFilter.StatusId);

                if (DateFrom.HasValue)
                    query = query.Where(o => o.OrderDate >= DateFrom.Value);

                if (DateTo.HasValue)
                    query = query.Where(o => o.OrderDate <= DateTo.Value);

                var list = query.OrderByDescending(o => o.OrderDate).ToList();

                Orders.Clear();
                foreach (var o in list)
                    Orders.Add(o);
            }
        }

        private void LoadOrderItems()
        {
            OrderItems.Clear();

            if (SelectedOrder == null)
                return;

            using (var db = new AutoPartsStoreDBEntities())
            {
                var items = db.OrderItems
                    .Include(oi => oi.Products)
                    .Where(oi => oi.OrderId == SelectedOrder.OrderId)
                    .ToList();

                foreach (var item in items)
                    OrderItems.Add(item);
            }
        }

        private void ResetFilters()
        {
            SelectedStatusFilter = null;
            DateFrom = null;
            DateTo = null;
            LoadOrders();
        }

        private void ChangeOrderStatus()
        {
            var selected = Orders.FirstOrDefault(o => o.NewStatusId.HasValue);

            if (selected == null)
            {
                SnackbarService.Show("Выберите новый статус");
                return;
            }

            using (var db = new AutoPartsStoreDBEntities())
            {
                var order = db.Orders.Find(selected.OrderId);

                if (order != null)
                {
                    order.StatusId = selected.NewStatusId.Value;
                    db.SaveChanges();
                }
            }

            SnackbarService.Show($"Статус заказа №{selected.OrderId} изменён");
            LoadOrders();
        }

        private void ExportOrder()
        {
            if (SelectedOrder == null)
                return;

            using (var db = new AutoPartsStoreDBEntities())
            {
                var order = db.Orders
                    .Include("Customers")
                    .Include("OrderStatuses")
                    .Include("OrderItems.Products")
                    .FirstOrDefault(o => o.OrderId == SelectedOrder.OrderId);

                if (order != null)
                {
                    new ExcelService().ExportOrder(order);
                    SnackbarService.Show($"Заказ №{order.OrderId} экспортирован");
                }
            }
        }

        private void OpenCreateOrderWindow()
        {
            var window = new Views.CreateOrderWindow(_currentUser);
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
            LoadOrders();
        }
    }
}