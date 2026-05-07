using System;
using System.Collections.Generic;
using System.Linq;
using AutoPartsStore.Models;

namespace AutoPartsStore.Services
{
    public class ReturnService
    {
        public bool CreateReturn(int orderId, int reasonId, List<(OrderItems item, int quantity)> items, int userId)
        {
            using (var db = new AutoPartsStoreDBEntities())
            {
                try
                {
                    var returnEntity = new Returns
                    {
                        OrderId = orderId,
                        ReturnDate = DateTime.Now,
                        ReasonId = reasonId,
                        UserId = userId
                    };
                    db.Returns.Add(returnEntity);
                    db.SaveChanges();

                    decimal totalRefund = 0;
                    foreach (var entry in items)
                    {
                        if (entry.quantity > entry.item.Quantity)
                            throw new Exception("Превышено количество возвращаемого товара");
                        db.ReturnItems.Add(new ReturnItems
                        {
                            ReturnId = returnEntity.ReturnId,
                            OrderItemId = entry.item.OrderItemId,
                            Quantity = entry.quantity,
                            UnitPrice = entry.item.UnitPrice
                        });
                        totalRefund += entry.item.UnitPrice * entry.quantity;
                    }
                    returnEntity.RefundAmount = totalRefund;
                    db.SaveChanges();

                    new LogService().Log(userId, "INSERT", "Returns", returnEntity.ReturnId, $"Создан возврат №{returnEntity.ReturnId}");
                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ReturnService error: {ex.Message}");
                    return false;
                }
            }
        }
    }
}