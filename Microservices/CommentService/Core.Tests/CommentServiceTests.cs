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

    #region GetCommentsByPlanIdAsync Tests

    [Fact]
    public async Task GetCommentsByPlanIdAsync_WithValidPlan_ShouldReturnComments()
    {
        // Arrange
        string planId = "plan-123";
        var planDto = CommentFakers.GetPlanIdsDto(seed: 0).Generate(); // IsExisted = true
        var comments = CommentFakers.GetComment(seed: 0).Generate(3);

        _grpcPlanServiceMock
            .Setup(x => x.GetPlanIdsAsync(planId))
            .ReturnsAsync(planDto);

        _commentRepositoryMock
            .Setup(x => x.FindAll(
                It.IsAny<System.Linq.Expressions.Expression<Func<Comment, bool>>>(),
                It.IsAny<Func<IQueryable<Comment>, IOrderedQueryable<Comment>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Comment, object>>[]>()))
            .ReturnsAsync(comments);

        // Act
        var result = await _sut.GetCommentsByPlanIdAsync(planId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Count().Should().Be(3);
        result.ErrorsList.Should().BeEmpty();

        _commentRepositoryMock.Verify(
            x => x.FindAll(
                It.IsAny<System.Linq.Expressions.Expression<Func<Comment, bool>>>(),
                It.IsAny<Func<IQueryable<Comment>, IOrderedQueryable<Comment>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Comment, object>>[]>()),
            Times.Once);
    }

    [Fact]
    public async Task GetCommentsByPlanIdAsync_WithValidPlan_ShouldReturnEmptyList()
    {
        // Arrange
        string planId = "plan-123";
        var planDto = CommentFakers.GetPlanIdsDto(seed: 0).Generate(); // IsExisted = true

        _grpcPlanServiceMock
            .Setup(x => x.GetPlanIdsAsync(planId))
            .ReturnsAsync(planDto);

        _commentRepositoryMock
            .Setup(x => x.FindAll(
                It.IsAny<System.Linq.Expressions.Expression<Func<Comment, bool>>>(),
                It.IsAny<Func<IQueryable<Comment>, IOrderedQueryable<Comment>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Comment, object>>[]>()))
            .ReturnsAsync(Enumerable.Empty<Comment>());

        // Act
        var result = await _sut.GetCommentsByPlanIdAsync(planId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().BeEmpty();
        result.ErrorsList.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCommentsByPlanIdAsync_WithInvalidPlan_ShouldFail()
    {
        // Arrange
        string planId = "invalid-plan";
        var planDto = CommentFakers.GetPlanIdsDto(seed: 1).Generate(); // IsExisted = false

        _grpcPlanServiceMock
            .Setup(x => x.GetPlanIdsAsync(planId))
            .ReturnsAsync(planDto);

        // Act
        var result = await _sut.GetCommentsByPlanIdAsync(planId);

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

        _commentRepositoryMock.Verify(
            x => x.FindAll(
                It.IsAny<System.Linq.Expressions.Expression<Func<Comment, bool>>>(),
                It.IsAny<Func<IQueryable<Comment>, IOrderedQueryable<Comment>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Comment, object>>[]>()),
            Times.Never);
    }

    #endregion

    #region UpdateCommentAsync Tests

    [Fact]
    public async Task UpdateCommentAsync_WithValidData_ShouldSucceed()
    {
        // Arrange - Even seed generates recent CreatedAt
        var updateCommentDto = CommentFakers.GetUpdateCommentDto(seed: 0).Generate();
        var existingComment = CommentFakers.GetComment(seed: 0).Generate();
        existingComment.CreatedAt = DateTime.UtcNow.AddMinutes(-2); // Within 5-minute window

        _commentRepositoryMock
            .Setup(x => x.Find(
                It.IsAny<System.Linq.Expressions.Expression<Func<Comment, bool>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Comment, object>>[]>()))
            .ReturnsAsync(existingComment);

        _commentRepositoryMock
            .Setup(x => x.Update(It.IsAny<Comment>()));

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _sut.UpdateCommentAsync(updateCommentDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.ErrorsList.Should().BeEmpty();

        _commentRepositoryMock.Verify(x => x.Update(It.IsAny<Comment>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateCommentAsync_CommentNotFound_ShouldFail()
    {
        // Arrange
        var updateCommentDto = CommentFakers.GetUpdateCommentDto(seed: 0).Generate();

        _commentRepositoryMock
            .Setup(x => x.Find(
                It.IsAny<System.Linq.Expressions.Expression<Func<Comment, bool>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Comment, object>>[]>()))
            .ReturnsAsync((Comment?)null);

        // Act
        var result = await _sut.UpdateCommentAsync(updateCommentDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result
            .ErrorsList
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .Be(CommentErrors.NotFound);

        _commentRepositoryMock.Verify(x => x.Update(It.IsAny<Comment>()), Times.Never);
    }

    [Fact]
    public async Task UpdateCommentAsync_UserNotOwner_ShouldFail()
    {
        // Arrange
        // using different seeds heare will generate different UserId for updateCommentDto and existingComment.
        var updateCommentDto = CommentFakers.GetUpdateCommentDto(seed: 4).Generate();
        var existingComment = CommentFakers.GetComment(seed: 0).Generate();

        _commentRepositoryMock
            .Setup(x => x.Find(
                It.IsAny<System.Linq.Expressions.Expression<Func<Comment, bool>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Comment, object>>[]>()))
            .ReturnsAsync(existingComment);

        // Act
        var result = await _sut.UpdateCommentAsync(updateCommentDto);

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

        _commentRepositoryMock.Verify(x => x.Update(It.IsAny<Comment>()), Times.Never);
    }

    [Fact]
    public async Task UpdateCommentAsync_EditTimeoutExceeded_ShouldFail()
    {
        // Arrange
        var updateCommentDto = CommentFakers.GetUpdateCommentDto(seed: 3).Generate();
        var existingComment = CommentFakers.GetComment(seed: 3).Generate(); // this the genrate a comment with CreatedAt in the past


        _commentRepositoryMock
            .Setup(x => x.Find(
                It.IsAny<System.Linq.Expressions.Expression<Func<Comment, bool>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Comment, object>>[]>()))
            .ReturnsAsync(existingComment);

        // Act
        var result = await _sut.UpdateCommentAsync(updateCommentDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result
            .ErrorsList
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .Be(CommentErrors.EditTimeout);

        _commentRepositoryMock.Verify(x => x.Update(It.IsAny<Comment>()), Times.Never);
    }

    #endregion
}
