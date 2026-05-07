using AutoPartsStore.Infrastructure;
using AutoPartsStore.Models;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Objects;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AutoPartsStore.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private string _selectedPeriod = "month";
        public string SelectedPeriod
        {
            get => _selectedPeriod;
            set { _selectedPeriod = value; OnPropertyChanged(); LoadDashboardAsync(); }
        }

        private DateTime _customDateFrom = DateTime.Now.AddMonths(-1);
        public DateTime CustomDateFrom
        {
            get => _customDateFrom;
            set { _customDateFrom = value; OnPropertyChanged(); }
        }
        private DateTime _customDateTo = DateTime.Now;
        public DateTime CustomDateTo
        {
            get => _customDateTo;
            set { _customDateTo = value; OnPropertyChanged(); }
        }

        public int TotalProducts { get; set; }
        public decimal MonthRevenue { get; set; }
        public int TodayOrdersCount { get; set; }
        public decimal MonthReturnsSum { get; set; }

        public SeriesCollection RevenueDailySeries { get; set; }
        public SeriesCollection OrdersByWeekdaySeries { get; set; }
        public SeriesCollection ReturnsByReasonSeries { get; set; }
        public ObservableCollection<string> Weekdays { get; set; }

        public RelayCommand ApplyCustomPeriodCommand { get; }

        public DashboardViewModel()
        {
            Weekdays = new ObservableCollection<string> { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс" };
            ApplyCustomPeriodCommand = new RelayCommand(_ => LoadDashboardAsync());
            LoadDashboardAsync();
        }

        private async void LoadDashboardAsync()
        {
            await ExecuteAsync("загрузки аналитики", async () =>
            {
                using (var db = new AutoPartsStoreDBEntities())
                {
                    DateTime startDate, endDate;
                    if (SelectedPeriod == "custom")
                    {
                        startDate = CustomDateFrom.Date;
                        endDate = CustomDateTo.Date.AddDays(1);
                    }
                    else
                    {
                        var now = DateTime.Now;
                        switch (SelectedPeriod)
                        {
                            case "month":
                                startDate = new DateTime(now.Year, now.Month, 1);
                                endDate = startDate.AddMonths(1);
                                break;
                            case "quarter":
                                int quarter = (now.Month - 1) / 3;
                                startDate = new DateTime(now.Year, quarter * 3 + 1, 1);
                                endDate = startDate.AddMonths(3);
                                break;
                            case "year":
                                startDate = new DateTime(now.Year, 1, 1);
                                endDate = startDate.AddYears(1);
                                break;
                            default:
                                startDate = DateTime.Now.AddDays(-30);
                                endDate = DateTime.Now;
                                break;
                        }
                    }

                    // Данные для графиков
                    int totalProducts = 0;
                    decimal monthRevenue = 0;
                    int todayOrdersCount = 0;
                    decimal monthReturnsSum = 0;
                    System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<DateTime?, decimal>> dailyRevenue = null;
                    System.Collections.Generic.Dictionary<int, int> weekdayOrders = null;
                    System.Collections.Generic.List<dynamic> returnsByReason = null;

                    await Task.Run(() =>
                    {
                        totalProducts = db.Products.Count();
                        monthRevenue = db.Orders
                            .Where(o => o.OrderDate >= startDate && o.OrderDate < endDate && o.StatusId == 4)
                            .Sum(o => o.TotalAmount) ?? 0;
                        todayOrdersCount = db.Orders
                            .Count(o => EntityFunctions.TruncateTime(o.OrderDate) == DateTime.Today);
                        monthReturnsSum = db.Returns
                            .Where(r => r.ReturnDate >= startDate && r.ReturnDate < endDate)
                            .Sum(r => r.RefundAmount) ?? 0;

                        // Доход по дням
                        dailyRevenue = db.Orders
                            .Where(o => o.OrderDate >= startDate && o.OrderDate < endDate && o.StatusId == 4)
                            .GroupBy(o => EntityFunctions.TruncateTime(o.OrderDate))
                            .Select(g => new { Date = g.Key, Total = g.Sum(x => x.TotalAmount) ?? 0 })
                            .OrderBy(x => x.Date)
                            .AsEnumerable()
                            .Select(x => new System.Collections.Generic.KeyValuePair<DateTime?, decimal>(x.Date, x.Total))
                            .ToList();

                        // Возвраты по причинам
                        returnsByReason = db.Returns
                            .Where(r => r.ReturnDate >= startDate && r.ReturnDate < endDate)
                            .GroupBy(r => r.ReturnReasons.Reason)
                            .Select(g => new { Reason = g.Key, Sum = g.Sum(x => x.RefundAmount) ?? 0 })
                            .ToList<dynamic>();

                        // Заказы по дням недели – сначала забираем даты, потом группируем в памяти
                        var orderDates = db.Orders
                            .Where(o => o.OrderDate >= startDate && o.OrderDate < endDate)
                            .Select(o => o.OrderDate)
                            .ToList();

                        weekdayOrders = orderDates
                            .GroupBy(d => (int)d.DayOfWeek) // DayOfWeek выполняется на клиенте
                            .ToDictionary(g => g.Key, g => g.Count());
                    });

                    // Обновляем простые свойства в UI-потоке
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        TotalProducts = totalProducts;
                        MonthRevenue = monthRevenue;
                        TodayOrdersCount = todayOrdersCount;
                        MonthReturnsSum = monthReturnsSum;
                        OnPropertyChanged(nameof(TotalProducts));
                        OnPropertyChanged(nameof(MonthRevenue));
                        OnPropertyChanged(nameof(TodayOrdersCount));
                        OnPropertyChanged(nameof(MonthReturnsSum));
                    });

                    // Создаём графики (только в UI-потоке)
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        // Линейный график дохода по дням
                        var lineValues = new ChartValues<decimal>();
                        foreach (var d in dailyRevenue)
                            if (d.Key.HasValue)
                                lineValues.Add(d.Value);
                        RevenueDailySeries = new SeriesCollection
                        {
                            new LineSeries { Title = "Доход", Values = lineValues, PointGeometrySize = 8, Fill = System.Windows.Media.Brushes.Transparent }
                        };
                        OnPropertyChanged(nameof(RevenueDailySeries));

                        // Столбчатый график заказов по дням недели
                        var columnValues = new ChartValues<int>();
                        // Приводим дни недели к формату: Пн=1, Вс=7
                        for (int i = 1; i <= 7; i++)
                        {
                            columnValues.Add(weekdayOrders.ContainsKey(i) ? weekdayOrders[i] : 0);
                        }
                        OrdersByWeekdaySeries = new SeriesCollection
                        {
                            new ColumnSeries { Title = "Заказы", Values = columnValues }
                        };
                        OnPropertyChanged(nameof(OrdersByWeekdaySeries));

                        // Круговая диаграмма возвратов по причинам
                        var pieSeries = new SeriesCollection();
                        foreach (var item in returnsByReason)
                        {
                            pieSeries.Add(new PieSeries
                            {
                                Title = item.Reason,
                                Values = new ChartValues<decimal> { item.Sum },
                                DataLabels = true
                            });
                        }
                        ReturnsByReasonSeries = pieSeries;
                        OnPropertyChanged(nameof(ReturnsByReasonSeries));
                    });
                }
            });
        }
    }
}