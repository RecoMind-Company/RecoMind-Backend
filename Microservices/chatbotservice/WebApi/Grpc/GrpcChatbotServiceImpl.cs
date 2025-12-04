using AutoMapper;
using Core.DTOs;
using Core.Services.Interface;
using Core.Services.Protos;
using Grpc.Core;
using Microsoft.AspNetCore.Http.HttpResults;
using RecoMindAuthenticationAPI.Grpc.Authentication;

namespace WebApi.Grpc
{
    public class GrpcChatbotServiceImpl : GrpcChatbotService.GrpcChatbotServiceBase
    {
        private readonly IChatBotService _chatBotService;
        private readonly IMapper _mapper;
        private readonly RecoMindAuthenticationAPI.Grpc.Authentication.AuthenticationService.AuthenticationServiceClient _authenticationServiceClient;

        public GrpcChatbotServiceImpl(IChatBotService chatBotService , IMapper mapper, RecoMindAuthenticationAPI.Grpc.Authentication.AuthenticationService.AuthenticationServiceClient authenticationServiceClient)
        {
            _chatBotService = chatBotService;
            _mapper = mapper;
            _authenticationServiceClient = authenticationServiceClient;
        }

        public override async Task<ChatMessageResponse> HandelQuery(CreateChatRequest request, ServerCallContext context)
        {
            var dto = _mapper.Map<CreateChatRequestDto>(request);
            try
            {
                // Call The Authentication Service to validate the user 

                if (!string.IsNullOrEmpty(dto.UserID))
                {
                    var user = _authenticationServiceClient.GetUserById(new GetUserByIdMessage { UserId = dto.UserID });

                    if (user == null || !(user.Role.ToLower().Equals(dto.UserRole)))
                        throw new RpcException(new Status(StatusCode.InvalidArgument, $" Invalid UserId Or Role"));
                }

                // Call The ChatBot Service to process the query

                var result = await _chatBotService.HandelQuery(dto);
               
                return new ChatMessageResponse
                {
                    ResponseMessage = result.Response,
                };
            }
            catch (Exception ex)
            {
                return new ChatMessageResponse
                {
                    ResponseMessage = ex.Message,
                };
            }
        }

        public override async Task<GetHistoryResponse> GetHistory(GetHistoryRequest request, ServerCallContext context)
        {
            var result = await _chatBotService.GetHistory(request.UserID);
            var response = new GetHistoryResponse();
            foreach (var item in result)
            {
                var msg = _mapper.Map<chatHistory>(item);
                response.ChatHistory.Add(msg);
            }

            return response;
        }

        public override async Task <DeleteResponse> ClearHistory (GetHistoryRequest request, ServerCallContext context)
        {
            try 
            {
                await _chatBotService.DeleteHistory(request.UserID);
                return new DeleteResponse
                {
                    Message = " History Cleared Successfully"
                };
            }
            catch (Exception ex)
            {
                return new DeleteResponse
                {
                    Message = ex.Message
                };  
            }
        }
    }
}
