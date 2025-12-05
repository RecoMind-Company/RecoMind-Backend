using AutoMapper;
using Core.Const;
using Core.DTOs.AiService;
using Core.DTOs.Chatbot;
using Core.Interfaces;
using Core.Models;
using Core.Services.Interface;
using Microsoft.Extensions.Options;
using System.Threading;

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

        public async Task<LastResponseDto> HandelQuery(AiRequestDto requestDto)
        {
            AiResponseDto postResponse;
            try
            {
                //1.Call Post Method
                postResponse = await _aiClientService.SentRequestToAiService(requestDto);
            }
            catch (HttpRequestException ex)
            {
                return new LastResponseDto
                {
                    status = Status.FAILURE,
                    ResponseMessage = $"AI service connection failed: {ex.Message}",                    
                };
            }
            
            if (postResponse == null || string.IsNullOrWhiteSpace(postResponse.task_id))
            {                
                return new LastResponseDto
                {
                    status = Status.FAILURE,
                    ResponseMessage = "AI service failed to start the task or returned an empty ID.",
                };
            }

            // 3. Get Method (Poling)
            var aiResponse = await CallGetMethod(postResponse.task_id);

            if (aiResponse.Status == Status.SUCCESS)
            {
                //  Database
                var entity = _mapper.Map<ChatMessage>(requestDto);

                
                
                entity.Id = Guid.NewGuid().ToString();
                entity.Response.Answer = aiResponse.Response.Answer;
                entity.Response.Sql_Query = aiResponse.Response.Sql_Query;
                entity.TimeStamp = DateTime.UtcNow;               
                entity.Query = requestDto.user_question;

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
                    status = Status.FAILURE,
                    ResponseMessage = "AI service failed to process the request.",
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

        public async Task<FinalResponseDto> CallGetMethod(string taskId)
        {
            while (true)
            {
                var responseDto = await _aiClientService.GetResponseFromAiService(taskId);

                switch (responseDto.Status)
                {
                    case Status.SUCCESS:                        
                        return responseDto;

                    case Status.FAILURE:
                    case Status.REVOKED: 
                        responseDto.Status = Status.FAILURE;
                        responseDto.Response.Answer = "The AI service failed or was revoked.";
                        return responseDto;

                    case Status.PENDING:
                    case Status.STARTED:
                    case Status.RETRY:
                       
                        await Task.Delay(TimeSpan.FromSeconds(10));                       
                        break;

                    default:
                        throw new Exception("An unexpected status occurred from the AI service.");
                }
            }
        }       
    }
}
