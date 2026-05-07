using System;
using System.Collections.Generic;
using System.Linq;
using AutoPartsStore.Models;

namespace AutoPartsStore.Services
{
    public class StockService
    {
        public bool CreateStockReceiving(List<(Products product, int quantity, decimal price)> items, int supplierId, int userId)
        {
            using (var db = new AutoPartsStoreDBEntities())
            {
                try
                {
                    var receiving = new StockReceivings
                    {
                        SupplierId = supplierId,
                        ReceivingDate = DateTime.Now,
                        UserId = userId
                    };
                    db.StockReceivings.Add(receiving);
                    db.SaveChanges();

                    foreach (var item in items)
                    {
                        var product = db.Products.Find(item.product.ProductId);
                        if (product == null) throw new Exception("Товар не найден");
                        product.StockQuantity += item.quantity;
                        db.StockReceivingItems.Add(new StockReceivingItems
                        {
                            ReceivingId = receiving.ReceivingId,
                            ProductId = product.ProductId,
                            Quantity = item.quantity,
                            UnitPrice = item.price
                        });
                    }
                    db.SaveChanges();

                    new LogService().Log(userId, "INSERT", "StockReceivings", receiving.ReceivingId, $"Создано поступление №{receiving.ReceivingId}");
                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"StockService error: {ex.Message}");
                    return false;
                }
            }
        }
    }
}