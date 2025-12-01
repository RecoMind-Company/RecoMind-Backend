using AutoMapper;
using Core.DTOs;
using Core.DTOs.AuthenticationDtos;
using Core.Interfaces;
using Core.Models;
using Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;

namespace Core.Tests
{
    public class InvitationServiceTests
    {
        private readonly Mock<IGrpcAuthenticationService> _mockGrpcAuthService;
        private readonly Mock<IInvitationRepository> _mockInvitationRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<InvitationService>> _mockLogger;
        private readonly InvitationService _sut;
        public InvitationServiceTests()
        {
            _mockGrpcAuthService = new Mock<IGrpcAuthenticationService>();
            _mockInvitationRepository = new Mock<IInvitationRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<InvitationService>>();
            _sut = new InvitationService(_mockGrpcAuthService.Object,
                                         _mockInvitationRepository.Object,
                                         _mockUnitOfWork.Object,
                                         _mockMapper.Object,
                                         _mockLogger.Object
            );
        }
        [Fact]
        public async Task SendInvitationAsync_WhenNotAuthenticated_ShouldReturnFalseAndMessage()
        {
            // arrange
            var inputDto = new SendInvitationDto
            {
                CompanyId = "abc",
                Email = "test@gmail.com",
                ReciverRole = "teamleader",
                SenderId = "abc"
            };
            var GrpcRegisterFailedResponse = new AuthResponseDto { IsAuthenticated = false, Message = "error" };
            _mockGrpcAuthService.Setup(s => s.Register(inputDto.Email, inputDto.ReciverRole)).ReturnsAsync(GrpcRegisterFailedResponse);
            // act
            var result = await _sut.SendInvitationAsync(inputDto);
            // assert
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("Failed");
            _mockInvitationRepository.Verify(r => r.CreateAsync(It.IsAny<Invitation>()), Times.Never, "shouldn't be called");
            _mockUnitOfWork.Verify(u => u.Save(), Times.Never, "shouldn't be called");
        }
        [Fact]
        public async Task SendInvitationAsync_WhenIsAuthenticated_ShouldRetrunTrueAndMessage()
        {
            // arrange 
            var inputDto = new SendInvitationDto
            {
                CompanyId = "abc",
                Email = "test@gmail.com",
                ReciverRole = "teamleader",
                SenderId = "abc"
            };
            var GrpcRegisterSuccessResponse = new AuthResponseDto { IsAuthenticated = true, Message = "success" };
            _mockGrpcAuthService.Setup(s => s.Register(inputDto.Email, inputDto.ReciverRole)).ReturnsAsync(GrpcRegisterSuccessResponse);
            // act
            var result = await _sut.SendInvitationAsync(inputDto);
            // assert
            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("successfully");
            _mockInvitationRepository.Verify(r => r.CreateAsync(It.IsAny<Invitation>()), Times.Once, "should be called once");
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once, "should be called once");
        }
        [Fact]
        public async Task LoginAttemptWithInvitation_WhenNoInvitation_ShouldReturnFalseAndMessage()
        {
            // arrange
            var email = "test@gmail.com";
            _mockInvitationRepository.Setup(r => r.Find(It.IsAny<Expression<Func<Invitation, bool>>>())).ReturnsAsync((Invitation)null);
            // act
            var result = await _sut.LoginAttemptWithInvitation(email);
            // assert
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("no invitation");
            _mockInvitationRepository.Verify(r => r.Find(It.IsAny<Expression<Func<Invitation, bool>>>()), Times.Once, "should be called once");
        }
        [Fact]
        public async Task LoginAttemptWithInvitation_WhenInvitationIsAllreadyAccepted_ShouldReturnTrueAndMessage()
        {
            // arrange
            var email = "test@gmail.com";
            var invitation = new Invitation
            {
                Id = 1,
                Email = "test",
                CompanyId = "test",
                CreatedAt = DateTime.UtcNow,
                ReceiverRole = "teamleader",
                SenderId = "test",
                Status = Status.Accepted
            };
            _mockInvitationRepository.Setup(r => r.Find(It.IsAny<Expression<Func<Invitation, bool>>>())).ReturnsAsync(invitation);
            // act
            var result = await _sut.LoginAttemptWithInvitation(email);
            // assert
            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("allready accepted");
            _mockInvitationRepository.Verify(r => r.Find(It.IsAny<Expression<Func<Invitation, bool>>>()), Times.Once, "should be called once");
            _mockInvitationRepository.Verify(r => r.Update(It.IsAny<Invitation>()), Times.Never, "shouldn't be called");
            _mockUnitOfWork.Verify(u => u.Save(), Times.Never, "shouldn't be called");
        }
        [Fact]
        public async Task LoginAttemptWithInvitation_WhenStatusIsExpired_ShouldReturnFalseAndMessage()
        {
            // arrange
            var email = "test@gmail.com";
            var invitation = new Invitation
            {
                Id = 1,
                Email = "test",
                CompanyId = "test",
                CreatedAt = DateTime.UtcNow,
                ReceiverRole = "teamleader",
                SenderId = "test",
                Status = Status.Expired
            };
            _mockInvitationRepository.Setup(r => r.Find(It.IsAny<Expression<Func<Invitation, bool>>>())).ReturnsAsync(invitation);
            // act
            var result = await _sut.LoginAttemptWithInvitation(email);
            // assert
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("expired");
            _mockInvitationRepository.Verify(r => r.Find(It.IsAny<Expression<Func<Invitation, bool>>>()), Times.Once, "should be called once");
            _mockInvitationRepository.Verify(r => r.Update(It.IsAny<Invitation>()), Times.Once, "should be called once");
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once, "should be called once");
        }
        [Fact]
        public async Task LoginAttemptWithInvitation_WhenStatusIsAccepted_ShouldReturnTrueAndMessage()
        {
            // arrange
            var email = "test@gmail.com";
            var invitation = new Invitation
            {
                Id = 1,
                Email = "test",
                CompanyId = "test",
                CreatedAt = DateTime.UtcNow,
                ReceiverRole = "teamleader",
                SenderId = "test",
                Status = Status.Pending
            };
            _mockInvitationRepository.Setup(r => r.Find(It.IsAny<Expression<Func<Invitation, bool>>>())).ReturnsAsync(invitation);
            // act
            var result = await _sut.LoginAttemptWithInvitation(email);
            // assert
            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("accepted successfully");
            _mockInvitationRepository.Verify(r => r.Find(It.IsAny<Expression<Func<Invitation, bool>>>()), Times.Once, "should be called once");
            _mockInvitationRepository.Verify(r => r.Update(It.IsAny<Invitation>()), Times.Once, "should be called once");
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once, "should be called once");
        }
        [Fact]
        public async Task GetInvitationByIdAsync_WhenNoInvitation_ShouldReturnNull()
        {
            // arrange
            _mockInvitationRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Invitation)null);
            // act
            var result = await _sut.GetInvitationByIdAsync(1);
            // assert
            result.Should().BeNull();
            _mockInvitationRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Once, "should be called once");
            _mockMapper.Verify(m => m.Map<InvitationsToReturnDto>(It.IsAny<Invitation>()), Times.Never, "shouldn't be called");
        }
        [Fact]
        public async Task GetInvitationByIdAsync_WhenInvitationExists_ShouldReturnMappedDto()
        {
            // arrange
            var mappedInvitationDto = new InvitationsToReturnDto
            {
                UserName = "testuser",
                Status = "Pending",
                ExpMessage = "Not expired"
            };
            var fakeInvitation = new Invitation
            {
                Id = 1,
                Email = "test",
                CompanyId = "test",
                CreatedAt = DateTime.UtcNow,
                ReceiverRole = "teamleader",
                SenderId = "test",
                Status = Status.Pending
            };
            _mockInvitationRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(fakeInvitation);
            _mockMapper.Setup(m => m.Map<InvitationsToReturnDto>(It.IsAny<Invitation>())).Returns(mappedInvitationDto);
            // act
            var result = await _sut.GetInvitationByIdAsync(1);
            // assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(mappedInvitationDto);
            _mockInvitationRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Once, "should be called once");
            _mockMapper.Verify(m => m.Map<InvitationsToReturnDto>(It.IsAny<Invitation>()), Times.Once, "should be called once");
        }
        [Fact]
        public async Task GetInvitationsByStatus_WhenNoInvitations_ShouldReturnEmptyList()
        {
            // arrange
            var fakeGetInvitationDto = new GetInvitationDto
            {
                CompanyId = "testcompany",
                Status = "Pending"
            };
            _mockInvitationRepository.Setup(r => r.FindAll(It.IsAny<Expression<Func<Invitation, bool>>>())).ReturnsAsync(Enumerable.Empty<Invitation>());
            // act
            var result = await _sut.GetInvitationsByStatus(fakeGetInvitationDto);
            // assert
            result.Should().BeEmpty();
            _mockInvitationRepository.Verify(r => r.FindAll(It.IsAny<Expression<Func<Invitation, bool>>>()), Times.Once, "should be called once");
            _mockMapper.Verify(m => m.Map<IEnumerable<InvitationsToReturnDto>>(It.IsAny<IEnumerable<Invitation>>()), Times.Never, "shouldn't be called");
        }
        [Fact]
        public async Task GetInvitationByStatus_WhenThereIsInvitations_ShouldReturnMappedList()
        {
            // arrange
            var fakeGetInvitationDto = new GetInvitationDto
            {
                CompanyId = "testcompany",
                Status = "Pending"
            };
            var fakeInvitations = new List<Invitation>
            {
                new Invitation
                {
                    Id = 1,
                    Email = "test1",
                    CompanyId = "testcompany",
                    CreatedAt = DateTime.UtcNow,
                    ReceiverRole = "teamleader",
                    SenderId = "test",
                    Status = Status.Pending
                },
                new Invitation
                {
                    Id = 2,
                    Email = "test2",
                    CompanyId = "testcompany",
                    CreatedAt = DateTime.UtcNow,
                    ReceiverRole = "developer",
                    SenderId = "test",
                    Status = Status.Pending
                }
            };
            var mappedInvitationDto = new List<InvitationsToReturnDto>
            {
                new InvitationsToReturnDto
                {
                    UserName = "testuser1",
                    ExpMessage = "Not expired",
                    Status = "Pending"
                },
                new InvitationsToReturnDto
                {
                    UserName = "testuser2",
                    ExpMessage = "Not expired",
                    Status = "Pending"
                }
            };
            _mockInvitationRepository.Setup(r => r.FindAll(It.IsAny<Expression<Func<Invitation, bool>>>())).ReturnsAsync(fakeInvitations);
            _mockMapper.Setup(m => m.Map<IEnumerable<InvitationsToReturnDto>>(It.IsAny<IEnumerable<Invitation>>())).Returns(mappedInvitationDto);
            // act
            var result = await _sut.GetInvitationsByStatus(fakeGetInvitationDto);
            // assert
            result.Should().NotBeEmpty();
            result.Should().BeEquivalentTo(mappedInvitationDto);
            _mockInvitationRepository.Verify(r => r.FindAll(It.IsAny<Expression<Func<Invitation, bool>>>()), Times.Once, "should be called once");
            _mockMapper.Verify(m => m.Map<IEnumerable<InvitationsToReturnDto>>(It.IsAny<IEnumerable<Invitation>>()), Times.Once, "should be called once");
        }
    }
}
