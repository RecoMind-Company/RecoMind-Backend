using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Notification.Core.DTOs;
using Notification.Core.Interfaces;
using Notification.Core.Models;
using Notification.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RecoMind.Contracts.Events;


namespace Notification.Tests.Services
{

    public class NotificationServiceTests
    {
        private readonly Mock<INotificationRepository> _repositoryMock;
        private readonly Mock<INotificationHubContext> _hubContextMock;
        private readonly Mock<IPushNotificationService> _fcmServiceMock;
        private readonly Mock<ILogger<NotificationService>> _loggerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly NotificationService _sut;

        public NotificationServiceTests()
        {
            _repositoryMock = new Mock<INotificationRepository>();
            _hubContextMock = new Mock<INotificationHubContext>();
            _fcmServiceMock = new Mock<IPushNotificationService>();
            _loggerMock = new Mock<ILogger<Notification.Core.Services.NotificationService>>();
            _mapperMock = new Mock<IMapper>();

            _sut = new Notification.Core.Services.NotificationService(
                _fcmServiceMock.Object,
                _loggerMock.Object,
                _hubContextMock.Object,
                _repositoryMock.Object,
                _mapperMock.Object
            );
        }

        // SendNotificationAsync
        [Fact]
        public async Task SendNotificationAsync_WhenDtoIsNull_ReturnsFailureResult()
        {
            // Arrange
            NotificationEventDto? dto = null;

            // Act
            var result = await _sut.SendNotificationAsync(dto!);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be("Notification.BadRequest");
        }

        [Fact]
        public async Task SendNotificationAsync_WithValidDto_SavesNotificationAndReturnsSuccess()
        {
            // Arrange
            var dto = new NotificationEventDto
            {
                Title = "Test Title",
                Message = "Test Message",
                ReceiverId = "user-123",
                SenderId = "sender-456"
            };

            var mappedModel = new NotificationModel
            {
                Title = dto.Title,
                Message = dto.Message,
                ReceiverId = dto.ReceiverId,
                SenderId = dto.SenderId
            };

            var responseDto = new NotificationResponseDto
            {
                Title = dto.Title,
                Message = dto.Message,
                ReceiverId = dto.ReceiverId
            };

            _mapperMock.Setup(m => m.Map<NotificationModel>(dto)).Returns(mappedModel);
            _mapperMock.Setup(m => m.Map<NotificationResponseDto>(mappedModel)).Returns(responseDto);
            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<NotificationModel>())).Returns(Task.CompletedTask);
            _hubContextMock.Setup(h => h.SendNotificationAsync(dto.ReceiverId, responseDto)).Returns(Task.CompletedTask);
            _repositoryMock.Setup(r => r.GetUserDeviceTokensAsync(dto.ReceiverId)).ReturnsAsync(new List<string>());

