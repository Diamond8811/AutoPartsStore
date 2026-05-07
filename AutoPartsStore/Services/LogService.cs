using System;
using AutoPartsStore.Models;

namespace AutoPartsStore.Services
{
    public class LogService
    {
        public void Log(int userId, string actionType, string table, int? recordId, string description)
        {
            using (var db = new AutoPartsStoreDBEntities())
            {
                db.ActivityLog.Add(new ActivityLog
                {
                    UserId = userId,
                    ActionType = actionType,
                    TableName = table,
                    RecordId = recordId,
                    Description = description,
                    ActionDate = DateTime.Now
                });
                db.SaveChanges();
            }
        }
    }
}