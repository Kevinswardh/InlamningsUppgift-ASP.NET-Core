using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace __Cross_cutting_Concerns.FormDTOs
{
    public class ProjectForm
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ClientName { get; set; }
        public string Description { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime EndDate { get; set; }
        public List<MemberItemDTO> Members { get; set; } = new();
        public bool IsCompleted { get; set; }
    }
}
