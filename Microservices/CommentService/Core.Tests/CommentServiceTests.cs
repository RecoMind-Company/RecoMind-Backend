using AutoMapper;
using Core.Interface;
using Core.MappingProfiles;
using Core.Models;
using Core.Result;
using Core.Services;
using Core.ServicesAbstraction;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Core.Tests;

public class CommentServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<Comment>> _commentRepositoryMock;
    private readonly Mock<IGrpcTeamService> _grpcTeamServiceMock;
    private readonly Mock<IGrpcPlanService> _grpcPlanServiceMock;
    private readonly IMapper _mapper;
    private readonly CommentService _sut;

    public CommentServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
        _unitOfWorkMock.Setup(u => u.GetRepository<Comment>()).Returns(_commentRepositoryMock.Object);

        _grpcTeamServiceMock = new Mock<IGrpcTeamService>();
        _grpcPlanServiceMock = new Mock<IGrpcPlanService>();

        var nullLoggerFactory = new NullLoggerFactory();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<CommentProfile>(), nullLoggerFactory);
        _mapper = config.CreateMapper();

        _sut = new CommentService(_unitOfWorkMock.Object, _mapper, _grpcTeamServiceMock.Object, _grpcPlanServiceMock.Object);
    }

    #region AddCommentAsync Tests

    [Fact]
    public async Task AddCommentAsync_WithValidData_UserIsOwner_ShouldSucceed()
    {
        // Arrange - Even seed generates valid data
        var addCommentDto = CommentFakers.GetAddCommentDto(seed: 0).Generate();
        var planDto = CommentFakers.GetPlanIdsDto(seed: 0).Generate(); // IsExisted = true

        _grpcPlanServiceMock
            .Setup(x => x.GetPlanIdsAsync(addCommentDto.PlanId!))
            .ReturnsAsync(planDto);

        _grpcPlanServiceMock
            .Setup(x => x.IsOwnerOfPlanAsync(addCommentDto.UserId!, addCommentDto.PlanId!))
            .ReturnsAsync(true);

        _commentRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Comment>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _sut.AddCommentAsync(addCommentDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.ErrorsList.Should().BeEmpty();

        _commentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Comment>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddCommentAsync_WithValidData_UserInTeam_ShouldSucceed()
    {
        // Arrange - Even seed generates valid data
        var addCommentDto = CommentFakers.GetAddCommentDto(seed: 0).Generate();
        var planDto = CommentFakers.GetPlanIdsDto(seed: 0).Generate(); // IsExisted = true

        _grpcPlanServiceMock
            .Setup(x => x.GetPlanIdsAsync(addCommentDto.PlanId!))
            .ReturnsAsync(planDto);

        _grpcPlanServiceMock
            .Setup(x => x.IsOwnerOfPlanAsync(addCommentDto.UserId!, addCommentDto.PlanId!))
            .ReturnsAsync(false);

        _grpcTeamServiceMock
            .Setup(x => x.IsUserExist(addCommentDto.UserId!, planDto.TeamId!))
            .ReturnsAsync(true);

        _commentRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Comment>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _sut.AddCommentAsync(addCommentDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.ErrorsList.Should().BeEmpty();

        _commentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Comment>()), Times.Once);
    }

    [Fact]
    public async Task AddCommentAsync_WithInvalidPlan_ShouldFail()
    {
        // Arrange - Odd seed generates invalid data (IsExisted = false)
        var addCommentDto = CommentFakers.GetAddCommentDto(seed: 0).Generate();
        var planDto = CommentFakers.GetPlanIdsDto(seed: 1).Generate(); // IsExisted = false

        _grpcPlanServiceMock
            .Setup(x => x.GetPlanIdsAsync(addCommentDto.PlanId!))
            .ReturnsAsync(planDto);

        // Act
        var result = await _sut.AddCommentAsync(addCommentDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result
            .ErrorsList
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .Be(PlanErrors.PlanNotFound);

        _commentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Comment>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task AddCommentAsync_UserNotOwnerAndNotInTeam_ShouldFail()
    {
        // Arrange - Even seed generates valid plan
        var addCommentDto = CommentFakers.GetAddCommentDto(seed: 0).Generate();
        var planDto = CommentFakers.GetPlanIdsDto(seed: 0).Generate(); // IsExisted = true

        _grpcPlanServiceMock
            .Setup(x => x.GetPlanIdsAsync(addCommentDto.PlanId!))
            .ReturnsAsync(planDto);

        _grpcPlanServiceMock
            .Setup(x => x.IsOwnerOfPlanAsync(addCommentDto.UserId!, addCommentDto.PlanId!))
            .ReturnsAsync(false);

        _grpcTeamServiceMock
            .Setup(x => x.IsUserExist(addCommentDto.UserId!, planDto.TeamId!))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.AddCommentAsync(addCommentDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result
            .ErrorsList
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .Be(CommentErrors.AccessDenied);

        _commentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Comment>()), Times.Never);
    }

    #endregion
}
