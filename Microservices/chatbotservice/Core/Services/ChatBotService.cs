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

        public async Task<FinalResponseDto> HandelQuery(AiRequestDto requestDto)
        {                        
            try
            {
                // Call Ai Service 
                // 1. Post Method To Create Request And To Get The Task Id  

                var responsemodel = new FinalResponseDto();
                var responseDto = await _aiClientService.SentRequestToAiService(requestDto);

                if (requestDto == null)
                {
                    responsemodel.Status = Status.FAILURE;
                    responsemodel.ResponseMessage = " The AI service is currently unavailable. Please try again later.";
                }

                // 2. Get Method To Get The Response By Using The Task Id

                var aiResponse = await CallGetMethod(responseDto.task_id);

                if (aiResponse.Status == Status.SUCCESS)
                {
                    // 2. Build entity
                    var entity = _mapper.Map<ChatMessage>(requestDto);

                    entity.Id = Guid.NewGuid().ToString();
                    entity.Response = aiResponse.ResponseMessage;
                    entity.TimeStamp = DateTime.UtcNow;
                    entity.Query = aiResponse.Query;


                    // 3. Save to database
                    var saved = await _unitOfWork.Entity.AddAsync(entity);
                    await _unitOfWork.Save();

                    // 4. Return DTO
                    return aiResponse;
                }
                else
                {
                    responsemodel.Status = Status.FAILURE;
                    responsemodel.ResponseMessage = " The AI service is currently unavailable. Please try again later.";
                    return responsemodel;
                }
            }
            catch (Exception)
            {
                var aiResponse = new FinalResponseDto();
                aiResponse.Status = Status.FAILURE;
                aiResponse.ResponseMessage = " The AI service is currently unavailable. Please try again later.";
                aiResponse.Query = requestDto.user_question;

                return aiResponse;
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
                        responseDto.ResponseMessage = "The AI service failed or was revoked.";
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
