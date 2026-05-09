using AutoPartsStore.Infrastructure;
using AutoPartsStore.Models;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace AutoPartsStore.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private string _selectedPeriod = "month";
        public string SelectedPeriod
        {
            get => _selectedPeriod;
            set
            {
                _selectedPeriod = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsCustomPeriod));
                LoadDashboard();
            }
        }

        public bool IsCustomPeriod => SelectedPeriod == "custom";

        public DateTime CustomDateFrom { get; set; } = DateTime.Now.AddMonths(-1);
        public DateTime CustomDateTo { get; set; } = DateTime.Now;

        public int TotalProducts { get; set; }
        public decimal PeriodRevenue { get; set; }
        public int PeriodOrdersCount { get; set; }
        public decimal PeriodReturnsSum { get; set; }

        public SeriesCollection RevenueDailySeries { get; set; }
        public SeriesCollection OrdersByWeekdaySeries { get; set; }
        public SeriesCollection ReturnsByReasonSeries { get; set; }

        public ObservableCollection<string> RevenueDailyLabels { get; set; }
        public ObservableCollection<string> Weekdays { get; set; }

        public Func<double, string> CurrencyFormatter { get; set; }

        public RelayCommand ApplyCustomPeriodCommand { get; }

        public DashboardViewModel()
        {
            CurrencyFormatter = value => value.ToString("N0") + " ₽";
            Weekdays = new ObservableCollection<string>
            {
                "Пн","Вт","Ср","Чт","Пт","Сб","Вс"
            };

            ApplyCustomPeriodCommand = new RelayCommand(_ => LoadDashboard());
            LoadDashboard();
        }

        private void LoadDashboard()
        {
            using (var db = new AutoPartsStoreDBEntities())
            {
                DateTime startDate;
                DateTime endDate;
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

                    case "custom":
                        startDate = CustomDateFrom.Date;
                        endDate = CustomDateTo.Date.AddDays(1);
                        break;

                    default:
                        startDate = now.AddMonths(-1);
                        endDate = now;
                        break;
                }

                TotalProducts = db.Products.Count();

                var orders = db.Orders
                    .Where(o => o.OrderDate >= startDate &&
                                o.OrderDate < endDate &&
                                o.StatusId == 4)
                    .ToList();

                PeriodRevenue = orders.Sum(o => o.TotalAmount ?? 0);
                PeriodOrdersCount = orders.Count;

                PeriodReturnsSum = db.Returns
                    .Where(r => r.ReturnDate >= startDate &&
                                r.ReturnDate < endDate)
                    .ToList()
                    .Sum(r => r.RefundAmount ?? 0);

                OnPropertyChanged(nameof(TotalProducts));
                OnPropertyChanged(nameof(PeriodRevenue));
                OnPropertyChanged(nameof(PeriodOrdersCount));
                OnPropertyChanged(nameof(PeriodReturnsSum));

                var grouped = orders
                    .GroupBy(o => o.OrderDate.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        Total = g.Sum(x => x.TotalAmount ?? 0)
                    })
                    .OrderBy(x => x.Date)
                    .ToList();

                RevenueDailyLabels = new ObservableCollection<string>();
                var revenueValues = new ChartValues<decimal>();

                foreach (var item in grouped)
                {
                    RevenueDailyLabels.Add(item.Date.ToString("dd.MM"));
                    revenueValues.Add(item.Total);
                }

                if (!revenueValues.Any())
                    revenueValues.Add(0);

                RevenueDailySeries = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Доход",
                        Values = revenueValues,
                        PointGeometrySize = 6,
                        Fill = System.Windows.Media.Brushes.Transparent
                    }
                };

                OnPropertyChanged(nameof(RevenueDailyLabels));
                OnPropertyChanged(nameof(RevenueDailySeries));

                var weekdayGroups = orders
                    .GroupBy(o => ((int)o.OrderDate.DayOfWeek + 6) % 7)
                    .ToDictionary(g => g.Key, g => g.Count());

                var weekdayValues = new ChartValues<int>();

                for (int i = 0; i < 7; i++)
                {
                    weekdayValues.Add(
                        weekdayGroups.ContainsKey(i)
                            ? weekdayGroups[i]
                            : 0);
                }

                OrdersByWeekdaySeries = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Заказы",
                        Values = weekdayValues
                    }
                };

                OnPropertyChanged(nameof(OrdersByWeekdaySeries));

                var returns = db.Returns
                    .Where(r => r.ReturnDate >= startDate &&
                                r.ReturnDate < endDate)
                    .ToList()
                    .GroupBy(r => r.ReturnReasons.Reason)
                    .Select(g => new
                    {
                        Reason = g.Key,
                        Sum = g.Sum(x => x.RefundAmount ?? 0)
                    })
                    .ToList();

                var pieSeries = new SeriesCollection();

                foreach (var r in returns)
                {
                    pieSeries.Add(new PieSeries
                    {
                        Title = r.Reason,
                        Values = new ChartValues<decimal> { r.Sum },
                        DataLabels = true
                    });
                }

                if (!pieSeries.Any())
                {
                    pieSeries.Add(new PieSeries
                    {
                        Title = "Нет данных",
                        Values = new ChartValues<decimal> { 1 },
                        DataLabels = true
                    });
                }

                ReturnsByReasonSeries = pieSeries;
                OnPropertyChanged(nameof(ReturnsByReasonSeries));
            }
        }
    }
}