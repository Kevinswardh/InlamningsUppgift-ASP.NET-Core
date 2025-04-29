using DomainLayer_BusinessLogicLayer_.DomainModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer_BusinessLogicLayer_.DomainModel
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StartDate { get; set; }
        public string CreatedByUserId { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();
    }

}
