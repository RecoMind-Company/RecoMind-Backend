
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Team.Core.DTOs;

namespace Team.Core.Interfaces
{
    public interface IGrpcAuthService
    {
        Task<UsersToReturnDto> GetUsersByIdsAsync(List<string> userIds);
        Task<UserToReturnDto> GetUserByIdAsync(string userId);
    }
}
