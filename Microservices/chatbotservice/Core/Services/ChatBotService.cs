using AutoMapper;
using Core.DTOs;
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

        public async Task<ChatMessageResponseDto> HandelQuery(CreateChatRequestDto requestDto)
        {
            // Call Ai Service 
            var aiResponse = new ApiResponseDto();
            try
            {
                aiResponse = await _aiClientService.GetAiResponse(requestDto.Query);

                if (string.IsNullOrWhiteSpace(aiResponse.Message))
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                aiResponse.Success = false;
                aiResponse.Message = " The AI service is currently unavailable. Please try again later.";
                return _mapper.Map<ChatMessageResponseDto>(aiResponse);
            }


            // 2. Build entity
            var entity = _mapper.Map<ChatMessage>(requestDto);

            entity.Id = Guid.NewGuid().ToString();
            entity.Response = aiResponse.Message;
            entity.TimeStamp = DateTime.UtcNow;
            entity.Query = requestDto.Query;


            // 3. Save
            var saved = await _unitOfWork.Entity.AddAsync(entity);
            await _unitOfWork.Save();

            // 4. Return DTO
            return _mapper.Map<ChatMessageResponseDto>(saved);
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

        //public async Task<string> DeleteQuery( string queryId )
        //{
        //    var message = await _unitOfWork.Entity.GetByIdAsync( queryId );

        //    if (message == null)
        //    {
        //        throw new KeyNotFoundException(queryId);
        //    }

        //    var result =  _unitOfWork.Entity.Delete(message);
        //    await _unitOfWork.Save();

        //    return $"Query With Id {result.Id} Has Been Deleted Successfuly ";
        //}

        //public async Task<ChatMessageResponseDto> GetChatMessageById(string queryId)
        //{
        //    var message = await _unitOfWork.Entity.GetByIdAsync(queryId);

        //    if (message == null)
        //    {
        //        throw new KeyNotFoundException(queryId);
        //    }

        //    return _mapper.Map<ChatMessageResponseDto>(message);
        //}
    }
}
