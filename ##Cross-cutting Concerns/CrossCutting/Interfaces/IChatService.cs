using __Cross_cutting_Concerns.FormDTOs;

namespace __Cross_cutting_Concerns.CrossCutting.Interfaces
{
    public interface IChatService
    {
        Task SaveMessageAsync(string from, string to, string message);
        Task<List<ChatMessageDTO>> GetChatHistoryAsync(string user1, string user2, int skip = 0, int take = 20);
        Task MarkAsReadAsync(string from, string to);
    }
}
