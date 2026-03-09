using AutoMapper;
using Core.Interfaces;
using Core.MappingProfiles;
using Core.Models;
using Core.Result;
using Core.Services;
using Core.Tests.Quests;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Linq.Expressions;

namespace Core.Tests.User.Quests;

public class UserQuestsServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<UserQuests>> _userQuestsRepositoryMock;
    private readonly Mock<IGenericRepository<Quest>> _questRepositoryMock;
    private readonly UserQuestsService _sut;
    public UserQuestsServiceTests()
    {
        _userQuestsRepositoryMock = new Mock<IGenericRepository<UserQuests>>();
        _questRepositoryMock = new Mock<IGenericRepository<Quest>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _unitOfWorkMock.Setup(u => u.GetRepository<Quest>())
            .Returns(_questRepositoryMock.Object);

        _unitOfWorkMock.Setup(u => u.GetRepository<UserQuests>())
            .Returns(_userQuestsRepositoryMock.Object);

        var loggerFactory = NullLoggerFactory.Instance;
        var config = new MapperConfiguration(
            cnfg =>
            {
                cnfg.AddProfile<QuestProfile>();
            },
            loggerFactory
        );
        var mapper = config.CreateMapper();
        _sut = new UserQuestsService(_unitOfWorkMock.Object, mapper);
    }
    [Theory]
    [InlineData(234)]
    [InlineData(567)]
    [InlineData(890)]
    [InlineData(14)]
    public async Task AddUserToQuestAsync_WithValidDate_ShouldReturnQuestToReturnDto(int seed)
    {
        // arrange
        var addUserToQuestDto = FakeUserQuests.GetFakeAddUserToQuestDto(seed).Generate("valid");
        var quest = FakeQuests.GetFakeQuest(seed).Generate();
        _questRepositoryMock.Setup(qr => qr.Find
        (
            It.IsAny<Expression<Func<Quest, bool>>>(),
            It.IsAny<Expression<Func<Quest, object>>>()
        ))
        .ReturnsAsync(quest);
        // act
        var result = await _sut.AddUserToQuestAsync(addUserToQuestDto);
        // assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once, "SaveChangesAsync should be called once when a user is added to a quest");
        _questRepositoryMock.Verify(
            qr => qr.Find
            (
                It.IsAny<Expression<Func<Quest, bool>>>(),
                It.IsAny<Expression<Func<Quest, object>>>()
            ),
            Times.Once,
            "Find should be called once to retrieve the quest by ID"
        );
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.UserAssignedQuests.Should().NotBeEmpty();
        result.Value.UserAssignedQuests.Should().Contain(addUserToQuestDto.QuestId);

    }
    [Theory]
    [InlineData(24)]
    [InlineData(57)]
    [InlineData(89)]
    [InlineData(134)]
    public async Task AddUserToQuestAsync_WithNonExistingQuest_ShouldReturnQuestNotFoundError(int seed)
    {
        // arrange
        var addUserToQuestDto = FakeUserQuests.GetFakeAddUserToQuestDto(seed).Generate(FakeUserQuests.ValidRuleSet);
        _questRepositoryMock.Setup(qr => qr.Find
        (
            It.IsAny<Expression<Func<Quest, bool>>>(),
            It.IsAny<Expression<Func<Quest, object>>>()
        ))
        .ReturnsAsync((Quest?)null);
        // act
        var result = await _sut.AddUserToQuestAsync(addUserToQuestDto);
        // assert
        _questRepositoryMock.Verify(
            qr => qr.Find
            (
                It.IsAny<Expression<Func<Quest, bool>>>(),
                It.IsAny<Expression<Func<Quest, object>>>()
            ),
            Times.Once,
            "Find should be called once to attempt to retrieve the non-existing quest by ID"
        );
        result.IsSuccess.Should().BeFalse();
        result.ErrorList.Should().Contain(QuestErrors.QuestNotFound, "the error list must include QuestNotFound when no quest matches the provided ID");
    }
    [Theory]
    [InlineData(3)]
    [InlineData(89)]
    [InlineData(55)]
    [InlineData(567)]
    public async Task AddUserToQeustAsync_WhenAlreadyUserIsAsignedToQuest_ShouldReturnUserAlreadyAssignedToQuestError(int seed)
    {
        // arrange
        var addUserToQuestDto = FakeUserQuests.GetFakeAddUserToQuestDto(seed).Generate(FakeUserQuests.UserAssignedRuleSet);
        var quest = FakeQuests.GetFakeQuest(seed).Generate();
        _questRepositoryMock.Setup(qr => qr.Find
        (
            It.IsAny<Expression<Func<Quest, bool>>>(),
            It.IsAny<Expression<Func<Quest, object>>>()
        ))
        .ReturnsAsync(quest);
        // act
        var result = await _sut.AddUserToQuestAsync(addUserToQuestDto);
        // assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorList.Should().Contain(UserQuestsErrors.UserAlreadyAssignedToQuest, "the error list must include UserAlreadyAssignedToQuest when the user is already assigned to the quest");
    }
    [Theory]
    [InlineData(23)]
    [InlineData(89)]
    [InlineData(55)]
    [InlineData(567)]
    public async Task GetUserAssignedQuestsAsync_WithValidUserId_ShouldReturnListOfQuestToReturnDto(int seed)
    {
        // arrange
        var userId = "user1";
        var userQuests = FakeUserQuests.GetFakeUserQuests(seed)
            .RuleFor(uq => uq.UserId, f => userId)
            .RuleFor(uq => uq.Quest, f => FakeQuests.GetFakeQuest(seed).Generate())
            .Generate(3);
        _userQuestsRepositoryMock.Setup(uqr => uqr.FindAll
            (
                It.IsAny<Expression<Func<UserQuests, bool>>>(),
                It.IsAny<Expression<Func<UserQuests, object>>>()
            )
        ).ReturnsAsync(userQuests);
        // act
        var result = await _sut.GetUserAssignedQuestsAsync(userId);
        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(3, "the returned list of quests should contain exactly 3 quests assigned to the user");
    }
    [Fact]
    public async Task GetUserAssignedQuestsAsync_WithInvalidUserId_ShouldReturnEmptyList()
    {
        // arrange
        var userId = "user1";
        var emptyUserQuests = new List<UserQuests>();
        _userQuestsRepositoryMock.Setup(uqr => uqr.FindAll
            (
                It.IsAny<Expression<Func<UserQuests, bool>>>(),
                It.IsAny<Expression<Func<UserQuests, object>>>()
            )
        ).ReturnsAsync(emptyUserQuests);
        // act
        var result = await _sut.GetUserAssignedQuestsAsync(userId);
        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty("the returned list of quests should be empty when no quests are assigned to the user");
    }
    [Fact]
    public async Task UnAssignUserFromQuestAsync_WhenUserIsNotAssignedToQuest_ShouldReturnUserNotAssignedToQuestError()
    {
        // arrange
        var questId = "quest1";
        var userId = "user1";
        _userQuestsRepositoryMock.Setup(uqr => uqr.Find
            (
                It.IsAny<Expression<Func<UserQuests, bool>>>()
            )
        ).ReturnsAsync((UserQuests?)null);
        // act
        var result = await _sut.UnAssignUserFromQuestAsync(questId, userId);
        // assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorList.Should().Contain(UserQuestsErrors.UserNotAssignedToQuest, "the error list must include UserNotAssignedToQuest when the user is not assigned to the quest");
    }
    [Theory]
    [InlineData(23)]
    [InlineData(89)]
    public async Task UnAssignUserFromQuestAsync_WithValidDate_ShouldReturnUserNotAssignedToQuestError(int seed)
    {
        // arrange
        var userQuest = FakeUserQuests.GetFakeUserQuests(seed).Generate();
        var userId = userQuest.UserId;
        var questId = userQuest.QuestId;
        _userQuestsRepositoryMock.Setup(uqr => uqr.Find
            (
                It.IsAny<Expression<Func<UserQuests, bool>>>()
            )
        )
        .ReturnsAsync(userQuest);
        // act
        var result = await _sut.UnAssignUserFromQuestAsync(questId, userId);
        // assert
        result.IsSuccess.Should().BeTrue();
        _userQuestsRepositoryMock.Verify(uqr => uqr.Delete(It.IsAny<UserQuests>()), Times.Once, "Delete should be called once to unassign the user from the quest");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once, "SaveChangesAsync should be called once to persist the unassignment of the user from the quest");
    }
}
