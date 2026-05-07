using System;
using System.Collections.Generic;
using System.Linq;
using AutoPartsStore.Models;

namespace AutoPartsStore.Services
{
    public class OrderService
    {
        public bool CreateOrder(int customerId, List<(Products product, int quantity)> items, int userId)
        {
            using (var db = new AutoPartsStoreDBEntities())
            {
                try
                {
                    var order = new Orders
                    {
                        CustomerId = customerId,
                        OrderDate = DateTime.Now,
                        StatusId = 1, // "Новый"
                        UserId = userId
                    };
                    db.Orders.Add(order);
                    db.SaveChanges();

                    decimal total = 0;
                    foreach (var item in items)
                    {
                        var product = db.Products.Find(item.product.ProductId);
                        if (product == null) throw new Exception("Товар не найден");
                        if (product.StockQuantity < item.quantity)
                            throw new Exception($"Недостаточно товара: {product.Name}");
                        product.StockQuantity -= item.quantity;

                        db.OrderItems.Add(new OrderItems
                        {
                            OrderId = order.OrderId,
                            ProductId = product.ProductId,
                            Quantity = item.quantity,
                            UnitPrice = product.Price
                        });
                        total += product.Price * item.quantity;
                    }
                    order.TotalAmount = total;
                    db.SaveChanges();

                    new LogService().Log(userId, "INSERT", "Orders", order.OrderId, $"Создан заказ №{order.OrderId}");
                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"OrderService error: {ex.Message}");
                    return false;
                }
            }
        }
    }
}