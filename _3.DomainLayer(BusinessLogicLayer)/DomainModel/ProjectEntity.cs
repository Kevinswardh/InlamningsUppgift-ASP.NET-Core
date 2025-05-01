using DomainLayer_BusinessLogicLayer_.DomainModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer_BusinessLogicLayer_.DomainModel
{
    public class ProjectEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Budget { get; set; }
        public bool IsCompleted { get; set; } = false;
        public string CreatedByUserId { get; set; } // 🧍 Primärt för ägarskap

        public int CustomerId { get; set; }              // FK
        public CustomerEntity Customer { get; set; }     // Navigation

        public ICollection<ProjectMemberEntity> ProjectMembers { get; set; } = new List<ProjectMemberEntity>(); // ✅
    }

}
