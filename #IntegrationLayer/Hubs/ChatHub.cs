using __Cross_cutting_Concerns.CrossCutting.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

public class ChatHub : Hub
{
    private static readonly Dictionary<string, string> UserConnections = new();
    private readonly IChatService _chatService;

    public ChatHub(IChatService chatService)
    {
        _chatService = chatService;
    }

    public override Task OnConnectedAsync()
    {
        var email = Context.User?.FindFirst(ClaimTypes.Email)?.Value;
        if (!string.IsNullOrEmpty(email))
        {
            UserConnections[email] = Context.ConnectionId;
        }

        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var email = Context.User?.FindFirst(ClaimTypes.Email)?.Value;
        if (!string.IsNullOrEmpty(email))
        {
            UserConnections.Remove(email);
        }

        return base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(string toEmail, string message)
    {
        var fromEmail = Context.User?.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrWhiteSpace(toEmail) || string.IsNullOrWhiteSpace(message) || string.IsNullOrWhiteSpace(fromEmail))
            return;

        await _chatService.SaveMessageAsync(fromEmail, toEmail, message);

        if (UserConnections.TryGetValue(toEmail, out var receiverConnId))
        {
            await Clients.Client(receiverConnId).SendAsync("ReceiveMessage", fromEmail, message);
        }

        await Clients.Caller.SendAsync("ReceiveMessage", fromEmail, message);
    }

    public async Task LoadChatHistory(string withEmail, int skip = 0)
    {
        var currentUser = Context.User?.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(currentUser) || string.IsNullOrEmpty(withEmail)) return;

        await _chatService.MarkAsReadAsync(withEmail, currentUser);

        var history = await _chatService.GetChatHistoryAsync(currentUser, withEmail, skip, 20); // paginate

        var messages = history.Select(m => new
        {
            fromEmail = m.FromEmail,
            content = m.Message
        }).ToList();

        await Clients.Caller.SendAsync("LoadChatHistory", messages, skip > 0);
    }
}
