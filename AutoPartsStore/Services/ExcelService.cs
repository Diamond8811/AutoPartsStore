using ClosedXML.Excel;
using System.IO;
using AutoPartsStore.Models;
using System.Collections.Generic;
using System;

namespace AutoPartsStore.Services
{
    public class ExcelService
    {
        public void ExportOrder(Orders order)
        {
            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("Накладная");
                ws.Cell(1, 1).Value = $"Заказ №{order.OrderId}";
                ws.Cell(1, 1).Style.Font.Bold = true; ws.Cell(1, 1).Style.Font.FontSize = 16;
                ws.Cell(2, 1).Value = $"Дата: {order.OrderDate:dd.MM.yyyy HH:mm}";
                ws.Cell(3, 1).Value = $"Клиент: {order.Customers.FullName}";
                ws.Cell(4, 1).Value = $"Статус: {order.OrderStatuses.Name}";
                int row = 6;
                ws.Cell(row, 1).Value = "Товар"; ws.Cell(row, 2).Value = "Количество"; ws.Cell(row, 3).Value = "Цена"; ws.Cell(row, 4).Value = "Сумма";
                ws.Range(row, 1, row, 4).Style.Font.Bold = true;
                row++;
                foreach (var item in order.OrderItems)
                {
                    ws.Cell(row, 1).Value = item.Products.Name;
                    ws.Cell(row, 2).Value = item.Quantity;
                    ws.Cell(row, 3).Value = item.UnitPrice;
                    ws.Cell(row, 4).Value = item.Quantity * item.UnitPrice;
                    row++;
                }
                row++;
                ws.Cell(row, 3).Value = "ИТОГО:"; ws.Cell(row, 3).Style.Font.Bold = true;
                ws.Cell(row, 4).Value = order.TotalAmount; ws.Cell(row, 4).Style.Font.Bold = true;
                ws.Columns().AdjustToContents();
                ws.Range(6, 1, row, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Range(6, 1, row, 4).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                string fileName = $"Order_{order.OrderId}_{order.OrderDate:yyyyMMddHHmm}.xlsx";
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
                wb.SaveAs(path);
            }
        }

        public void ExportProducts(IEnumerable<Products> products)
        {
            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("Товары");
                ws.Cell(1, 1).Value = "Артикул"; ws.Cell(1, 2).Value = "Название"; ws.Cell(1, 3).Value = "Бренд"; ws.Cell(1, 4).Value = "Категория"; ws.Cell(1, 5).Value = "Цена"; ws.Cell(1, 6).Value = "Остаток";
                ws.Range(1, 1, 1, 6).Style.Font.Bold = true;
                int row = 2;
                foreach (var p in products)
                {
                    ws.Cell(row, 1).Value = p.Article ?? "";
                    ws.Cell(row, 2).Value = p.Name;
                    ws.Cell(row, 3).Value = p.Brands?.Name;
                    ws.Cell(row, 4).Value = p.Categories?.Name;
                    ws.Cell(row, 5).Value = p.Price;
                    ws.Cell(row, 6).Value = p.StockQuantity;
                    row++;
                }
                ws.Columns().AdjustToContents();
                string fileName = $"Products_{DateTime.Now:yyyyMMddHHmm}.xlsx";
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
                wb.SaveAs(path);
            }
        }

        public void ExportReportData(System.Collections.IEnumerable data, string fileName)
        {
            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("Report");
                var enumerator = data.GetEnumerator();
                if (!enumerator.MoveNext()) return;
                var first = enumerator.Current;
                var props = first.GetType().GetProperties();
                for (int i = 0; i < props.Length; i++)
                    ws.Cell(1, i + 1).Value = props[i].Name;
                int row = 2;
                foreach (var item in data)
                {
                    for (int i = 0; i < props.Length; i++)
                    {
                        var value = props[i].GetValue(item);
                        ws.Cell(row, i + 1).Value = value?.ToString();
                    }
                    row++;
                }
                ws.Columns().AdjustToContents();
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName + ".xlsx");
                wb.SaveAs(path);
            }
        }
    }
}