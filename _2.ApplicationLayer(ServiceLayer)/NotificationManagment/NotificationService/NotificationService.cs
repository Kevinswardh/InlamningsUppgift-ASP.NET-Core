using ApplicationLayer_ServiceLayer_.NotificationManagment.NotificationService.Interface;
using __Cross_cutting_Concerns.FormDTOs;
using DomainLayer_BusinessLogicLayer_.DomainModel;
using DomainLayer_BusinessLogicLayer_.InfraInterfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApplicationLayer.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task AddNotificationAsync(NotificationForm dto)
        {
            var entity = new NotificationEntity
            {
                Title = dto.Title,
                Message = dto.Message,
                ReceiverUserId = dto.ReceiverUserId,
                CreatedAt = dto.CreatedAt,
                IsRead = dto.IsRead
            };

            await _notificationRepository.AddAsync(entity);
        }

        public async Task<List<NotificationForm>> GetUnreadByUserAsync(string userId)
        {
            var result = await _notificationRepository.GetUnreadByUserAsync(userId);

            return result.Select(n => new NotificationForm
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                ReceiverUserId = n.ReceiverUserId,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            }).ToList();
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            await _notificationRepository.MarkAsReadAsync(notificationId);
        }
        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _notificationRepository.GetUnreadCountAsync(userId);
        }

    }
}
