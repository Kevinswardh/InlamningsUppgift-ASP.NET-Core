using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace __Cross_cutting_Concerns.FormDTOs
{
    public class ProjectForm
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string ClientEmail { get; set; } // 📨 används för att hitta kunden

        public string Description { get; set; }

        public string? ImageUrl { get; set; } // ✅ användas efter uppladdning

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public List<MemberItemDTO> Members { get; set; } = new();

        public decimal Budget { get; set; }

        public bool IsCompleted { get; set; } = false;

        public string CreatedByUserId { get; set; } // ✅ NYTT: sparas till databasen
    }
}
