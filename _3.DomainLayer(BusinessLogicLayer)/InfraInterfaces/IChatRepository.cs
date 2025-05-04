using DomainLayer_BusinessLogicLayer_.DomainModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer_BusinessLogicLayer_.InfraInterfaces
{
    public interface IChatRepository
    {
        Task SaveMessageAsync(ChatMessageEntity message);
        Task<List<ChatMessageEntity>> GetChatHistoryAsync(string userEmail1, string userEmail2, int skip = 0, int take = 20);
        Task MarkMessagesAsReadAsync(string fromEmail, string toEmail);
    }

}
