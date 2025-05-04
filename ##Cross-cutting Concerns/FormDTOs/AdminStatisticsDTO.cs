using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace __Cross_cutting_Concerns.FormDTOs
{
    public class AdminStatisticsDTO
    {
        public int TotalUsers { get; set; }
        public int Managers { get; set; }
        public int TeamMembers { get; set; }
        public int Customers { get; set; }
        public int NewMembers { get; set; }

        public int Projects { get; set; }
        public int CompletedProjects { get; set; }
        public int ActiveProjects { get; set; }

        public decimal TotalBudget { get; set; }
    }

}
