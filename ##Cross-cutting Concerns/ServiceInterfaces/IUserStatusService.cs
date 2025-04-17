using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace __Cross_cutting_Concerns.ServiceInterfaces
{
    public interface IUserStatusService
    {
        void SetOnline(string email);
        void SetOffline(string email);
        bool IsOnline(string email);
    }
}
