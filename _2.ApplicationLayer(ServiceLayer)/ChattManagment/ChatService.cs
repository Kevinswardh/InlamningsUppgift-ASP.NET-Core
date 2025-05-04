using DomainLayer_BusinessLogicLayer_.DomainModel;
using DomainLayer_BusinessLogicLayer_.InfraInterfaces;

using __Cross_cutting_Concerns.CrossCutting.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using __Cross_cutting_Concerns.FormDTOs;

namespace ApplicationLayer_ServiceLayer_.ChattManagment
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;

        public ChatService(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }

        public async Task SaveMessageAsync(string from, string to, string message)
        {
            var msg = new ChatMessageEntity
            {
                FromEmail = from,
                ToEmail = to,
                Message = message,
                Timestamp = DateTime.UtcNow,
                IsRead = false
            };

            await _chatRepository.SaveMessageAsync(msg);
        }

        public async Task<List<ChatMessageDTO>> GetChatHistoryAsync(string user1, string user2, int skip = 0, int take = 20)
        {
            var history = await _chatRepository.GetChatHistoryAsync(user1, user2, skip, take);

            return history.Select(m => new ChatMessageDTO
            {
                FromEmail = m.FromEmail,
                ToEmail = m.ToEmail,
                Message = m.Message,
                Timestamp = m.Timestamp,
                IsRead = m.IsRead
            }).ToList();
        }


        public Task MarkAsReadAsync(string from, string to)
            => _chatRepository.MarkMessagesAsReadAsync(from, to);
    }
}
