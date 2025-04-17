using __Cross_cutting_Concerns.ServiceInterfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4.infrastructureLayer.InfraServices.Statuses
{
    public class UserStatusService : IUserStatusService
    {
        private static readonly ConcurrentDictionary<string, bool> _statusMap = new();

        public void SetOnline(string email)
        {
            _statusMap[email] = true;
        }

        public void SetOffline(string email)
        {
            _statusMap[email] = false;
        }

        public bool IsOnline(string email)
        {
            return _statusMap.TryGetValue(email, out var online) && online;
        }
    }
}
