using DomainLayer_BusinessLogicLayer_.DomainModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer_BusinessLogicLayer_.InfraInterfaces
{
    public interface INotificationRepository
    {
        Task AddAsync(NotificationEntity notification);
        Task<List<NotificationEntity>> GetUnreadByUserAsync(string userId);
        Task MarkAsReadAsync(int notificationId);
        Task<int> GetUnreadCountAsync(string userId);

    }
}
