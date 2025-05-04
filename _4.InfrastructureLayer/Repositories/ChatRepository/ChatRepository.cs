using _5.DataAccessLayer_DAL_;
using DomainLayer_BusinessLogicLayer_.DomainModel;
using DomainLayer_BusinessLogicLayer_.InfraInterfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4.infrastructureLayer.Repositories.ChatRepository
{
    public class ChatRepository : IChatRepository
    {
        private readonly ApplicationChatDbContext _context;

        public ChatRepository(ApplicationChatDbContext context)
        {
            _context = context;
        }

        public async Task SaveMessageAsync(ChatMessageEntity message)
        {
            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ChatMessageEntity>> GetChatHistoryAsync(string userEmail1, string userEmail2, int skip = 0, int take = 20)
        {
            var messages = await _context.ChatMessages
                .Where(m => (m.FromEmail == userEmail1 && m.ToEmail == userEmail2)
                         || (m.FromEmail == userEmail2 && m.ToEmail == userEmail1))
                .OrderByDescending(m => m.Timestamp) // Nyaste först
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return messages.OrderBy(m => m.Timestamp).ToList(); // Återställ ordningen till äldst → nyast
        }


        public async Task<List<ChatMessageEntity>> GetUnreadMessagesAsync(string toEmail)
        {
            return await _context.ChatMessages
                .Where(m => m.ToEmail == toEmail && !m.IsRead)
                .ToListAsync();
        }

        public async Task MarkMessagesAsReadAsync(string fromEmail, string toEmail)
        {
            var messages = await _context.ChatMessages
                .Where(m => m.FromEmail == fromEmail && m.ToEmail == toEmail && !m.IsRead)
                .ToListAsync();

            foreach (var message in messages)
                message.IsRead = true;

            await _context.SaveChangesAsync();
        }
    }

}
