using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Entities;
using WebApi.Helpers;

namespace WebApi.Services
{
    public interface INotificationService {
        Task Add(Notification notification);
        Task<List<Notification>> Get(string userId);
    }
    public class NotificationService : INotificationService
    {
        private readonly IMongoCollection<Notification> _notifications;
        public NotificationService(IAccommodDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _notifications = Util.GetCollection<Notification>(database, settings.NotificationsCollectionName);
        }

        public async Task Add(Notification notification)
        {
            _notifications.InsertOne(notification);
        }

        public async Task<List<Notification>> Get(string userId)
        {
            var filter = Builders<Notification>.Filter.Where(n => n.UserId == userId);
            var noti = (await _notifications.FindAsync(filter)).ToList();
            await _notifications.DeleteManyAsync(filter);
            return noti;
        }
    }
}
