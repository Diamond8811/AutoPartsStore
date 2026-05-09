using System;
using System.Collections.ObjectModel;
using System.Linq;
using AutoPartsStore.Infrastructure;
using AutoPartsStore.Models;
using AutoPartsStore.Services;
using System.Data.Entity;

namespace AutoPartsStore.ViewModels
{
    public class ReportsViewModel : BaseViewModel
    {
        public DateTime DateFrom { get; set; } = DateTime.Now.AddMonths(-1);
        public DateTime DateTo { get; set; } = DateTime.Now;

        public ObservableCollection<dynamic> ReportData { get; set; }

        public RelayCommand SalesReportCommand { get; }
        public RelayCommand StockReportCommand { get; }
        public RelayCommand ReturnsReportCommand { get; }
        public RelayCommand ExportReportCommand { get; }
        public RelayCommand ApplyDatesCommand { get; }

        public ReportsViewModel(Users user)
        {
            ReportData = new ObservableCollection<dynamic>();

            SalesReportCommand = new RelayCommand(_ => GenerateSalesReport());
            StockReportCommand = new RelayCommand(_ => GenerateStockReport());
            ReturnsReportCommand = new RelayCommand(_ => GenerateReturnsReport());
            ExportReportCommand = new RelayCommand(_ => ExportReport());
            ApplyDatesCommand = new RelayCommand(_ =>
                SnackbarService.Show("Даты обновлены. Нажмите кнопку отчёта."));
        }

        private void GenerateSalesReport()
        {
            using (var db = new AutoPartsStoreDBEntities())
            {
                var data = db.Orders
                    .Where(o => o.OrderDate >= DateFrom &&
                                o.OrderDate <= DateTo &&
                                o.StatusId == 4)
                    .Select(o => new
                    {
                        OrderId = o.OrderId,
                        OrderDate = o.OrderDate,
                        Customer = o.Customers.FullName,
                        TotalAmount = o.TotalAmount
                    })
                    .ToList();

                RefreshData(data);
                SnackbarService.Show($"Найдено {data.Count} записей");
            }
        }

        private void GenerateStockReport()
        {
            using (var db = new AutoPartsStoreDBEntities())
            {
                var data = db.Products
                    .Include(p => p.Brands)
                    .Include(p => p.Categories)
                    .Select(p => new
                    {
                        Article = p.Article,
                        Name = p.Name,
                        Brand = p.Brands.Name,
                        Category = p.Categories.Name,
                        StockQuantity = p.StockQuantity,
                        Price = p.Price,
                        TotalValue = p.StockQuantity * p.Price
                    })
                    .ToList();

                RefreshData(data);
                SnackbarService.Show($"Найдено {data.Count} позиций");
            }
        }

        private void GenerateReturnsReport()
        {
            using (var db = new AutoPartsStoreDBEntities())
            {
                var data = db.Returns
                    .Include(r => r.ReturnReasons)
                    .Include(r => r.Orders)
                    .Include(r => r.Orders.Customers)
                    .Where(r => r.ReturnDate >= DateFrom &&
                                r.ReturnDate <= DateTo)
                    .Select(r => new
                    {
                        ReturnId = r.ReturnId,
                        OrderId = r.OrderId,
                        Customer = r.Orders.Customers.FullName,
                        ReturnDate = r.ReturnDate,
                        Reason = r.ReturnReasons.Reason,
                        RefundAmount = r.RefundAmount
                    })
                    .ToList();

                RefreshData(data);
                SnackbarService.Show($"Найдено {data.Count} возвратов");
            }
        }

        private void RefreshData(System.Collections.IEnumerable data)
        {
            ReportData.Clear();

            foreach (var item in data)
                ReportData.Add(item);

            OnPropertyChanged(nameof(ReportData));
        }

        private void ExportReport()
        {
            if (ReportData.Count == 0)
            {
                SnackbarService.Show("Нет данных для экспорта");
                return;
            }

            var service = new ExcelService();
            service.ExportReportData(ReportData, $"Report_{DateTime.Now:yyyyMMddHHmm}");
            SnackbarService.Show("Отчёт экспортирован на рабочий стол");
        }
    }
}