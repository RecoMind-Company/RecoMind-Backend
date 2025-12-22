using AutoMapper;
using Core.Const;
using Core.DTOs.AiService;
using Core.DTOs.Chatbot;
using Core.Interfaces;
using Core.Models;
using Core.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Core.Services
{
    //[Authorize]
    public class ChatBotService : IChatBotService
    {
        private readonly IUnitOfWork<ChatMessage> _unitOfWork;
        private readonly IAiClientService _aiClientService;
        private readonly IMapper _mapper;
        private readonly ITeamService _teamService;

        public ChatBotService(IUnitOfWork<ChatMessage> unitOfWork,
            IAiClientService aiClientService,
            IMapper mapper,
            ITeamService teamService)
        {
            _aiClientService = aiClientService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _teamService = teamService;
        }

        public async Task<AiResponseDto> HandelRequestBeforeBassingItToAiService(CreateChatRequestDto requestDto)
        {
            try
            {
                // call team service to get team name and company id
                var teaminfo =await _teamService.GetTeamInformation(requestDto.UserID);

                // create AiRequestDto to send to Ai service
                AiRequestDto aiRequestDto = new AiRequestDto
                {
                    company_id = teaminfo.CompanyId,
                    team_name = teaminfo.TeamName,
                    user_question = requestDto.user_question,
                };
                //1.Call Post Method
                AiResponseDto postResponse;

                postResponse = await _aiClientService.SentRequestToAiService(aiRequestDto);
                return postResponse;
            }
            catch (Exception ex)
            {
                return new AiResponseDto
                {
                    status = Status.FAILURE,
                    message = ex.Message
                };
            }            
        }

        public async Task<FinalResponseDto> GetResponseFromAiService(GetMethodDto dto)
        {
            var aiResponse = await _aiClientService.GetResponseFromAiService(dto.TaskId);

            return aiResponse;
        }

        public async Task<IEnumerable<GetHistoryDto>> GetHistory(string userId)
        {
            var messages = await _unitOfWork.Entity.FindAll(m => m.UserId == userId);

            if (messages == null || !messages.Any())
            {
                throw new KeyNotFoundException($"User With Id : {userId} Not Found ");
            }

            var responseDtos = _mapper.Map<IEnumerable<GetHistoryDto>>(messages);

            return responseDtos;
        }

        public async Task DeleteHistory(string userId)
        {
            var messages = await _unitOfWork.Entity.FindAll(x => x.UserId == userId);

            if (messages.Any())
            {
                foreach (var message in messages)
                {
                    _unitOfWork.Entity.Delete(message);
                }
                await _unitOfWork.Save();
                return;
            }
            throw new KeyNotFoundException($"User With Id : {userId} Has No Operations Or History ");
        }

        public async Task SaveToDatabase(SaveDto model)
        {
            var charmessage = _mapper.Map<ChatMessage>(model);
            await _unitOfWork.Entity.AddAsync(charmessage);
            await _unitOfWork.Save();
        }
    }
}

