using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using AutoPartsStore.ViewModels;

namespace AutoPartsStore.Views
{
    public partial class ReportsView : UserControl
    {
        private static readonly Dictionary<string, string> ColumnNames =
            new Dictionary<string, string>
        {
            { "OrderId", "№ Заказа" },
            { "OrderDate", "Дата" },
            { "Customer", "Клиент" },
            { "TotalAmount", "Сумма" },

            { "Article", "Артикул" },
            { "Name", "Название" },
            { "Brand", "Бренд" },
            { "Category", "Категория" },
            { "StockQuantity", "Остаток" },
            { "Price", "Цена" },
            { "TotalValue", "Стоимость" },

            { "ReturnId", "№ Возврата" },
            { "ReturnDate", "Дата возврата" },
            { "Reason", "Причина" },
            { "RefundAmount", "Сумма возврата" }
        };

        public ReportsView()
        {
            InitializeComponent();

            DataContextChanged += (s, e) =>
            {
                var vm = DataContext as ReportsViewModel;
                if (vm != null)
                {
                    vm.PropertyChanged += (sender, args) =>
                    {
                        if (args.PropertyName == "ReportData")
                        {
                            GenerateColumns(vm);
                        }
                    };
                }
            };
        }

        private void GenerateColumns(ReportsViewModel vm)
        {
            ReportGrid.Columns.Clear();

            if (vm.ReportData == null || vm.ReportData.Count == 0)
                return;

            var firstItem = vm.ReportData[0];
            var properties = firstItem.GetType().GetProperties();

            foreach (var prop in properties)
            {
                var column = new DataGridTextColumn();

                if (ColumnNames.ContainsKey(prop.Name))
                    column.Header = ColumnNames[prop.Name];
                else
                    column.Header = prop.Name;

                var binding = new Binding(prop.Name);

                if (prop.PropertyType == typeof(System.DateTime) ||
                    prop.PropertyType == typeof(System.DateTime?))
                {
                    binding.StringFormat = "dd.MM.yyyy HH:mm";
                }

                column.Binding = binding;

                ReportGrid.Columns.Add(column);
            }
        }
    }
}