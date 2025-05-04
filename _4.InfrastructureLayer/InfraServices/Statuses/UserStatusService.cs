using __Cross_cutting_Concerns.ServiceInterfaces;
using _IntegrationLayer.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace _4.infrastructureLayer.InfraServices.Statuses
{
    public class UserStatusService : IUserStatusService
    {
        private static readonly ConcurrentDictionary<string, bool> _statusMap = new();
        private readonly IHubContext<UserHub> _hubContext;

        public UserStatusService(IHubContext<UserHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public void SetOnline(string email)
        {
            _statusMap[email] = true;
            _hubContext.Clients.All.SendAsync("UserStatusChanged", email, true);
        }

        public void SetOffline(string email)
        {
            _statusMap[email] = false;
            _hubContext.Clients.All.SendAsync("UserStatusChanged", email, false);
        }

        public bool IsOnline(string email)
        {
            return _statusMap.TryGetValue(email, out var online) && online;
        }
    }
}
