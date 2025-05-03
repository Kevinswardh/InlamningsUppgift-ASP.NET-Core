using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using __Cross_cutting_Concerns.FormDTOs;

namespace ApplicationLayer_ServiceLayer_.NotificationManagment.NotificationService.Interface
{
    public interface INotificationService
    {
        Task AddNotificationAsync(NotificationForm notification);
        Task<List<NotificationForm>> GetUnreadByUserAsync(string userId);
        Task MarkAsReadAsync(int notificationId);
        Task<int> GetUnreadCountAsync(string userId);

    }
}
