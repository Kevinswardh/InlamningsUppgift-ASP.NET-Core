using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using __Cross_cutting_Concerns.FormDTOs;

namespace ApplicationLayer_ServiceLayer_.AdminPageManagment.Interface
{
    public interface IAdminService
    {
        Task<AdminStatisticsDTO> GetAdminStatisticsAsync();
    }

}
