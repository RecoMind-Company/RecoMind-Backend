using Core.DTOs.TeamService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface ITeamService
    {
        GetTeamInformationDto GetTeamInformation(string userId);
    }
}
