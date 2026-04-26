using AutoMapper;
using Core.Const;
using Core.DTOs.AiService;
using Core.DTOs.Chatbot;
using Core.DTOs.TeamService;
using Core.Interfaces;
using Core.Models;
using Core.Services;
using Core.Services.Interface;
using FluentAssertions;
using Moq;
using System.Linq.Expressions;
using Xunit;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Tests.ChatBotServiceTests
{
    public class ChatBotServiceTests
    {
        private readonly Mock<IUnitOfWork<ChatMessage>> _uowMock;
        private readonly Mock<IGenericRepository<ChatMessage>> _repoMock;
        private readonly Mock<IAiClientService> _aiClientMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ITeamService> _teamServiceMock;
        private readonly ChatBotService _service;

        public ChatBotServiceTests()
        {
            _uowMock = new Mock<IUnitOfWork<ChatMessage>>();
            _repoMock = new Mock<IGenericRepository<ChatMessage>>();
            _aiClientMock = new Mock<IAiClientService>();
            _mapperMock = new Mock<IMapper>();
            _teamServiceMock = new Mock<ITeamService>();

            // ربط الـ Repository بالـ UnitOfWork Mock
            _uowMock.Setup(u => u.Entity).Returns(_repoMock.Object);

            _service = new ChatBotService(
                _uowMock.Object,
                _aiClientMock.Object,
                _mapperMock.Object,
                _teamServiceMock.Object);
        }

        #region HandleRequest (AI Logic) Tests
        [Fact]
        public async Task HandleRequest_Success_ReturnsAiResponse()
        {
            // Arrange
            var requestDto = new CreateChatRequestDto { UserID = "user123", user_question = "What is AI?" };
            var teamInfo = new GetTeamInformationDto { CompanyId = "comp-001", TeamName = "DevTeam" };
            var aiResponse = new AiResponseDto { status = Status.SUCCESS, message = "Processing" };

            _teamServiceMock.Setup(s => s.GetTeamInformation(requestDto.UserID))
                            .ReturnsAsync(teamInfo);

            _aiClientMock.Setup(s => s.SentRequestToAiService(It.IsAny<AiRequestDto>()))
                         .ReturnsAsync(aiResponse);

            // Act
            var result = await _service.HandelRequestBeforeBassingItToAiService(requestDto);

            // Assert
            result.Should().NotBeNull();
            result.status.Should().Be(Status.SUCCESS);
            _aiClientMock.Verify(s => s.SentRequestToAiService(It.Is<AiRequestDto>(r =>
                r.company_id == "comp-001" && r.team_name == "DevTeam")), Times.Once);
        }

        [Fact]
        public async Task HandleRequest_OnException_ReturnsFailureDto()
        {
            // Arrange
            _teamServiceMock.Setup(s => s.GetTeamInformation(It.IsAny<string>()))
                            .ThrowsAsync(new Exception("Internal Server Error"));

            // Act
            var result = await _service.HandelRequestBeforeBassingItToAiService(new CreateChatRequestDto());

            // Assert
            result.status.Should().Be(Status.FAILURE);
            result.message.Should().Be("Internal Server Error");
        }
        #endregion

        #region History Logic Tests
        [Fact]
        public async Task GetHistory_MessagesExist_ReturnsMappedDtos()
        {
            // Arrange
            var userId = "user1";
            var messages = new List<ChatMessage> { new ChatMessage { UserId = userId, UserQuestion = "Hi" } };
            var expectedDtos = new List<GetHistoryDto> { new GetHistoryDto { Query = "Hi" } };

            _repoMock.Setup(r => r.FindAll(It.IsAny<Expression<Func<ChatMessage, bool>>>()))
                     .ReturnsAsync(messages);

            _mapperMock.Setup(m => m.Map<IEnumerable<GetHistoryDto>>(messages))
                       .Returns(expectedDtos);

            // Act
            var result = await _service.GetHistory(userId);

            // Assert
            result.Should().HaveCount(1);
            result.First().Query.Should().Be("Hi");
        }

        [Fact]
        public async Task GetHistory_NoMessages_ThrowsKeyNotFoundException()
        {
            // Arrange
            _repoMock.Setup(r => r.FindAll(It.IsAny<Expression<Func<ChatMessage, bool>>>()))
                     .ReturnsAsync(new List<ChatMessage>()); // قائمة فاضية

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetHistory("unknown"));
        }

        [Fact]
        public async Task DeleteHistory_MessagesExist_CallsDeleteForEachAndSaves()
        {
            // Arrange
            var userId = "user1";
            var messages = new List<ChatMessage>
            {
                new ChatMessage { Id = "1", UserId = userId },
                new ChatMessage { Id = "2", UserId = userId }
            };

            _repoMock.Setup(r => r.FindAll(It.IsAny<Expression<Func<ChatMessage, bool>>>()))
                     .ReturnsAsync(messages);

            // Act
            await _service.DeleteHistory(userId);

            // Assert
            _repoMock.Verify(r => r.Delete(It.IsAny<ChatMessage>()), Times.Exactly(2));
            _uowMock.Verify(u => u.Save(), Times.Once);
        }

        [Fact]
        public async Task DeleteHistory_NoMessages_ThrowsKeyNotFoundException()
        {
            // Arrange
            _repoMock.Setup(r => r.FindAll(It.IsAny<Expression<Func<ChatMessage, bool>>>()))
                     .ReturnsAsync(new List<ChatMessage>());

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteHistory("user_with_no_data"));
        }
        #endregion

        #region Save To Database Tests
        [Fact]
        public async Task SaveToDatabase_ValidModel_SetsIdAndSaves()
        {
            // Arrange
            var saveDto = new SaveDto { UserQuestion = "Hello AI" };
            var chatMessage = new ChatMessage { UserQuestion = "Hello AI" };

            _mapperMock.Setup(m => m.Map<ChatMessage>(saveDto)).Returns(chatMessage);

            // Act
            await _service.SaveToDatabase(saveDto);

            // Assert
            chatMessage.Id.Should().NotBeNullOrEmpty();
            Guid.TryParse(chatMessage.Id, out _).Should().BeTrue(); // تأكد إنه Guid سليم
            _repoMock.Verify(r => r.AddAsync(chatMessage), Times.Once);
            _uowMock.Verify(u => u.Save(), Times.Once);
        }
        #endregion
    }
}