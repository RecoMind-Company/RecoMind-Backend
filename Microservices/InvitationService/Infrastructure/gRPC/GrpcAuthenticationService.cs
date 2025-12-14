using Core.DTOs.AuthenticationDtos;
using Core.Interfaces;
using Grpc.Core;
using Infrastructure.gRPC.Protos;
using Microsoft.Extensions.Logging;

namespace Infrastructure.gRPC
{
    public class GrpcAuthenticationService(AuthenticationService.AuthenticationServiceClient client, ILogger<GrpcAuthenticationService> logger) : IGrpcAuthenticationService
    {
        public async Task<AuthResponseDto> Register(string email, string role)
        {
            var request = new RegisterMessage { Email = email, Role = role };
            try
            {
                var response = await client.RegisterAsync(request);
                return new AuthResponseDto
                {
                    IsAuthenticated = response.IsAuthenticated,
                    Message = response.Message,
                    UserId = response.Id
                };
            }
            catch (RpcException ex)
            {
                logger.LogError("gRPC Register error: {Status} - {Detail}", ex.Status.StatusCode, ex.Status.Detail);
                return new AuthResponseDto
                {
                    Message = $"Authentication server error: {ex.Status.Detail}"
                };
            }
        }
    }
}