            // Act
            var result = await _sut.SendNotificationAsync(dto);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeTrue();
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<NotificationModel>()), Times.Once);
            _hubContextMock.Verify(h => h.SendNotificationAsync(dto.ReceiverId, responseDto), Times.Once);
        }

        [Fact]
        public async Task SendNotificationAsync_WithDeviceTokens_SendsPushNotificationForEachToken()
        {
            // Arrange
            var dto = new NotificationEventDto
            {
                Title = "Push Title",
                Message = "Push Body",
                ReceiverId = "user-789"
            };

            var mappedModel = new NotificationModel { Id = "notif-id", Title = dto.Title, Message = dto.Message, ReceiverId = dto.ReceiverId };
            var responseDto = new NotificationResponseDto();
            var deviceTokens = new List<string> { "token-A", "token-B" };

            _mapperMock.Setup(m => m.Map<NotificationModel>(dto)).Returns(mappedModel);
            _mapperMock.Setup(m => m.Map<NotificationResponseDto>(mappedModel)).Returns(responseDto);
            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<NotificationModel>())).Returns(Task.CompletedTask);
            _hubContextMock.Setup(h => h.SendNotificationAsync(It.IsAny<string>(), It.IsAny<NotificationResponseDto>())).Returns(Task.CompletedTask);
            _repositoryMock.Setup(r => r.GetUserDeviceTokensAsync(dto.ReceiverId)).ReturnsAsync(deviceTokens);
            _fcmServiceMock.Setup(f => f.SendPushNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                           .ReturnsAsync(true);

            // Act
            var result = await _sut.SendNotificationAsync(dto);

            // Assert
            result.IsSuccess.Should().BeTrue();
            _fcmServiceMock.Verify(
                f => f.SendPushNotificationAsync(It.IsAny<string>(), dto.Title, dto.Message, It.IsAny<Dictionary<string, string>>()),
                Times.Exactly(deviceTokens.Count));
        }

        [Fact]
        public async Task SendNotificationAsync_WhenFcmThrows_StillReturnsSuccessAndLogsError()
        {
            // Arrange
            var dto = new NotificationEventDto { Title = "T", Message = "M", ReceiverId = "user-1" };
            var mappedModel = new NotificationModel { Title = dto.Title, Message = dto.Message, ReceiverId = dto.ReceiverId };
            var responseDto = new NotificationResponseDto();

            _mapperMock.Setup(m => m.Map<NotificationModel>(dto)).Returns(mappedModel);
            _mapperMock.Setup(m => m.Map<NotificationResponseDto>(mappedModel)).Returns(responseDto);
            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<NotificationModel>())).Returns(Task.CompletedTask);
            _hubContextMock.Setup(h => h.SendNotificationAsync(It.IsAny<string>(), It.IsAny<NotificationResponseDto>())).Returns(Task.CompletedTask);
            _repositoryMock.Setup(r => r.GetUserDeviceTokensAsync(dto.ReceiverId)).ReturnsAsync(new List<string> { "some-token" });
            _fcmServiceMock.Setup(f => f.SendPushNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                           .ThrowsAsync(new Exception("FCM unavailable"));

            // Act
            var result = await _sut.SendNotificationAsync(dto);

            // Assert
            result.IsSuccess.Should().BeTrue("FCM errors must not fail the overall operation");
        }

        // GetUserHistoryAsync
        [Fact]
        public async Task GetUserHistoryAsync_WithNullOrEmptyUserId_ReturnsInvalidUserError()
        {
            // Act
            var resultNull = await _sut.GetUserHistoryAsync(null!);
            var resultEmpty = await _sut.GetUserHistoryAsync(string.Empty);
            var resultWhitespace = await _sut.GetUserHistoryAsync("   ");

            // Assert
            resultNull.IsSuccess.Should().BeFalse();
            resultEmpty.IsSuccess.Should().BeFalse();
            resultWhitespace.IsSuccess.Should().BeFalse();

            resultNull.Error!.Code.Should().Be("Notification.InvalidUser");
        }

        [Fact]
        public async Task GetUserHistoryAsync_WithValidUserId_ReturnsNotificationList()
        {
            // Arrange
            var userId = "user-001";
            var notifications = new List<NotificationModel>
        {
            new() { Id = "n1", Message = "Hello", ReceiverId = userId },
            new() { Id = "n2", Message = "World", ReceiverId = userId }
        };
            var responseDtos = notifications.Select(n => new NotificationResponseDto { Id = n.Id, Message = n.Message }).ToList();

            _repositoryMock.Setup(r => r.GetUserNotificationsAsync(userId)).ReturnsAsync(notifications);
            _mapperMock.Setup(m => m.Map<IEnumerable<NotificationResponseDto>>(notifications)).Returns(responseDtos);

            // Act
            var result = await _sut.GetUserHistoryAsync(userId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(2);
            _repositoryMock.Verify(r => r.GetUserNotificationsAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetUserHistoryAsync_WhenUserHasNoNotifications_ReturnsEmptyCollection()
        {
            // Arrange
            var userId = "user-no-notifs";
            _repositoryMock.Setup(r => r.GetUserNotificationsAsync(userId)).ReturnsAsync(new List<NotificationModel>());
            _mapperMock.Setup(m => m.Map<IEnumerable<NotificationResponseDto>>(It.IsAny<IEnumerable<NotificationModel>>()))
                       .Returns(Enumerable.Empty<NotificationResponseDto>());

            // Act
            var result = await _sut.GetUserHistoryAsync(userId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEmpty();
        }

        // GetByStatusAsync
        [Fact]
        public async Task GetByStatusAsync_WithNullOrEmptyUserId_ReturnsInvalidUserError()
        {
            // Act
            var result = await _sut.GetByStatusAsync(string.Empty, isRead: false);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be("Notification.InvalidUser");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetByStatusAsync_WithValidUserId_ReturnsFilteredNotifications(bool isRead)
        {
            // Arrange
            var userId = "user-abc";
            var models = new List<NotificationModel> { new() { Id = "x", IsRead = isRead } };
            var dtos = new List<NotificationResponseDto> { new() { Id = "x", IsRead = isRead } };

            _repositoryMock.Setup(r => r.GetByReadStatusAsync(userId, isRead)).ReturnsAsync(models);
            _mapperMock.Setup(m => m.Map<IEnumerable<NotificationResponseDto>>(models)).Returns(dtos);

            // Act
            var result = await _sut.GetByStatusAsync(userId, isRead);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(1);
            _repositoryMock.Verify(r => r.GetByReadStatusAsync(userId, isRead), Times.Once);
        }

        // GetAndMarkAsReadAsync
        [Fact]
        public async Task GetAndMarkAsReadAsync_WithNullOrEmptyId_ReturnsInvalidIdError()
        {
            // Act
            var result = await _sut.GetAndMarkAsReadAsync(string.Empty);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be("Notification.InvalidId");
        }

        [Fact]
        public async Task GetAndMarkAsReadAsync_WhenNotificationNotFound_ReturnsNotFoundError()
        {
            // Arrange
            var id = "missing-id";
            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((NotificationModel?)null);

            // Act
            var result = await _sut.GetAndMarkAsReadAsync(id);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be("Notification.NotFound");
        }

        [Fact]
        public async Task GetAndMarkAsReadAsync_WhenAlreadyRead_DoesNotCallMarkAsRead()
        {
            // Arrange
            var id = "already-read-id";
            var model = new NotificationModel { Id = id, IsRead = true };
            var dto = new NotificationResponseDto { Id = id, IsRead = true };

            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(model);
            _mapperMock.Setup(m => m.Map<NotificationResponseDto>(model)).Returns(dto);

            // Act
            var result = await _sut.GetAndMarkAsReadAsync(id);

            // Assert
            result.IsSuccess.Should().BeTrue();
            _repositoryMock.Verify(r => r.MarkAsReadAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetAndMarkAsReadAsync_WhenUnread_MarksAsReadAndReturnsDto()
        {
            // Arrange
            var id = "unread-id";
            var model = new NotificationModel { Id = id, IsRead = false };
            var dto = new NotificationResponseDto { Id = id, IsRead = true };

            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(model);
            _repositoryMock.Setup(r => r.MarkAsReadAsync(id)).Returns(Task.CompletedTask);
            _mapperMock.Setup(m => m.Map<NotificationResponseDto>(model)).Returns(dto);

            // Act
            var result = await _sut.GetAndMarkAsReadAsync(id);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value!.IsRead.Should().BeTrue();
            _repositoryMock.Verify(r => r.MarkAsReadAsync(id), Times.Once);
        }

        // MarkAllAsReadAsync
        [Fact]
        public async Task MarkAllAsReadAsync_WithNullOrEmptyUserId_ReturnsInvalidUserError()
        {
            // Act
            var result = await _sut.MarkAllAsReadAsync("  ");

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be("Notification.InvalidUser");
        }

        [Fact]
        public async Task MarkAllAsReadAsync_WithValidUserId_CallsRepositoryAndReturnsTrue()
        {
            // Arrange
            var userId = "user-mark-all";
            _repositoryMock.Setup(r => r.MarkAllAsReadAsync(userId)).Returns(Task.CompletedTask);

            // Act
            var result = await _sut.MarkAllAsReadAsync(userId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeTrue();
            _repositoryMock.Verify(r => r.MarkAllAsReadAsync(userId), Times.Once);
        }

        // GetUnreadCountAsync
        [Fact]
        public async Task GetUnreadCountAsync_WithNullOrEmptyUserId_ReturnsInvalidUserError()
        {
            // Act
            var result = await _sut.GetUnreadCountAsync(string.Empty);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be("Notification.InvalidUser");
        }

        [Fact]
        public async Task GetUnreadCountAsync_WithValidUserId_ReturnsCorrectCount()
        {
            // Arrange
            var userId = "user-count";
            _repositoryMock.Setup(r => r.GetUnreadCountAsync(userId)).ReturnsAsync(7);

            // Act
            var result = await _sut.GetUnreadCountAsync(userId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(7);
        }

        [Fact]
        public async Task GetUnreadCountAsync_WhenNoUnreadNotifications_ReturnsZero()
        {
            // Arrange
            var userId = "user-no-unread";
            _repositoryMock.Setup(r => r.GetUnreadCountAsync(userId)).ReturnsAsync(0);

            // Act
            var result = await _sut.GetUnreadCountAsync(userId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(0);
        }

        // DeleteNotificationAsync
        [Fact]
        public async Task DeleteNotificationAsync_WithNullOrEmptyId_ReturnsInvalidIdError()
        {
            // Act
            var result = await _sut.DeleteNotificationAsync(string.Empty);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be("Notification.InvalidId");
        }

        [Fact]
        public async Task DeleteNotificationAsync_WhenNotificationNotFound_ReturnsNotFoundError()
        {
            // Arrange
            var id = "nonexistent-id";
            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((NotificationModel?)null);

            // Act
            var result = await _sut.DeleteNotificationAsync(id);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be("Notification.NotFound");
            result.Error.Message.Should().Contain(id);
        }

        [Fact]
        public async Task DeleteNotificationAsync_WhenNotificationExists_DeletesAndReturnsTrue()
        {
            // Arrange
            var id = "delete-me";
            var model = new NotificationModel { Id = id };

            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(model);
            _repositoryMock.Setup(r => r.DeleteAsync(id)).Returns(Task.CompletedTask);

            // Act
            var result = await _sut.DeleteNotificationAsync(id);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeTrue();
            _repositoryMock.Verify(r => r.DeleteAsync(id), Times.Once);
        }

        // RegisterDeviceTokenAsync
        [Fact]
        public async Task RegisterDeviceTokenAsync_WithNullOrEmptyUserId_ReturnsInvalidUserError()
        {
            // Arrange
            var dto = new RegisterDeviceTokenDto { DeviceToken = "valid-token" };

            // Act
            var result = await _sut.RegisterDeviceTokenAsync(string.Empty, dto);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be("Notification.InvalidUser");
        }

        [Fact]
        public async Task RegisterDeviceTokenAsync_WithEmptyDeviceToken_ReturnsInvalidTokenError()
        {
            // Arrange
            var dto = new RegisterDeviceTokenDto { DeviceToken = string.Empty };

            // Act
            var result = await _sut.RegisterDeviceTokenAsync("user-123", dto);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be("Notification.InvalidToken");
        }

        [Fact]
        public async Task RegisterDeviceTokenAsync_WhenTokenAlreadyExists_UpdatesExistingToken()
        {
            // Arrange
            var userId = "user-123";
            var dto = new RegisterDeviceTokenDto { DeviceToken = "existing-token", DeviceType = "Android" };
            var existingToken = new UserDeviceToken { Id = "token-id", UserId = userId, DeviceToken = dto.DeviceToken };

            _repositoryMock.Setup(r => r.FindDeviceTokenAsync(userId, dto.DeviceToken)).ReturnsAsync(existingToken);
            _repositoryMock.Setup(r => r.UpdateDeviceTokenAsync(existingToken)).Returns(Task.CompletedTask);

            // Act
            var result = await _sut.RegisterDeviceTokenAsync(userId, dto);

            // Assert
            result.IsSuccess.Should().BeTrue();
            existingToken.DeviceType.Should().Be("Android");
            existingToken.UpdatedAt.Should().NotBeNull();
            _repositoryMock.Verify(r => r.UpdateDeviceTokenAsync(existingToken), Times.Once);
            _repositoryMock.Verify(r => r.AddDeviceTokenAsync(It.IsAny<UserDeviceToken>()), Times.Never);
        }

        [Fact]
        public async Task RegisterDeviceTokenAsync_WhenTokenIsNew_AddsNewToken()
        {
            // Arrange
            var userId = "user-456";
            var dto = new RegisterDeviceTokenDto { DeviceToken = "brand-new-token", DeviceType = "iOS" };

            _repositoryMock.Setup(r => r.FindDeviceTokenAsync(userId, dto.DeviceToken)).ReturnsAsync((UserDeviceToken?)null);
            _repositoryMock.Setup(r => r.AddDeviceTokenAsync(It.IsAny<UserDeviceToken>())).Returns(Task.CompletedTask);

            // Act
            var result = await _sut.RegisterDeviceTokenAsync(userId, dto);

            // Assert
            result.IsSuccess.Should().BeTrue();
            _repositoryMock.Verify(
                r => r.AddDeviceTokenAsync(It.Is<UserDeviceToken>(t =>
                    t.UserId == userId &&
                    t.DeviceToken == dto.DeviceToken &&
                    t.DeviceType == "iOS" &&
                    !string.IsNullOrEmpty(t.Id)
                )),
                Times.Once);
            _repositoryMock.Verify(r => r.UpdateDeviceTokenAsync(It.IsAny<UserDeviceToken>()), Times.Never);
        }
    }
}
