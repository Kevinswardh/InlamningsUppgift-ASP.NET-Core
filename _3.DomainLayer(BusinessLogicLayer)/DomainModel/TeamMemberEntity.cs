using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer_BusinessLogicLayer_.DomainModel
{
    public class TeamMemberEntity
    {
        public int Id { get; set; }         // intern ID
        public string ExternalUserId { get; set; } // t.ex. IdentityUserId
        public string Name { get; set; }
        public string Email { get; set; }
        public ICollection<ProjectMemberEntity> ProjectMembers { get; set; } = new List<ProjectMemberEntity>();

    }


}
