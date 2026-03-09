using AutoMapper;
using Core.Interfaces;
using Core.MappingProfiles;
using Core.Models;
using Core.Result;
using Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Linq.Expressions;

namespace Core.Tests.Quests;

public class QuestServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<Quest>> _questRepositoryMock;
    private readonly QuestService _sut;
    public QuestServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _questRepositoryMock = new Mock<IGenericRepository<Quest>>();
        _unitOfWorkMock.Setup(u => u.GetRepository<Quest>()).Returns(_questRepositoryMock.Object);
        var loggerFactory = NullLoggerFactory.Instance;
        var config = new MapperConfiguration(
            cnfg =>
            {
                cnfg.AddProfile<QuestProfile>();
            },
            loggerFactory
        );
        var mapper = config.CreateMapper();
        _sut = new QuestService(_unitOfWorkMock.Object, mapper);
    }

    [Theory]
    [InlineData(100)]
    [InlineData(200)]
    [InlineData(30)]
    public async Task CreateQuestAsync_WithValidDate_ShouldReturnQuestToReturnDto(int seed)
    {
        // arrange
        var planId = "plan1";
        var questDto = FakeQuests.GetFakeQuestDto(seed).Generate();
        // act
        var result = await _sut.CreateQuestAsync(questDto, planId);
        // assert
        _questRepositoryMock.Verify
        (
            u => u.AddAsync
            (
                It.Is<Quest>(q => q.PlanId == planId && !string.IsNullOrEmpty(q.QuestId))
            ),
            Times.Once
        );
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once, "SaveChangesAsync should be called once when a new quest is created");
        result.Should().NotBeNull("CreateQuestAsync should return a result object, not null");
        result.IsSuccess.Should().BeTrue("CreateQuestAsync should succeed when a valid QuestDto and planId are provided");
        Guid.Parse(result.Value!.QuestId).Should().NotBeEmpty("the created quest must be assigned a valid non-empty GUID as its ID");

        result.Value.Title.Should().Be(questDto.Title, "the returned quest title must match the submitted DTO title");
        result.Value.Description.Should().Be(questDto.Description, "the returned quest description must match the submitted DTO description");
        result.Value.StartDate.Should().BeCloseTo(questDto.StartDate!.Value, TimeSpan.FromSeconds(1), "the returned start date should match the submitted DTO start date within a 1-second tolerance");
        result.Value.DeadLine.Should().BeCloseTo(questDto.DeadLine!.Value, TimeSpan.FromSeconds(1), "the returned deadline should match the submitted DTO deadline within a 1-second tolerance");
        if (questDto.Status != null)
        {
            result.Value.Status.Should().Be((QuestStatusEnum)questDto.Status.Value, "the returned status must match the explicitly provided status in the DTO");
        }
        else
        {
            var expectedStatus = (questDto.StartDate ?? DateTime.UtcNow).Date == DateTime.UtcNow.Date
                                 ? QuestStatusEnum.active
                                 : QuestStatusEnum.pending;

            result.Value.Status.Should().Be(expectedStatus, "when no status is provided, the quest should be 'active' if it starts today, or 'pending' if it starts in the future");
        }
    }

    [Theory]
    [InlineData(234)]
    [InlineData(345)]
    [InlineData(456)]
    public async Task GetAllQuestsAsync_WithExistingPlanId_ShouldReturnListOfQuestToReturnDto(int seed)
    {
        // arrange
        var quests = FakeQuests.GetFakeQuest(seed).Generate(5);
        var planId = "plan1";
        _questRepositoryMock.Setup(r => r.FindAll(It.IsAny<Expression<Func<Quest, bool>>>(), x => x.UserAssignedQuests)).ReturnsAsync(quests);
        // act
        var result = await _sut.GetAllQuestsAsync(planId);
        // assert
        _questRepositoryMock.Verify(r => r.FindAll(x => x.PlanId == planId, x => x.UserAssignedQuests), Times.Once, "FindAll should be called once with the correct plan ID");
        result.IsSuccess.Should().BeTrue("GetAllQuestsAsync should succeed when a valid planId with existing quests is provided");
        result.Value.Should().NotBeNull("the result value should contain a list of quests, not null");
        result.Value!.Count().Should().Be(quests.Count, "the number of returned quests must match the number of quests seeded for the plan");
        result.Value.Select(q => q.PlanId).Should().AllBeEquivalentTo(planId, "every returned quest must belong to the requested plan");
        result.Value.Select(q => q.UserAssignedQuests).Should().AllSatisfy(u => u.Should().NotBeNull("each quest must include its assigned users collection, not null"));
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
        _questRepositoryMock.Setup(r => r.Find(It.IsAny<Expression<Func<Quest, bool>>>())).ReturnsAsync(existingQuest);
        // act
        var result = await _sut.EditQuestAsync(updateDto, questId);
        // assert
        _questRepositoryMock.Verify(r => r.Find(It.IsAny<Expression<Func<Quest, bool>>>()), Times.Once, "Find should be called once with the correct quest ID");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once, "SaveChangesAsync should be called once when the quest is updated");
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
        _questRepositoryMock.Setup(r => r.Find(It.IsAny<Expression<Func<Quest, bool>>>())).ReturnsAsync((Quest?)null);
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
        _questRepositoryMock.Setup(r => r.Find(It.IsAny<Expression<Func<Quest, bool>>>())).ReturnsAsync((Quest?)null);
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
        _questRepositoryMock.Setup(r => r.Find(It.IsAny<Expression<Func<Quest, bool>>>())).ReturnsAsync(existingQuest);
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
        var planId = "plan1";
        var questByStatus = FakeQuests.GetFakeQuestByStatusDto(seed).Generate();
        var quests = FakeQuests.GetFakeQuest(seed).RuleFor(q => q.Status, f => (QuestStatusEnum)questByStatus.Status!).Generate(5);
        _questRepositoryMock.Setup(r => r.FindAll(q => q.Status == (QuestStatusEnum)questByStatus.Status!, q => q.UserAssignedQuests)).ReturnsAsync(quests);
        // act
        var result = await _sut.GetAllQuestsByStatusAsync(questByStatus, planId);
        // assert
        result.IsSuccess.Should().BeTrue("GetAllQuestsByStatusAsync should succeed when quests with the requested status exist");
        result.Value.Should().NotBeNull("the result value should contain a list of quests, not null");
        result.Value!.Count().Should().Be(quests.Count, "the number of returned quests must match the number of quests seeded with the requested status");
        result.Value.Select(q => q.Status).Should().AllBeEquivalentTo((QuestStatusEnum)questByStatus.Status!, "every returned quest must have the requested status");
        result.Value.Select(x => x.UserAssignedQuests).Should().AllSatisfy(u => u.Should().NotBeNull("each quest must include its assigned users collection, not null"));
    }

    [Fact]
    public async Task GetAllQuestsByStatusAsync_WhenQuestStatusIsNull_ReturnAllQuestsOfThePlan()
    {
        // arrange
        var planId = "plan1";
        var questByStatus = FakeQuests.GetFakeQuestByStatusDto().RuleFor(q => q.Status, f => null).Generate();
        var quests = FakeQuests.GetFakeQuest().Generate(5);
        _questRepositoryMock.Setup(r => r.FindAll(x => x.PlanId == planId, x => x.UserAssignedQuests)).ReturnsAsync(quests);
        // act
        var result = await _sut.GetAllQuestsByStatusAsync(questByStatus, planId);
        // assert
        result.IsSuccess.Should().BeTrue("GetAllQuestsByStatusAsync should succeed and return all quests when no status filter is provided");
        result.Value.Should().NotBeNull("the result value should contain a list of quests, not null");
        result.Value!.Count().Should().Be(quests.Count, "all quests belonging to the plan should be returned when no status filter is applied");
        result.Value.Select(q => q.PlanId).Should().AllBeEquivalentTo(planId, "every returned quest must belong to the requested plan");
        result.Value.Select(x => x.UserAssignedQuests).Should().AllSatisfy(u => u.Should().NotBeNull("each quest must include its assigned users collection, not null"));
    }
}
