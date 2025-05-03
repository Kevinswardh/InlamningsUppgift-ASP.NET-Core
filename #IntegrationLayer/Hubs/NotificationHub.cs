
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace _3.IntegrationLayer.Hubs
{
    public class NotificationHub : Hub
    {
        // Exempel: skicka en notifikation till en specifik användare
        public async Task SendNotification(string userId, string title, string message)
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", title, message);
        }
    }
}
