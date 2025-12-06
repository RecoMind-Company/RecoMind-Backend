using Team.Core.DTOs;
using Team.Core.Interfaces;
using Team.WebApi.AuthGrpc;

namespace Team.Infrastructure.Grpc
{
    public class GrpcAuthService(AuthenticationService.AuthenticationServiceClient authenticationServiceClient) : IGrpcAuthService
    {
        public async Task<UserToReturnDto> GetUserByIdAsync(string userId)
        {
            var request = new GetUserByIdMessage { UserId = userId };
            var response = await authenticationServiceClient.GetUserByIdAsync(request);
            return new UserToReturnDto
            {
                Id = response.Id,
                Role = response.Role,
                Email = response.Email
            };
        }

        public async Task<UsersToReturnDto> GetUsersByIdsAsync(List<string> userIds)
        {
            var request = new GetUsersMessage { Ids = { userIds } };
            var response = await authenticationServiceClient.GetUsersAsync(request);
            return new UsersToReturnDto { Usernames = response.UserNames.ToList() };
        }
    }
}
