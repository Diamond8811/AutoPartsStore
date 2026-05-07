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
        public ObservableCollection<object> ReportData { get; set; }

        public RelayCommand SalesReportCommand { get; }
        public RelayCommand StockReportCommand { get; }
        public RelayCommand ReturnsReportCommand { get; }
        public RelayCommand ExportReportCommand { get; }
        public RelayCommand ApplyDatesCommand { get; }   // новая команда

        public ReportsViewModel(Users user)
        {
            ReportData = new ObservableCollection<object>();
            SalesReportCommand = new RelayCommand(_ => GenerateSalesReport());
            StockReportCommand = new RelayCommand(_ => GenerateStockReport());
            ReturnsReportCommand = new RelayCommand(_ => GenerateReturnsReport());
            ExportReportCommand = new RelayCommand(_ => ExportReport());
            ApplyDatesCommand = new RelayCommand(_ => SnackbarService.Show("Даты обновлены. Нажмите кнопку отчёта."));
        }

        private void GenerateSalesReport()
        {
            using (var db = new AutoPartsStoreDBEntities())
            {
                var data = db.Orders
                    .Where(o => o.OrderDate >= DateFrom && o.OrderDate <= DateTo && o.StatusId == 4)
                    .Select(o => new { o.OrderId, o.OrderDate, o.TotalAmount, Customer = o.Customers.FullName })
                    .ToList();
                ReportData.Clear();
                foreach (var d in data) ReportData.Add(d);
                SnackbarService.Show($"Найдено {data.Count} записей");
            }
        }

        private void GenerateStockReport()
        {
            using (var db = new AutoPartsStoreDBEntities())
            {
                var data = db.Products.Select(p => new { p.Article, p.Name, p.StockQuantity, p.Price }).ToList();
                ReportData.Clear();
                foreach (var d in data) ReportData.Add(d);
                SnackbarService.Show($"Найдено {data.Count} позиций");
            }
        }

        private void GenerateReturnsReport()
        {
            using (var db = new AutoPartsStoreDBEntities())
            {
                var data = db.Returns
                    .Where(r => r.ReturnDate >= DateFrom && r.ReturnDate <= DateTo)
                    .Select(r => new { r.ReturnId, r.OrderId, r.ReturnDate, r.RefundAmount, Reason = r.ReturnReasons.Reason })
                    .ToList();
                ReportData.Clear();
                foreach (var d in data) ReportData.Add(d);
                SnackbarService.Show($"Найдено {data.Count} возвратов");
            }
        }

        private void ExportReport()
        {
            if (ReportData.Count == 0)
            {
                SnackbarService.Show("Нет данных для экспорта");
                return;
            }
            var service = new ExcelService();
            service.ExportReportData(ReportData, $"Отчёт_{DateTime.Now:yyyyMMddHHmm}");
            SnackbarService.Show("Отчёт экспортирован на рабочий стол");
        }
    }
}