using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer_BusinessLogicLayer_.DomainModel
{
    public class ProjectMemberEntity
    {
        public int ProjectId { get; set; } // ⬅️ måste matcha ProjectEntity.Id
        public ProjectEntity Project { get; set; }

        public string TeamMemberId { get; set; } // ⬅️ matchar TeamMemberEntity.Id
        public TeamMemberEntity TeamMember { get; set; }
    }

}
