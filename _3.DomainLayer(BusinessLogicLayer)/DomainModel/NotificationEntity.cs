using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer_BusinessLogicLayer_.DomainModel
{
    public class NotificationEntity
    {
        public int Id { get; set; }
        public string Title { get; set; } // Ex: "Project updated"
        public string Message { get; set; } // Ex: "Project X has been updated."
        public string ReceiverUserId { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
