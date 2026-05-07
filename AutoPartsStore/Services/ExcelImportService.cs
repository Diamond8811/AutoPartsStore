using AutoPartsStore.Models;
using ClosedXML.Excel;
using System;
using System.Linq;

namespace AutoPartsStore.Services
{
    public class ExcelImportService
    {
        public (int added, int updated) ImportProducts(string filePath, int userId)
        {
            int added = 0, updated = 0;
            using (var db = new AutoPartsStoreDBEntities())
            using (var workbook = new XLWorkbook(filePath))
            {
                var worksheet = workbook.Worksheets.First();
                var rows = worksheet.RangeUsed().RowsUsed().Skip(1);
                foreach (var row in rows)
                {
                    try
                    {
                        string article = row.Cell(1).GetString().Trim();
                        string name = row.Cell(2).GetString().Trim();
                        string description = row.Cell(3).GetString().Trim();
                        decimal price = row.Cell(4).GetValue<decimal>();
                        int quantity = row.Cell(5).GetValue<int>();
                        string brandName = row.Cell(6).GetString().Trim();
                        string categoryName = row.Cell(7).GetString().Trim();

                        if (string.IsNullOrWhiteSpace(article)) continue;

                        var brand = db.Brands.FirstOrDefault(b => b.Name == brandName);
                        if (brand == null) { brand = new Brands { Name = brandName }; db.Brands.Add(brand); db.SaveChanges(); }
                        var category = db.Categories.FirstOrDefault(c => c.Name == categoryName);
                        if (category == null) { category = new Categories { Name = categoryName }; db.Categories.Add(category); db.SaveChanges(); }

                        var existing = db.Products.FirstOrDefault(p => p.Article == article);
                        if (existing != null)
                        {
                            existing.Price = price;
                            existing.StockQuantity += quantity;
                            existing.BrandId = brand.BrandId;
                            existing.CategoryId = category.CategoryId;
                            updated++;
                        }
                        else
                        {
                            db.Products.Add(new Products
                            {
                                Article = article,
                                Name = name,
                                Description = description,
                                Price = price,
                                StockQuantity = quantity,
                                BrandId = brand.BrandId,
                                CategoryId = category.CategoryId
                            });
                            added++;
                        }
                    }
                    catch { continue; }
                }
                db.SaveChanges();
                new LogService().Log(userId, "IMPORT", "Products", null, $"Импорт: +{added} / обновлено {updated}");
            }
            return (added, updated);
        }
    }
}