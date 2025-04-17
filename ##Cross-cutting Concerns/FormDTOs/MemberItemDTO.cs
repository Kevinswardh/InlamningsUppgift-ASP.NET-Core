using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace __Cross_cutting_Concerns.FormDTOs
{
    public class MemberItemDTO
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Position { get; set; }
        public string Role { get; set; }
        public bool IsOnline { get; set; }
    }
}
