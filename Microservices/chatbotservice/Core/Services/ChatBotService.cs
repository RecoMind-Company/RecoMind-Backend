using AutoMapper;
using Core.Const;
using Core.DTOs.AiService;
using Core.DTOs.Chatbot;
using Core.Interfaces;
using Core.Models;
using Core.Services.Interface;

namespace Core.Services
{
    public class ChatBotService : IChatBotService
    {
        private readonly IUnitOfWork<ChatMessage> _unitOfWork;
        private readonly IAiClientService _aiClientService;
        private readonly IMapper _mapper;

        public ChatBotService(IUnitOfWork<ChatMessage> unitOfWork,
            IAiClientService aiClientService,
            IMapper mapper)
        {
            _aiClientService = aiClientService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<AiResponseDto> SendQuryToAiService(AiRequestDto requestDto)
        {
            AiResponseDto postResponse;
            try
            {
                //1.Call Post Method
                postResponse = await _aiClientService.SentRequestToAiService(requestDto);
                return postResponse;
            }
            catch (Exception ex)
            {
                return new AiResponseDto
                {
                    status = Status.FAILURE,
                };
            }
        }

        public async Task<LastResponseDto> GetResponseFromAiService(GetMethodDto dto)
        {
            var aiResponse = await _aiClientService.GetResponseFromAiService(dto.TaskId);

            if (aiResponse.Status == Status.SUCCESS)
            {
                var entity = _mapper.Map<ChatMessage>(aiResponse);

                entity.Id = Guid.NewGuid().ToString();
                entity.Response.Answer = aiResponse.Result.Answer;
                entity.Response.Sql_Query = "this is query";
                entity.TimeStamp = DateTime.UtcNow;
                entity.UserQuestion = dto.user_question;
                entity.UserId = dto.UserID;
                entity.UserRole = dto.UserRole;

                await _unitOfWork.Entity.AddAsync(entity);
                await _unitOfWork.Save();

                // 5. Return DTO
                var aiResponseDto = _mapper.Map<LastResponseDto>(aiResponse);
                return aiResponseDto;
            }
            else
            {
                return new LastResponseDto
                {
                    status = aiResponse.Status,
                    ResponseMessage = " Pleas Try Again ",
                };
            }
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
    }
}

