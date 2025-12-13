using Core.DTOs.TeamService;
using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository
{
    public class TeamService : ITeamService
    {
        public Task<GetTeamInformationDto> GetTeamInformation(string userId)
        {
            throw new NotImplementedException();
        }
    }
}
