using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Team.Core.DTOs;
using Team.Core.Result;

namespace Team.Core.Interfaces
{
    public interface IAuthGrpcService
    {
        Task<Result<List<UserJobTitleDto>>> GetTeamEmployeesJobTitlesAsync(List<string> userIds);
    }
}
