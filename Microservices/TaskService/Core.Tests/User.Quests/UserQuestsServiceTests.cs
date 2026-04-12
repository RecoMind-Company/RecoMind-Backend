using AutoMapper;
using Core.Interfaces;
using Core.MappingProfiles;
using Core.Models;
using Core.Result;
using Core.Services;
using Core.ServicesAbstractions;
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
    private readonly Mock<IGrpcTeamService> _grpcTeamServiceMock;
    private readonly UserQuestsService _sut;

    public UserQuestsServiceTests()
    {
        _userQuestsRepositoryMock = new Mock<IGenericRepository<UserQuests>>();
        _questRepositoryMock = new Mock<IGenericRepository<Quest>>();
        _grpcTeamServiceMock = new Mock<IGrpcTeamService>();
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
        _sut = new UserQuestsService(_unitOfWorkMock.Object, mapper, _grpcTeamServiceMock.Object);
    }

    [Fact]
    public async Task AddUserToQuestAsync_WithValidDate_ShouldReturnQuestToReturnDto()
    {
        // arrange
        var addUserToQuestDto = FakeUserQuests.GetFakeAddUserToQuestDto().Generate(FakeUserQuests.ValidRuleSet);
        var quest = FakeQuests.GetFakeQuest().Generate();

        _questRepositoryMock.Setup(qr => qr.Find
        (
            It.IsAny<Expression<Func<Quest, bool>>>(),
            It.IsAny<Expression<Func<Quest, object>>>()
        ))
        .ReturnsAsync(quest);

        _grpcTeamServiceMock.Setup(gs => gs.IsUserExist(
            It.IsAny<string>(),
            It.IsAny<string>()
        ))
        .ReturnsAsync(true);

        // act
        var result = await _sut.AddUserToQuestAsync(addUserToQuestDto);

        // assert
        _grpcTeamServiceMock.Verify(
            gs => gs.IsUserExist(addUserToQuestDto.UserId, addUserToQuestDto.TeamId),
            Times.Once,
            "IsUserExist should be called once to verify the user is in the team"
        );

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

    [Fact]
    public async Task AddUserToQuestAsync_WithNonExistingQuest_ShouldReturnQuestNotFoundError()
    {
        // arrange
        var addUserToQuestDto = FakeUserQuests.GetFakeAddUserToQuestDto().Generate(FakeUserQuests.ValidRuleSet);
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
        _grpcTeamServiceMock.Verify(
            gs => gs.IsUserExist(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never,
            "IsUserExist should not be called when the quest does not exist"
        );
        result.IsSuccess.Should().BeFalse();
        result.ErrorList.Should().Contain(QuestErrors.QuestNotFound, "the error list must include QuestNotFound when no quest matches the provided ID");
    }

    [Fact]
    public async Task AddUserToQeustAsync_WhenAlreadyUserIsAsignedToQuest_ShouldReturnUserAlreadyAssignedToQuestError()
    {
        // arrange
        var addUserToQuestDto = FakeUserQuests.GetFakeAddUserToQuestDto().Generate(FakeUserQuests.UserAssignedRuleSet);
        var quest = FakeQuests.GetFakeQuest().Generate();
        _questRepositoryMock.Setup(qr => qr.Find
        (
            It.IsAny<Expression<Func<Quest, bool>>>(),
            It.IsAny<Expression<Func<Quest, object>>>()
        ))
        .ReturnsAsync(quest);

        // act
        var result = await _sut.AddUserToQuestAsync(addUserToQuestDto);

        // assert
        _grpcTeamServiceMock.Verify(
            gs => gs.IsUserExist(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never,
            "IsUserExist should not be called when the user is already assigned to the quest"
        );
        result.IsSuccess.Should().BeFalse();
        result.ErrorList.Should().Contain(UserQuestsErrors.UserAlreadyAssignedToQuest, "the error list must include UserAlreadyAssignedToQuest when the user is already assigned to the quest");
    }

    [Fact]
    public async Task AddUserToQuestAsync_WhenUserIsNotInTeam_ShouldReturnUserNotInTeamError()
    {
        // arrange
        var addUserToQuestDto = FakeUserQuests.GetFakeAddUserToQuestDto().Generate(FakeUserQuests.ValidRuleSet);
        var quest = FakeQuests.GetFakeQuest().Generate();

        _questRepositoryMock.Setup(qr => qr.Find
        (
            It.IsAny<Expression<Func<Quest, bool>>>(),
            It.IsAny<Expression<Func<Quest, object>>>()
        ))
        .ReturnsAsync(quest);

        _grpcTeamServiceMock.Setup(gs => gs.IsUserExist(
            It.IsAny<string>(),
            It.IsAny<string>()
        ))
        .ReturnsAsync(false);

        // act
        var result = await _sut.AddUserToQuestAsync(addUserToQuestDto);

        // assert
        _grpcTeamServiceMock.Verify(
            gs => gs.IsUserExist(addUserToQuestDto.UserId!, addUserToQuestDto.TeamId!),
            Times.Once,
            "IsUserExist should be called once to verify the user is in the team"
        );
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never, "SaveChangesAsync should not be called when the user is not in the team");
        result.IsSuccess.Should().BeFalse();
        result.ErrorList.Should().Contain(UserQuestsErrors.UserNotInTeam, "the error list must include UserNotInTeam when the user is not a member of the team");
    }

    [Fact]
    public async Task GetUserAssignedQuestsAsync_WithValidUserId_ShouldReturnListOfQuestToReturnDto()
    {
        // arrange
        var userId = "user1";
        var userQuests = FakeUserQuests.GetFakeUserQuests()
            .RuleFor(uq => uq.UserId, f => userId)
            .RuleFor(uq => uq.Quest, f => FakeQuests.GetFakeQuest().Generate())
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

    [Fact]
    public async Task UnAssignUserFromQuestAsync_WithValidDate_ShouldReturnUserNotAssignedToQuestError()
    {
        // arrange
        var userQuest = FakeUserQuests.GetFakeUserQuests().Generate();
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
