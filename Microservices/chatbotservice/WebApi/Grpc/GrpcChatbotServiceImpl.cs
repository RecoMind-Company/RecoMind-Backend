using AutoMapper;
using Core.DTOs.AiService;
using Core.DTOs.Chatbot;
using Core.Services.Interface;
using Core.Services.Protos;
using Grpc.Core;
using Microsoft.AspNetCore.Http.HttpResults;
using RecoMindAuthenticationAPI.Grpc.Authentication;
using WebApi.Grpc.ConnectedService;

namespace WebApi.Grpc
{
    public class GrpcChatbotServiceImpl : GrpcChatbotService.GrpcChatbotServiceBase
    {
        private readonly IChatBotService _chatBotService;
        private readonly IMapper _mapper;
        private readonly AuthService _authService;
        private readonly TeamService _teamService;
        public GrpcChatbotServiceImpl(IChatBotService chatBotService , IMapper mapper, AuthService authService , TeamService teamService)
        {
            _chatBotService = chatBotService;
            _mapper = mapper;
            _authService = authService;
            _teamService = teamService;
        }

        public override async Task<ChatMessageResponse> HandelQuery(CreateChatRequest request, ServerCallContext context)
        {            
            try
            {
                var dto = _mapper.Map<CreateChatRequestDto>(request);

                if (!(await _authService.CheckValidUser(dto) && !(await _teamService.CheckValidTeam(dto.UserID))))
                    throw new ArgumentException("Request body Has Invalid Data ");

                var team = await _teamService.GetTeamByUserId(dto.UserID);

                var Dto = new AiRequestDto
                {
                    compnay_id = team.CompanyId,
                    team_name = team.TeamName,
                    user_question = dto.Query
                };

                var result = await _chatBotService.HandelQuery(Dto);
                return _mapper.Map<ChatMessageResponse>(result);
            }
            catch (KeyNotFoundException knfEx)
            {
                return new ChatMessageResponse
                {
                   ResponseMessage= knfEx.Message                   
                };
            }
            catch (Exception ex)
            {
               return new ChatMessageResponse
               {
                    ResponseMessage = ex.Message
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
