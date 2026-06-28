using AutoMapper;
using Core.Dtos.Plan;
using Core.Interfaces;
using Core.MappingProfiles;
using Core.Models;
using Core.Result;
using Core.Services;
using Core.ServicesAbstractions;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Linq.Expressions;

namespace Core.Tests.Quests;

public class QuestServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<Quest>> _questRepositoryMock;
    private readonly Mock<IGrpcModuleService> _grpcModuleServiceMock;
    private readonly QuestService _sut;

    public QuestServiceTests()
    {
        _grpcModuleServiceMock = new Mock<IGrpcModuleService>();

        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _questRepositoryMock = new Mock<IGenericRepository<Quest>>();
        _unitOfWorkMock.Setup(u => u.GetRepository<Quest>()).Returns(_questRepositoryMock.Object);
        var loggerFactory = NullLoggerFactory.Instance;
        var config = new MapperConfiguration(
            cnfg => cnfg.AddProfile<QuestProfile>(),
            loggerFactory
        );
        var mapper = config.CreateMapper();
        _sut = new QuestService(_unitOfWorkMock.Object, mapper, _grpcModuleServiceMock.Object);
    }

    [Theory]
    [InlineData(200)] // even seed => past StartDate, expected status: pending (when Status is null)
    [InlineData(31)]  // odd  seed => today StartDate, expected status: active  (when Status is null)
    public async Task CreateQuestAsync_WithValidmoduleId_ShouldReturnQuestToReturnDto(int seed)
    {
        // arrange
        var questDto = FakeQuests.GetFakeQuestDto(seed).Generate();
        var moduleId = "module1";

        _grpcModuleServiceMock
            .Setup(g => g.GetmoduleIdsAsync(moduleId))
            .ReturnsAsync(new ModuleIdsDto { IsExisted = true, ModuleId = moduleId });

        // act
        var result = await _sut.CreateQuestAsync(questDto);

        // assert
        _grpcModuleServiceMock.Verify(
            g => g.GetmoduleIdsAsync(It.IsAny<string>()),
            Times.Once,
            "GetmoduleIdsAsync should be called exactly once to validate the module exists");

        _questRepositoryMock.Verify(
            r => r.AddAsync(It.Is<Quest>(q => q.ModuleId == moduleId && !string.IsNullOrEmpty(q.QuestId))),
            Times.Once,
            "AddAsync should be called exactly once with the correct moduleId and a non-empty QuestId");

        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(),
            Times.Once,
            "SaveChangesAsync should be called once when a new quest is created");

        result.Should().NotBeNull("CreateQuestAsync should return a result object, not null");
        result.IsSuccess.Should().BeTrue("CreateQuestAsync should succeed when a valid QuestDto, moduleId, and existing plan are provided");

        Guid.Parse(result.Value!.QuestId).Should().NotBeEmpty("the created quest must be assigned a valid non-empty GUID as its ID");
        result.Value.Title.Should().Be(questDto.Title, "the returned title must match the submitted DTO title");
        result.Value.Description.Should().Be(questDto.Description, "the returned description must match the submitted DTO description");

        result.Value.StartDate.Should().BeCloseTo(
            questDto.StartDate!.Value,
            TimeSpan.FromSeconds(1),
            "the returned start date should match the submitted DTO start date within 1 second");

        result.Value.DeadLine.Should().BeCloseTo(
            questDto.DeadLine!.Value,
            TimeSpan.FromSeconds(1),
            "the returned deadline should match the submitted DTO deadline within 1 second");

        result.Value.ModuleId.Should().Be(moduleId, "the returned moduleId must match the provided moduleId");
    }

    [Fact]
    public async Task CreateQuestAsync_WhenPlanDoesNotExist_ShouldReturnPlanNotFoundError()
    {
        // arrange
        var questDto = FakeQuests.GetFakeQuestDto(2).Generate();

        _grpcModuleServiceMock
            .Setup(g => g.GetmoduleIdsAsync(It.IsAny<string>()))
            .ReturnsAsync(new ModuleIdsDto { IsExisted = false });

        // act
        var result = await _sut.CreateQuestAsync(questDto);

        // assert
        _grpcModuleServiceMock.Verify(
            g => g.GetmoduleIdsAsync(It.IsAny<string>()),
            Times.Once,
            "GetmoduleIdsAsync should be called to validate the plan");

        _questRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Quest>()),
            Times.Never,
            "AddAsync should not be called when the plan does not exist");

        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(),
            Times.Never,
            "SaveChangesAsync should not be called when the plan does not exist");

        result.IsSuccess.Should().BeFalse("CreateQuestAsync should fail when the plan does not exist");
        result.ErrorList.Should().Contain(ModuleErrors.NotFound, "the error list must include ModuleNotFound when the module does not exist");
    }

    [Theory]
    [InlineData(234)]
    [InlineData(345)]
    [InlineData(456)]
    public async Task GetAllQuestsAsync_WithExistingmoduleId_ShouldReturnListOfQuestToReturnDto(int seed)
    {
        // arrange
        var planId = "plan1";
        var moduleId = "module1";
        var quests = FakeQuests.GetFakeQuest(seed).Generate(5);

        // Moq cannot match lambda expressions by semantic equality.
        // Use It.IsAny<> so the setup matches any predicate the SUT passes.
        _questRepositoryMock
            .Setup(r => r.FindAll(
                It.IsAny<Expression<Func<Quest, bool>>>(),
                It.IsAny<Expression<Func<Quest, object>>>()))
            .ReturnsAsync(quests);

        // act
        var result = await _sut.GetAllQuestsAsync(planId, moduleId);

        // assert
        _questRepositoryMock.Verify(
            r => r.FindAll(
                It.IsAny<Expression<Func<Quest, bool>>>(),
                It.IsAny<Expression<Func<Quest, object>>>()),
            Times.Once,
            "FindAll should be called exactly once to retrieve quests for the given moduleId");

        result.IsSuccess.Should().BeTrue("GetAllQuestsAsync should succeed when a valid moduleId with existing quests is provided");
        result.Value.Should().NotBeNull("the result value should contain a list of quests, not null");
        result.Value!.Count().Should().Be(quests.Count, "the number of returned quests must match the number of seeded quests");

        // All faked quests have moduleId == "module1" by faker definition
        result.Value.Select(q => q.ModuleId).Should().AllBeEquivalentTo(moduleId, "every returned quest must belong to the requested plan");

        result.Value.Select(q => q.UserAssignedQuests).Should().AllSatisfy(
            u => u.Should().NotBeNull("each quest must include its assigned users collection, not null"));
    }

    [Theory]
    [InlineData(567)]
    [InlineData(234)]
    [InlineData(345)]
    [InlineData(456)]
    public async Task EditQuestAsync_WhenQuestIsExisted_ReturnUpdatedQuest(int seed)
    {
        // arrange
        var questId = Guid.NewGuid().ToString();
        var updateDto = FakeQuests.GetFakeUpdateQuestDto(seed).Generate();
        var existingQuest = FakeQuests.GetFakeQuest(seed).Generate();

        _questRepositoryMock
            .Setup(r => r.Find(It.IsAny<Expression<Func<Quest, bool>>>()))
            .ReturnsAsync(existingQuest);

        // act
        var result = await _sut.EditQuestAsync(updateDto, questId);

        // assert
        _questRepositoryMock.Verify(
            r => r.Find(It.IsAny<Expression<Func<Quest, bool>>>()),
            Times.Once,
            "Find should be called once to retrieve the quest by ID");

        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(),
            Times.Once,
            "SaveChangesAsync should be called once when the quest is updated");

        result.IsSuccess.Should().BeTrue("EditQuestAsync should succeed when the quest exists");
        result.Value.Should().NotBeNull("the result value should contain the updated quest, not null");

        result.Value.Should().BeEquivalentTo(new
        {
            Title = updateDto.Title ?? existingQuest.Title,
            Description = updateDto.Description ?? existingQuest.Description,
            StartDate = updateDto.StartDate ?? existingQuest.StartDate,
            DeadLine = updateDto.DeadLine ?? existingQuest.DeadLine,
            Status = updateDto.Status != null ? (QuestStatusEnum)updateDto.Status : existingQuest.Status
        }, "each field should reflect the updated DTO value, or fall back to the existing quest value when the DTO field is null");
    }

    [Fact]
    public async Task EditQuestAsync_WhenQuestIsNotExisted_ReturnQuestNotFoundError()
    {
        // arrange
        var updateDto = FakeQuests.GetFakeUpdateQuestDto().Generate();
        var questId = Guid.NewGuid().ToString();

        _questRepositoryMock
            .Setup(r => r.Find(It.IsAny<Expression<Func<Quest, bool>>>()))
            .ReturnsAsync((Quest?)null);

        // act
        var result = await _sut.EditQuestAsync(updateDto, questId);

        // assert
        result.IsSuccess.Should().BeFalse("EditQuestAsync should fail when the quest does not exist");
        result.ErrorList.Should().Contain(QuestErrors.QuestNotFound, "the error list must include QuestNotFound when no quest matches the provided ID");
    }

    [Fact]
    public async Task DeleteQuestAsync_WhenQuestIsNotExisted_ReturnQuestNotFoundError()
    {
        // arrange
        var questId = Guid.NewGuid().ToString();

        _questRepositoryMock
            .Setup(r => r.Find(It.IsAny<Expression<Func<Quest, bool>>>()))
            .ReturnsAsync((Quest?)null);

        // act
        var result = await _sut.DeleteQuestAsync(questId);

        // assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never, "SaveChangesAsync should not be called when the quest does not exist");
        _questRepositoryMock.Verify(r => r.Delete(It.IsAny<Quest>()), Times.Never, "Delete should not be called when the quest does not exist");
        result.IsSuccess.Should().BeFalse("DeleteQuestAsync should fail when the quest does not exist");
        result.ErrorList.Should().Contain(QuestErrors.QuestNotFound, "the error list must include QuestNotFound when no quest matches the provided ID");
    }

    [Theory]
    [InlineData(567)]
    [InlineData(234)]
    [InlineData(345)]
    [InlineData(456)]
    public async Task DeleteQuestAsync_WhenQuestIsExisted_ReturnSuccess(int seed)
    {
        // arrange
        var existingQuest = FakeQuests.GetFakeQuest(seed).Generate();

        _questRepositoryMock
            .Setup(r => r.Find(It.IsAny<Expression<Func<Quest, bool>>>()))
            .ReturnsAsync(existingQuest);

        // act
        var result = await _sut.DeleteQuestAsync(existingQuest.QuestId);

        // assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once, "SaveChangesAsync should be called once when a quest is deleted");
        _questRepositoryMock.Verify(r => r.Delete(It.IsAny<Quest>()), Times.Once, "Delete should be called once with the existing quest");
        result.IsSuccess.Should().BeTrue("DeleteQuestAsync should succeed when the quest exists");
        result.Value.Should().BeTrue("the result value should be true to confirm the quest was successfully deleted");
    }

    [Theory]
    [InlineData(567)]
    [InlineData(234)]
    [InlineData(345)]
    [InlineData(456)]
    public async Task GetAllQuestsByStatusAsync_WhenQuestsWithRequiredStatusExist_ReturnListOfQuestToReturnDto(int seed)
    {
        // arrange
        var moduleId = "module1";
        var questByStatus = FakeQuests.GetFakeQuestByStatusDto(seed).Generate();
        var expectedStatus = (QuestStatusEnum)questByStatus.Status!;
        var quests = FakeQuests.GetFakeQuest(seed)
            .RuleFor(q => q.Status, _ => expectedStatus)
            .Generate(5);

        _questRepositoryMock
            .Setup(r => r.FindAll(
                It.IsAny<Expression<Func<Quest, bool>>>(),
                It.IsAny<Expression<Func<Quest, object>>>()))
            .ReturnsAsync(quests);

        // act
        var result = await _sut.GetAllQuestsByStatusAsync(questByStatus, moduleId);

        // assert
        _questRepositoryMock.Verify(
            r => r.FindAll(
                It.IsAny<Expression<Func<Quest, bool>>>(),
                It.IsAny<Expression<Func<Quest, object>>>()),
            Times.Once,
            "FindAll should be called exactly once to retrieve quests filtered by status");

        result.IsSuccess.Should().BeTrue("GetAllQuestsByStatusAsync should succeed when quests with the requested status exist");
        result.Value.Should().NotBeNull("the result value should contain a list of quests, not null");
        result.Value!.Count().Should().Be(quests.Count, "the number of returned quests must match the number of seeded quests with the requested status");

        result.Value.Select(q => q.Status).Should().AllBeEquivalentTo(
            expectedStatus,
            "every returned quest must have the requested status");

        result.Value.Select(q => q.UserAssignedQuests).Should().AllSatisfy(
            u => u.Should().NotBeNull("each quest must include its assigned users collection, not null"));
    }

    [Fact]
    public async Task GetAllQuestsByStatusAsync_WhenQuestStatusIsNull_ReturnAllQuestsOfThePlan()
    {
        // arrange
        var moduleId = "module1";
        var questByStatus = FakeQuests.GetFakeQuestByStatusDto()
            .RuleFor(q => q.Status, _ => (int?)null)
            .Generate();
        var quests = FakeQuests.GetFakeQuest().Generate(5);

        _questRepositoryMock
            .Setup(r => r.FindAll(
                It.IsAny<Expression<Func<Quest, bool>>>(),
                It.IsAny<Expression<Func<Quest, object>>>()))
            .ReturnsAsync(quests);

        // act
        var result = await _sut.GetAllQuestsByStatusAsync(questByStatus, moduleId);

        // assert
        _questRepositoryMock.Verify(
            r => r.FindAll(
                It.IsAny<Expression<Func<Quest, bool>>>(),
                It.IsAny<Expression<Func<Quest, object>>>()),
            Times.Once,
            "FindAll should be called exactly once, delegating to GetAllQuestsAsync when no status filter is provided");

        result.IsSuccess.Should().BeTrue("GetAllQuestsByStatusAsync should succeed and return all quests when no status filter is provided");
        result.Value.Should().NotBeNull("the result value should contain a list of quests, not null");
        result.Value!.Count().Should().Be(quests.Count, "all quests belonging to the plan should be returned when no status filter is applied");

        result.Value.Select(q => q.UserAssignedQuests).Should().AllSatisfy(
            u => u.Should().NotBeNull("each quest must include its assigned users collection, not null"));
    }
}
