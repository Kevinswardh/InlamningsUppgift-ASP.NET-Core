using _5.DataAccessLayer_DAL_;
using DomainLayer_BusinessLogicLayer_.DomainModel;
using DomainLayer_BusinessLogicLayer_.InfraInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace _4.infrastructureLayer.Repositories.NotificationRepository
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly ApplicationProjectDbContext _context;

        public NotificationRepository(ApplicationProjectDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(NotificationEntity notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<List<NotificationEntity>> GetUnreadByUserAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.ReceiverUserId == userId && !n.IsRead)
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }
        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.ReceiverUserId == userId && !n.IsRead)
                .CountAsync();
        }

    }
}
