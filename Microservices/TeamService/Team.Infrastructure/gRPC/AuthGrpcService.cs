using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Team.Core.DTOs;
using Team.Core.Interfaces;
using Team.Core.Result;

namespace Team.Infrastructure.gRPC
{
    public class AuthGrpcService : IAuthGrpcService
    {
        private readonly AccountService.AccountServiceClient _client;
        public AuthGrpcService(AccountService.AccountServiceClient client) => _client = client;


        public async Task<Result<List<UserJobTitleDto>>> GetTeamEmployeesJobTitlesAsync(List<string> userIds)
        {
            if(userIds == null || !userIds.Any())
                return new List<UserJobTitleDto>();

            var request = new GetJobTitlesListRequest();
            request.Ids.AddRange(userIds);

            try
            {
                var response = await _client.GetJobTitlesListAsync(request);

                if (response?.JobTitlesList == null) return new List<UserJobTitleDto>();

                return response.JobTitlesList.Select(x => new UserJobTitleDto
                {
                    UserId = x.UserId,
                    JobTitle = x.JobTitle
                }).ToList();
            }
            catch (RpcException ex)
            {
                return Result<List<UserJobTitleDto>>.Failure(new Error(ex.Status.Detail, "AuthService.Error"));
            }
        }
    }
}
