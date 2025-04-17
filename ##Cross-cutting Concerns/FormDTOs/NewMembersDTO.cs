using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace __Cross_cutting_Concerns.FormDTOs
{
    public class NewMembersDTO
    {
        public List<MemberItemDTO> Members { get; set; }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public string SearchQuery { get; set; }
        public string Filter { get; set; }
    }
}
