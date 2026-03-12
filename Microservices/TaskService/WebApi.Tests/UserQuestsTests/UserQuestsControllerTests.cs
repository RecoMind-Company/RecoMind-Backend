using Core.Dtos;
using Core.Result;
using FluentAssertions;
using Infrastructure.Context;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebApi.Tests.QuestTests;

namespace WebApi.Tests.UserQuestsTests;

public class UserQuestsControllerTests : IClassFixture<TestingWebApplicationFactory<Program>>
{
    private const string BaseUrl = "api/user-tasks";
    private readonly TestingWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
        PropertyNameCaseInsensitive = true
    };
    public UserQuestsControllerTests(TestingWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("Test-Authorization", "test-user");
    }
    #region Add User To Task
    [Fact]
    public async Task AddUserToTaskAsync_withValidData_ReturnOk()
    {
        // arrange
        var quest = QuestFakers.Quest(seed: 4).Generate();
        var addUserToQuestDto = UserQuestFakers.AddUserToQuest(seed: 4).RuleFor(auq => auq.QuestId, f => quest.QuestId).Generate();
        var questToReturn = QuestFakers.QuestToReturnDto(seed: 4).RuleFor(q => q.UserAssignedQuests, f => [addUserToQuestDto.UserId]).Generate();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Quests.AddAsync(quest);
            await db.SaveChangesAsync();
        }
        // act
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/add-user-to-task", addUserToQuestDto);
        // assert
        response.Should().Be200Ok();
        var result = await response.Content.ReadFromJsonAsync<QuestToReturnDto>(_jsonOptions);
        //result!.UserAssignedQuests.Should().Contain(addUserToQuestDto.UserId);
        result
            .Should()
            .BeEquivalentTo(questToReturn, options => options
                .Excluding(q => q.StartDate)
                .Excluding(q => q.DeadLine)
                .Excluding(q => q.Duration)
            );
        result.StartDate.Should().BeCloseTo(quest.StartDate, TimeSpan.FromSeconds(5));
        result.DeadLine.Should().BeCloseTo(quest.DeadLine, TimeSpan.FromSeconds(5));
        result.Duration.Should().BeCloseTo(quest.Duration, TimeSpan.FromSeconds(5));
    }
    [Fact]
    public async Task AddUserToTaskAsync_WithInValidRequest_ReturnBadRequest()
    {
        // arrange
        var addUserToQuestDto = UserQuestFakers.AddUserToQuest(4).Generate(UserQuestFakers.InValid);
        var errors = new List<Error>()
        {
            new("UserId", "UserId is required."),
            new("QuestId", "QuestId is required."),
            new("TeamId", "TeamId is required.")
        };
        // act
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/add-user-to-task", addUserToQuestDto);
        // assert
        response.Should().Be400BadRequest();
        response.Should().BeAs(errors);
    }
    [Fact]
    public async Task AddUserToTaskAsync_WhenTaskIsNotFound_ReturnNotFound()
    {
        // arrange
        var addUserToQuestDto = UserQuestFakers.AddUserToQuest(seed: 4).Generate();
        var errors = new List<Error>()
        {
            new("Task.NotFound", "The specified Task was not found.")
        };
        // act
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/add-user-to-task", addUserToQuestDto);
        // assert 
        response.Should().Be404NotFound();
        response.Should().BeAs(errors);
    }
    [Fact]
    public async Task AddUserToTaskAsync_WhenUserIsAlreadyExisted_ReturnBadRequest()
    {
        // arrange
        var quest = QuestFakers.Quest(seed: 4).Generate();

        var userWithQuest = UserQuestFakers
                            .UserQuests(seed: 4)
                            .RuleFor(uq => uq.QuestId, f => quest.QuestId)
                            .RuleFor(uq => uq.Quest, f => quest)
                            .Generate();

        var AddUserToQuestDto = UserQuestFakers
                                .AddUserToQuest(seed: 4)
                                .RuleFor(uq => uq.QuestId, f => quest.QuestId)
                                .Generate();
        var errors = new List<Error>()
        {
            new("Task.UserAlreadyAssigned", "The user is already assigned to this Task.")
        };
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.UserQuests.AddAsync(userWithQuest);
            await db.SaveChangesAsync();
        }
        // act
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/add-user-to-task", AddUserToQuestDto);
        // assert
        response.Should().Be400BadRequest();
        response.Should().BeAs(errors);
    }
    #endregion

    #region Get User Assigned Tasks 
    [Fact]
    public async Task GetUserAssignedTasksAsync_WhenAuthorizedAndUserIsAssignedToTasks_ReturnsOk()
    {
        // arrange
        var userId = "test-user";
        var quests = QuestFakers.Quest(seed: 4).Generate(4);
        var questsIds = quests.Select(q => q.QuestId).ToList();
        var userQuests = UserQuestFakers
                        .UserQuests(seed: 4)
                        .RuleFor(uq => uq.UserId, f => userId)
                        .RuleFor(uq => uq.QuestId, f => questsIds[f.IndexVariable++])
                        .Generate(3);
        var questToReturnDtos = QuestFakers.QuestToReturnDto(seed: 4)
                                .RuleFor(q => q.UserAssignedQuests, f => [userId])
                                .Generate();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Quests.AddRangeAsync(quests);
            await db.UserQuests.AddRangeAsync(userQuests);
            await db.SaveChangesAsync();
        }

        // act
        var response = await _client.GetAsync($"{BaseUrl}/user-tasks");
        // assert
        response.Should().Be200Ok();
        var result = await response.Content.ReadFromJsonAsync<List<QuestToReturnDto>>(_jsonOptions);
        result.Should().HaveCount(3);
        result.Select(q => q.QuestId).Should().NotContain(questsIds.Last());
        result.Select(q => q.UserAssignedQuests).Should().AllSatisfy(uaq => uaq.Should().Contain(userId));
    }
    [Fact]
    public async Task GetUserAssignedTaskAsync_WhenUserIsUnAuthorized_ReturnUnAuthorized()
    {
        // arrange
        var specificClient = _factory.CreateClient();
        // act
        var response = await specificClient.GetAsync($"{BaseUrl}/user-tasks");
        // assert
        response.Should().Be401Unauthorized();
    }
    [Fact]
    public async Task GetUserAssignedTaskAsync_WhenUserHasNoAssignedQuests_ReturnOkWithEmptyList()
    {
        // arrange
        var quests = QuestFakers.Quest(seed: 4).Generate(4);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Quests.AddRangeAsync(quests);
            await db.SaveChangesAsync();
        }
        // act
        var response = await _client.GetAsync($"{BaseUrl}/user-tasks");
        // assert
        response.Should().Be200Ok();
        var result = await response.Content.ReadFromJsonAsync<List<QuestToReturnDto>>(_jsonOptions);
        result.Should().HaveCount(0);
    }
    #endregion
}
