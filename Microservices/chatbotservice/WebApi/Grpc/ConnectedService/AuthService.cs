using Core.DTOs.Chatbot;
using RecoMindAuthenticationAPI.Grpc.Authentication;
namespace WebApi.Grpc.ConnectedService
{
    public class AuthService
    {
        private readonly AuthenticationService.AuthenticationServiceClient _authServiceClient;

        public AuthService(AuthenticationService.AuthenticationServiceClient authServiceClient)
        {
            _authServiceClient = authServiceClient;
        }

        public async Task<bool> CheckValidUser(CreateChatRequestDto Dto)
        {
            var user = _authServiceClient.GetUserById(new GetUserByIdMessage { UserId = Dto.UserID });

            if (user == null || !(user.Role.ToLower().Equals(Dto.UserRole)))
                throw new KeyNotFoundException($" Invalid UserId Or Role");

            return true;
        }
    }
}
