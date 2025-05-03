using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace __Cross_cutting_Concerns.FormDTOs
{
    public class NotificationForm
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string ReceiverUserId { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
