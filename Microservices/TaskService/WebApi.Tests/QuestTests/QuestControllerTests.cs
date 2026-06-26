using Core.Dtos;
using Core.Models;
using Core.Result;
using FluentAssertions;
using Infrastructure.Context;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebApi.Tests.UserQuestsTests;

namespace WebApi.Tests.QuestTests;

public class QuestControllerTests : IClassFixture<TestingWebApplicationFactory<Program>>
{
    private readonly string _baseUrl = "api/tasks";
    private readonly HttpClient _client;
    private readonly TestingWebApplicationFactory<Program> _factory;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
        PropertyNameCaseInsensitive = true
    };


    public QuestControllerTests(TestingWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        _client.DefaultRequestHeaders.Add("Test-Authorization", "test-user-id");
    }
    #region Create Task
    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public async Task CreateTask_WhenDateIsValid_ReturnOk(int seed)
    {
        // arrange
        var planId = seed % 2 == 0 ? "plan1" : "plan2";
        var questDto = QuestFakers.QuestDto(seed).Generate();
        var questToReturnDto = QuestFakers.QuestToReturnDto(seed).Generate();
        // act
        var response = await _client.PostAsJsonAsync($"{_baseUrl}/{planId}/add-task", questDto);
        // assert
        response.Should().Be200Ok();

        var result = await response.Content.ReadFromJsonAsync<QuestToReturnDto>(_jsonOptions);

        result
            .Should()
            .BeEquivalentTo(questToReturnDto, opt =>
                opt
                .Excluding(q => q.QuestId)
                .Excluding(q => q.Duration)
                .Excluding(q => q.StartDate)
                .Excluding(q => q.DeadLine)
            );

        result!.QuestId.Should().NotBeNullOrWhiteSpace();
        result.Duration.Should().BeCloseTo(questToReturnDto.Duration, TimeSpan.FromSeconds(3));
        result.StartDate.Should().BeCloseTo(questToReturnDto.StartDate, TimeSpan.FromSeconds(3));
        result.DeadLine.Should().BeCloseTo(questToReturnDto.DeadLine, TimeSpan.FromSeconds(3));
    }
    [Fact]
    public async Task CreateTask_WhenDataIsInvalid_RetrunBadRequest()
    {
        // arrange
        var planId = "plan1";
        var questDto = QuestFakers.QuestDto().Generate(QuestFakers.InValid);
        var errors = new Error[]
        {
            new("Title", "Title is required."),
            new("Description", "The length of 'Description' must be 255 characters or fewer. You entered 300 characters."),
            new("Status", "Status must be between 0 and 3. 0: pending 1: active 2: completed 3: action_required"),
            new("StartDate", "Start date must be in the future."),
            new("DeadLine", "Deadline must be in the future and after the start date.")
        };

        // act
        var response = await _client.PostAsJsonAsync($"{_baseUrl}/{planId}/add-task", questDto);
        // assert
        response.Should().Be400BadRequest();
        response.Should().BeAs(errors);
    }
    #endregion

    #region Get All Tasks
    [Fact]
    public async Task GetAllTaskAsync_WithValidPlanIdAndQuestsAssignedToUsers_ReturnOKWithListOfTasksIncludedUserQuests()
    {
        // arrange
        var quests = QuestFakers.Quest().Generate(5);
        var userQuests = UserQuestFakers
            .UserQuests()
            .RuleFor(uq => uq.QuestId, f =>
                quests.First().QuestId
            )
            .Generate(2);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Quests.AddRangeAsync(quests);
            await db.SaveChangesAsync();
        }
        var planId = quests.First().ModuleId;
        // act 
        var response = await _client.GetAsync($"{_baseUrl}/{planId}/tasks");
        // assert
        response.Should().Be200Ok();

        var result = await response.Content.ReadFromJsonAsync<IEnumerable<QuestToReturnDto>>(_jsonOptions);

        result
            .Should()
            .NotBeEmpty()
            .And
            .OnlyContain(x => x.ModuleId == planId);
        result
            .Should()
            .Contain(x => x.QuestId == quests.First().QuestId);

        result.Should().HaveCount(quests.Count);
    }
    [Fact]
    public async Task GetAllTaskAsync_WhenThereIsNoTaskWithRequiredPlanId_ReturnOKWithEmptyList()
    {
        // arrange
        var planId = "nonExistingPlanId";
        var quests = QuestFakers.Quest().Generate(5);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Quests.AddRangeAsync(quests);
            await db.SaveChangesAsync();
        }
        // act 
        var response = await _client.GetAsync($"{_baseUrl}/{planId}/tasks");
        // assert
        response.Should().Be200Ok();
        var result = await response.Content.ReadFromJsonAsync<IEnumerable<QuestToReturnDto>>(_jsonOptions);
        result.Should().BeEmpty();
    }
    #endregion

    #region Get All Taks By Status 
    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public async Task GetAllTasksByStatusAsync_WithValidStatus_ReturnOkWithListOfTasks(int seed)
    {
        // arrange
        var quests = QuestFakers.Quest(seed).Generate(6);
        var status = quests.First().Status;
        var questsCount = quests.Count(q => q.Status == status);
        var planId = quests.First().ModuleId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Quests.AddRangeAsync(quests);
            await db.SaveChangesAsync();
        }
        // act 
        var response = await _client.GetAsync($"{_baseUrl}/{planId}/by-status?status={(int)status}");
        // assert
        response.Should().Be200Ok();
        var result = await response.Content.ReadFromJsonAsync<IEnumerable<QuestToReturnDto>>(_jsonOptions);

        result
            .Should()
            .HaveCount(questsCount)
            .And
            .OnlyContain(q => q.Status == status && q.ModuleId == planId);
    }
    [Fact]
    public async Task GetAllTasksByStatusAsync_WithInValidStatus_ReturnBadRequest()
    {
        // arrange
        var planId = "plan1";
        var status = 5; // invalid status
        var quests = QuestFakers.Quest().Generate(3);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Quests.AddRangeAsync(quests);
            await db.SaveChangesAsync();
        }
        // act 
        var response = await _client.GetAsync($"{_baseUrl}/{planId}/by-status?status={status}");
        response.Should().Be400BadRequest();
        var errors = new Error[]
        {
            new("Status", "Status must be between 0 and 3. 0: pending 1: active 2: completed 3: action_required")
        };
        response.Should().BeAs(errors);
    }
    [Fact]
    public async Task GetAllTasksByStatusAsync_WithNullStatus_ReturnOkWithAllTasks()
    {
        // arrange
        var planId = "plan1";
        var quests = QuestFakers.Quest().Generate(5);
        var questsCount = quests.Count(q => q.ModuleId == planId);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Quests.AddRangeAsync(quests);
            await db.SaveChangesAsync();
        }
        // act 
        var response = await _client.GetAsync($"{_baseUrl}/{planId}/by-status");
        // assert
        response.Should().Be200Ok();
        var result = await response.Content.ReadFromJsonAsync<IEnumerable<QuestToReturnDto>>(_jsonOptions);
        result
            .Should()
            .HaveCount(questsCount)
            .And
            .OnlyContain(q => q.ModuleId == planId);
    }
    #endregion

    #region Edit Task

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public async Task EditTaskAsync_WithValidData_ReturnOk(int seed)
    {
        // arrange
        var quest = QuestFakers.Quest(seed).Generate();
        var updateQuest = QuestFakers.UpdateQuestDto(seed).Generate();
        var validQuestAfterUpdate = new QuestToReturnDto
        {
            QuestId = quest.QuestId,
            ModuleId = quest.ModuleId,
            Title = updateQuest.Title ?? quest.Title,
            Description = updateQuest.Description ?? quest.Description,
            Status = updateQuest.Status == null ? quest.Status : (QuestStatusEnum)updateQuest.Status,
            StartDate = updateQuest.StartDate ?? quest.StartDate,
            DeadLine = updateQuest.DeadLine ?? quest.DeadLine,
        };
        validQuestAfterUpdate.Duration = validQuestAfterUpdate.DeadLine - validQuestAfterUpdate.StartDate;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Quests.AddAsync(quest);
            await db.SaveChangesAsync();
        }
        // act
        var request = await _client.PatchAsJsonAsync($"{_baseUrl}/update/{quest.QuestId}", updateQuest);
        // assert
        request.Should().Be200Ok();
        var result = await request.Content.ReadFromJsonAsync<QuestToReturnDto>(_jsonOptions);
        result
            .Should()
            .BeEquivalentTo(validQuestAfterUpdate, opt =>
                opt
                .Excluding(q => q.Duration)
                .Excluding(q => q.StartDate)
                .Excluding(q => q.DeadLine)
            );
        result!.Duration.Should().BeCloseTo(validQuestAfterUpdate.Duration, TimeSpan.FromSeconds(3));
        result.StartDate.Should().BeCloseTo(validQuestAfterUpdate.StartDate, TimeSpan.FromSeconds(3));
        result.DeadLine.Should().BeCloseTo(validQuestAfterUpdate.DeadLine, TimeSpan.FromSeconds(3));
    }
    [Fact]
    public async Task EditTaskAsync_WithInValidData_ReturnBadRequest()
    {
        // arrange
        var quest = QuestFakers.Quest().Generate();
        var updateQuest = QuestFakers.UpdateQuestDto().Generate(QuestFakers.InValid);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Quests.AddAsync(quest);
            await db.SaveChangesAsync();
        }
        var errors = new Error[]
        {
            new("Title", "Title must not exceed 100 characters."),
            new("Description", "The length of 'Description' must be 255 characters or fewer. You entered 300 characters."),
            new("Status", "Status must be between 0 and 3. 0: pending 1: active 2: completed 3: action_required"),
            new("StartDate", "Start date must be in the future."),
            new("DeadLine", "Deadline must be in the future and after the start date.")
        };
        // act
        var request = await _client.PatchAsJsonAsync($"{_baseUrl}/update/{quest.QuestId}", updateQuest);
        // assert
        request.Should().Be400BadRequest();
        request.Should().BeAs(errors);
    }
    [Fact]
    public async Task EditTaskAsync_WhenTaskIsNotFound_ReturnNotFound()
    {
        // arrange
        var updateQuest = QuestFakers.UpdateQuestDto(6).Generate();
        var nonExistingQuestId = "nonExistingQuestId";
        // act
        var request = await _client.PatchAsJsonAsync($"{_baseUrl}/update/{nonExistingQuestId}", updateQuest);
        // assert
        request.Should().Be404NotFound();
        var errors = new Error[]
        {
            new("Task.NotFound", "The specified Task was not found.")
        };
        request.Should().BeAs(errors);
    }
    #endregion

    #region Delete Task
    [Fact]
    public async Task DeleteTaskAsync_WhenTaskIsExisted_ReturnNoContent()
    {
        // arrange
        var quest = QuestFakers.Quest(seed: 4).Generate();
        var questId = quest.QuestId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Quests.AddAsync(quest);
            await db.SaveChangesAsync();
        }
        // act
        var response = await _client.DeleteAsync($"{_baseUrl}/delete/{questId}");
        // assert
        response.Should().Be204NoContent();
    }
    [Fact]
    public async Task DeleteTaskAsync_WhenTaskIsNotFound_ReturnNotFound()
    {
        // arrange
        var nonExistingQuestId = "nonExistingQuestId";
        // act
        var response = await _client.DeleteAsync($"{_baseUrl}/delete/{nonExistingQuestId}");
        // assert
        response.Should().Be404NotFound();
        var errors = new Error[]
        {
            new("Task.NotFound", "The specified Task was not found.")
        };
        response.Should().BeAs(errors);
    }
    #endregion
}
