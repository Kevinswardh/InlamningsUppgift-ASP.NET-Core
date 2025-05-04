using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace _IntegrationLayer.Hubs
{
    public class UserHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine($"🔌 SignalR ansluten: {userId ?? "ingen användare"}");
            await base.OnConnectedAsync();
        }

        public async Task RefreshClaims(string userId)
        {
            await Clients.User(userId).SendAsync("RefreshClaims");
        }
    }
}
