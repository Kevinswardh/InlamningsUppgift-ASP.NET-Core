using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer_BusinessLogicLayer_.DomainModel
{
    public class CustomerEntity
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }

        public ICollection<ProjectEntity> Projects { get; set; } = new List<ProjectEntity>();
    }

}
