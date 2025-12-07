//using AutoMapper;
//using Core.Const;
//using Core.DTOs.AiService;
//using Core.DTOs.Chatbot;
//using Core.Interfaces;
//using Core.Models;
//using Core.Services;
//using Core.Services.Interface;
//using Moq;
//using Xunit;

//public class ChatBotServiceTests
//{
//    private readonly Mock<IUnitOfWork<ChatMessage>> _mockUnitOfWork;
//    private readonly Mock<IAiClientService> _mockAiClientService;
//    private readonly Mock<IMapper> _mockMapper;
//    private readonly ChatBotService _chatBotService;

//    public ChatBotServiceTests()
//    {
//        _mockUnitOfWork = new Mock<IUnitOfWork<ChatMessage>>();
//        _mockAiClientService = new Mock<IAiClientService>();
//        _mockMapper = new Mock<IMapper>();

//        _chatBotService = new ChatBotService(
//            _mockUnitOfWork.Object,
//            _mockAiClientService.Object,
//            _mockMapper.Object);
//    }

//    [Fact]
//    public async Task HandelQuery_SuccessfulPolling_ReturnsSuccessAndSavesToDb()
//    {
//        // Arrange
//        var requestDto = new AiRequestDto { user_question = "Test Question" };
//        var taskId = "task_123";
        
//        _mockAiClientService
//            .Setup(s => s.SentRequestToAiService(requestDto))
//            .ReturnsAsync(new AiResponseDto { task_id = taskId, Status = Status.STARTED });

        
//        var callCount = 0;

//        _mockAiClientService
//            .Setup(s => s.GetResponseFromAiService(taskId))
//            .ReturnsAsync(() =>
//            {
//                callCount++;
//                if (callCount < 3)
//                {
//                    // محاولات Polling فاشلة/قيد التنفيذ
//                    return new FinalResponseDto { Status = Status.PENDING };
//                }
//                else
//                {
//                    // محاولة ناجحة
//                    return new FinalResponseDto
//                    {
//                        Status = Status.SUCCESS,
//                        Response = new ResponseMessage {  Answer = "AI Answer", Sql_Query = "SELECT * FROM DB" }
//                    };
//                }
//            });

//        // 3. Mock Mapper
//        var expectedEntity = new ChatMessage { UserId = "user123" };
//        _mockMapper
//            .Setup(m => m.Map<ChatMessage>(requestDto))
//            .Returns(expectedEntity);

//        // Act
//        var result = await _chatBotService.SendQuryToAiService(requestDto);

//        // Assert
//        Assert.Equal(Status.SUCCESS, result.status);
//        Assert.Equal("AI Answer", result.ResponseMessage);
//    }

//    [Fact]
//    public async Task HandelQuery_PostRequestThrowsException_ReturnsFailure()
//    {
//        // Arrange
//        var requestDto = new AiRequestDto { user_question = "Failing Test" };

//        // Mock SentRequestToAiService لرمي HttpRequestException
//        _mockAiClientService
//            .Setup(s => s.SentRequestToAiService(requestDto))
//            .ThrowsAsync(new HttpRequestException("Connection Refused"));

//        // Act
//        var result = await _chatBotService.SendQuryToAiService(requestDto);

//        // Assert
//        Assert.Equal(Status.FAILURE, result.status);
//        Assert.Contains("Connection Refused", result.ResponseMessage);

//        // التحقق من أنه لم يتم محاولة الحفظ
//        _mockUnitOfWork.Verify(u => u.Save(), Times.Never);
//    }

//    [Fact]
//    public async Task HandelQuery_PostReturnsNullTaskId_ReturnsFailure()
//    {
//        // Arrange
//        var requestDto = new AiRequestDto { user_question = "Test No Task ID" };

//        _mockAiClientService
//            .Setup(s => s.SentRequestToAiService(requestDto))
//            .ReturnsAsync(new AiResponseDto { task_id = null }); // Task ID فارغ

//        // Act
//        var result = await _chatBotService.SendQuryToAiService(requestDto);

//        // Assert
//        Assert.Equal(Status.FAILURE, result.status);
//        Assert.Contains("returned an empty ID", result.ResponseMessage);

//        // التحقق من أنه لم يتم محاولة الـ Polling
//        _mockAiClientService.Verify(s => s.GetResponseFromAiService(It.IsAny<string>()), Times.Never);
//    }        
//    [Fact]
//    public async Task HandelQuery_PostRequestThrowsHttpRequestException_ReturnsFailure()
//    {
//        // Arrange
//        var requestDto = new AiRequestDto { user_question = "Test Connection Failure" };
//        var exceptionMessage = "The connection timed out.";

//        // Mock SentRequestToAiService لرمي HttpRequestException
//        _mockAiClientService
//            .Setup(s => s.SentRequestToAiService(requestDto))
//            .ThrowsAsync(new HttpRequestException(exceptionMessage));

//        // Act
//        var result = await _chatBotService.SendQuryToAiService(requestDto);

//        // Assert
//        Assert.Equal(Status.FAILURE, result.status);
//        // يجب أن تحتوي الرسالة على سبب فشل الاتصال
//        Assert.Contains(exceptionMessage, result.ResponseMessage);

//        // التحقق من أنه لم يتم إجراء Polling أو حفظ
//        _mockAiClientService.Verify(s => s.GetResponseFromAiService(It.IsAny<string>()), Times.Never);
//        _mockUnitOfWork.Verify(u => u.Save(), Times.Never);
//    }

//    //[Fact]
//    //public async Task HandelQuery_PollingReturnsPermanentFailure_ReturnsFailure()
//    //{
//    //    // Arrange
//    //    var requestDto = new AiRequestDto { user_question = "Test Polling Failure" };
//    //    var taskId = "task_789";

//    //    // 1. Mock POST: إرجاع Task ID بنجاح
//    //    _mockAiClientService
//    //        .Setup(s => s.SentRequestToAiService(requestDto))
//    //        .ReturnsAsync(new AiResponseDto { task_id = taskId, Status = Status.STARTED });

//    //    // 2. Mock GET Polling: الرد بالفشل فوراً
//    //    _mockAiClientService
//    //        .Setup(s => s.GetResponseFromAiService(taskId))
//    //        .ReturnsAsync(new FinalResponseDto
//    //        {
//    //            Status = Status.FAILURE,
//    //            Response = new FinalResponseDto.ResponseModel { Answer = "Internal AI Error" }
//    //        });

//    //    // 3. Mock Mapper لـ FinalResponseDto إلى LastResponseDto
//    //    _mockMapper
//    //        .Setup(m => m.Map<LastResponseDto>(It.IsAny<FinalResponseDto>()))
//    //        .Returns(new LastResponseDto { status = Status.FAILURE, ResponseMessage = "AI service failed to process the request." });

//    //    // Act
//    //    var result = await _chatBotService.HandelQuery(requestDto);

//    //    // Assert
//    //    Assert.Equal(Status.FAILURE, result.status);
//    //    Assert.Contains("AI service failed to process the request.", result.ResponseMessage);

//    //    // التحقق من أنه لم يتم محاولة الحفظ
//    //    _mockUnitOfWork.Verify(u => u.Save(), Times.Never);
//    //}
//}