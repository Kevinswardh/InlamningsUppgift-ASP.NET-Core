using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer_BusinessLogicLayer_.DomainModel
{
    public class ProjectMemberEntity
    {
        public int ProjectId { get; set; }
        public ProjectEntity Project { get; set; }

        public int TeamMemberId { get; set; }
        public TeamMemberEntity TeamMember { get; set; }
    }
}
