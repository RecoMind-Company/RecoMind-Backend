using Core.Dtos;
using Core.Result;
using FluentAssertions;
using Infrastructure.Context;
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
        _client = factory.CreateClient();
    }
    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public async Task CreateTask_WhenDateIsValid_ReturnOk(int seed)
    {
        // arrange
        var planId = "plan1";
        var QuestDto = QuestFakers.QuestDto(seed).Generate();
        var questToReturnDto = QuestFakers.QuestToRetrunDto(seed).Generate();
        // act
        var response = await _client.PostAsJsonAsync($"{_baseUrl}/{planId}/add-task", QuestDto);
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
    [Fact]
    public async Task GetAllTaskAsync_WithValidPlanIdAndQuestsAssignedToUsers_ReturnOKWithListOfTasksIncludedUserQuests()
    {
        // arrange
        var quests = QuestFakers.Quest().Generate(5);
        var userQuests = UserQuestFaker
            .UserQuests()
            .RuleFor(uq => uq.QuestId, f =>
                quests.First().QuestId
            )
            .Generate(2);
        using var scope = _factory.Services.CreateScope();
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Quests.AddRangeAsync(quests);
            await db.SaveChangesAsync();
        }
        var planId = quests.First().PlanId;
        // act 
        var response = await _client.GetAsync($"{_baseUrl}/{planId}/tasks");
        // assert
        response.Should().Be200Ok();

        var result = await response.Content.ReadFromJsonAsync<IEnumerable<QuestToReturnDto>>(_jsonOptions);

        result
            .Should()
            .NotBeEmpty()
            .And
            .OnlyContain(x => x.PlanId == planId);
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
        using var scope = _factory.Services.CreateScope();
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
}
