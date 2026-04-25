namespace Core.Tests;

using Core.DTOs;
using Core.DTOs.AI;
using Core.Interfaces;
using Core.Models;
using Core.Services;
using FluentAssertions;
using Moq;
using Xunit;

public class ReportServiceTests
{
    private readonly Mock<IGenerateReportService> _mockGenerateReportService;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IReportRepository> _mockReportRepository;
    private readonly Mock<IFileStorageService> _mockFileStorageService;
    private readonly Mock<IGrpcTeamService> _mockGrpcTeamService;
    private readonly Mock<IDataAssignService> _mockDataAssignService;
    private readonly ReportService _reportService;

    public ReportServiceTests()
    {
        _mockGenerateReportService = new Mock<IGenerateReportService>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockReportRepository = new Mock<IReportRepository>();
        _mockFileStorageService = new Mock<IFileStorageService>();
        _mockGrpcTeamService = new Mock<IGrpcTeamService>();
        _mockDataAssignService = new Mock<IDataAssignService>();

        _reportService = new ReportService(
            _mockGenerateReportService.Object,
            _mockUnitOfWork.Object,
            _mockReportRepository.Object,
            _mockFileStorageService.Object,
            _mockGrpcTeamService.Object,
            _mockDataAssignService.Object
        );
    }

    #region GetUserData Tests

    [Fact]
    public async Task GetUserData_WithValidUserId_ReturnsTeamToReturnDto()
    {
        // Arrange
        var userId = "user-123";
        var teamDetails = new TeamToReturnDto
        {
            TeamName = "Test Team",
            TeamId = "team-123",
            CompanyId = "company-123"
        };

        _mockGrpcTeamService
            .Setup(s => s.GetTeamByUserId(userId))
            .ReturnsAsync(teamDetails);

        // Act
        var result = await _reportService.GetUserData(userId);

        // Assert
        result.Should().NotBeNull();
        result.TeamName.Should().Be("Test Team");
        result.TeamId.Should().Be("team-123");
        result.CompanyId.Should().Be("company-123");
        _mockGrpcTeamService.Verify(s => s.GetTeamByUserId(userId), Times.Once);
    }

    [Fact]
    public async Task GetUserData_WithNullTeamDetails_ReturnsNull()
    {
        // Arrange
        var userId = "user-456";

        _mockGrpcTeamService
            .Setup(s => s.GetTeamByUserId(userId))
            .ReturnsAsync((TeamToReturnDto)null!);

        // Act
        var result = await _reportService.GetUserData(userId);

        // Assert
        result.Should().BeNull();
        _mockGrpcTeamService.Verify(s => s.GetTeamByUserId(userId), Times.Once);
    }

    #endregion

    #region CreateReportByAi Tests

    [Fact]
    public async Task CreateReportByAi_WithValidRequest_ReturnsAnalysisResponseDto()
    {
        // Arrange
        var analysisRequest = new AnalysisRequestDto
        {
            CompanyId = "company-123",
            UserRequest = "Generate sales analysis",
            TeamName = "Sales Team"
        };

        var expectedResponse = new AnalysisResponseDto
        {
            TaskId = "task-123",
            Status = "PENDING",
            Message = "Report generation started"
        };

        _mockGenerateReportService
            .Setup(s => s.GenerateReport(analysisRequest))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _reportService.CreateReportByAi(analysisRequest);

        // Assert
        result.Should().NotBeNull();
        result.TaskId.Should().Be("task-123");
        result.Status.Should().Be("PENDING");
        result.Message.Should().Be("Report generation started");
        _mockGenerateReportService.Verify(s => s.GenerateReport(analysisRequest), Times.Once);
    }

    #endregion

    #region GetReportFromAi Tests

    [Fact]
    public async Task GetReportFromAi_WithPendingStatus_ReturnsInProgressMessage()
    {
        // Arrange
        var reportFromAiDto = new GetReportFromAiDto
        {
            TaskId = "task-123",
            TeamId = "team-123",
            UserId = "user-123",
            Periodic = "Monthly"
        };

        var taskStatus = new TaskStatusResponseDto
        {
            TaskId = "task-123",
            Status = Status.PENDING,
            Result = null
        };

        _mockGenerateReportService
            .Setup(s => s.GetStatus(reportFromAiDto.TaskId))
            .ReturnsAsync(taskStatus);

        // Act
        var result = await _reportService.GetReportFromAi(reportFromAiDto);

        // Assert
        result.Should().NotBeNull();
        result.message.Should().Be("your report is being generated.");
        _mockGenerateReportService.Verify(s => s.GetStatus(reportFromAiDto.TaskId), Times.Once);
    }

    [Fact]
    public async Task GetReportFromAi_WithProgressStatus_ReturnsInProgressMessage()
    {
        // Arrange
        var reportFromAiDto = new GetReportFromAiDto
        {
            TaskId = "task-123",
            TeamId = "team-123",
            UserId = "user-123",
            Periodic = "Weekly"
        };

        var taskStatus = new TaskStatusResponseDto
        {
            TaskId = "task-123",
            Status = Status.PROGRESS,
            Result = null
        };

        _mockGenerateReportService
            .Setup(s => s.GetStatus(reportFromAiDto.TaskId))
            .ReturnsAsync(taskStatus);

        // Act
        var result = await _reportService.GetReportFromAi(reportFromAiDto);

        // Assert
        result.Should().NotBeNull();
        result.message.Should().Be("your report is being generated.");
        _mockGenerateReportService.Verify(s => s.GetStatus(reportFromAiDto.TaskId), Times.Once);
    }

    [Fact]
    public async Task GetReportFromAi_WithSuccessStatus_SavesReportAndReturnsResult()
    {
        // Arrange
        var reportFromAiDto = new GetReportFromAiDto
        {
            TaskId = "task-123",
            TeamId = "team-123",
            UserId = "user-123",
            Periodic = "Quarterly"
        };

        var reportContent = "Generated report content";
        var dynamicPath = "/reports/report-123.txt";

        var taskStatus = new TaskStatusResponseDto
        {
            TaskId = "task-123",
            Status = Status.SUCCESS,
            Result = reportContent
        };

        _mockGenerateReportService
            .Setup(s => s.GetStatus(reportFromAiDto.TaskId))
            .ReturnsAsync(taskStatus);

        _mockFileStorageService
            .Setup(s => s.SaveFileAsync(reportContent))
            .ReturnsAsync(dynamicPath);

        _mockReportRepository
            .Setup(r => r.AddAsync(It.IsAny<Report>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.Save())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _reportService.GetReportFromAi(reportFromAiDto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.AiResponse.Should().Be(reportContent);
        result.GeneratedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        _mockFileStorageService.Verify(s => s.SaveFileAsync(reportContent), Times.Once);
        _mockReportRepository.Verify(r => r.AddAsync(It.IsAny<Report>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
    }

    [Fact]
    public async Task GetReportFromAi_WithSuccessStatus_CreatesReportWithCorrectPeriodic()
    {
        // Arrange
        var reportFromAiDto = new GetReportFromAiDto
        {
            TaskId = "task-123",
            TeamId = "team-123",
            UserId = "user-123",
            Periodic = "Yearly"
        };

        var reportContent = "Report content";
        var dynamicPath = "/reports/report-123.txt";

        var taskStatus = new TaskStatusResponseDto
        {
            TaskId = "task-123",
            Status = Status.SUCCESS,
            Result = reportContent
        };

        _mockGenerateReportService
            .Setup(s => s.GetStatus(reportFromAiDto.TaskId))
            .ReturnsAsync(taskStatus);

        _mockFileStorageService
            .Setup(s => s.SaveFileAsync(reportContent))
            .ReturnsAsync(dynamicPath);

        Report capturedReport = null!;
        _mockReportRepository
            .Setup(r => r.AddAsync(It.IsAny<Report>()))
            .Callback<Report>(r => capturedReport = r)
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.Save())
            .Returns(Task.CompletedTask);

        // Act
        await _reportService.GetReportFromAi(reportFromAiDto);

        // Assert
        capturedReport.Should().NotBeNull();
        capturedReport.Periodic.Should().Be(Periodic.Yearly);
        capturedReport.TeamId.Should().Be("team-123");
        capturedReport.UserId.Should().Be("user-123");
        capturedReport.FilePath.Should().Be(dynamicPath);
        capturedReport.FileType.Should().Be(".txt");
    }

    #endregion

    #region GetReportById Tests

    [Fact]
    public async Task GetReportById_WithValidId_ReturnsAiReportResponseDto()
    {
        // Arrange
        var reportId = "report-123";
        var reportContent = "Report content here";
        var generatedDate = DateTime.Now.AddDays(-1);

        var report = new Report
        {
            Id = reportId,
            TeamId = "team-123",
            UserId = "user-123",
            FilePath = "/reports/report-123.txt",
            FileType = ".txt",
            GeneratedDate = generatedDate,
            Periodic = Periodic.Monthly
        };

        _mockReportRepository
            .Setup(r => r.GetByIdAsync(reportId))
            .ReturnsAsync(report);

        _mockFileStorageService
            .Setup(f => f.ReadFileAsync(report.FilePath))
            .ReturnsAsync(reportContent);

        // Act
        var result = await _reportService.GetReportById(reportId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.AiResponse.Should().Be(reportContent);
        result.GeneratedDate.Should().Be(generatedDate);

        _mockReportRepository.Verify(r => r.GetByIdAsync(reportId), Times.Once);
        _mockFileStorageService.Verify(f => f.ReadFileAsync(report.FilePath), Times.Once);
    }

    [Fact]
    public async Task GetReportById_WithNonExistentId_ReturnsErrorMessage()
    {
        // Arrange
        var reportId = "non-existent-id";

        _mockReportRepository
            .Setup(r => r.GetByIdAsync(reportId))
            .ReturnsAsync((Report)null!);

        // Act
        var result = await _reportService.GetReportById(reportId);

        // Assert
        result.Should().NotBeNull();
        result.message.Should().Be("there is no report with this id");

        _mockReportRepository.Verify(r => r.GetByIdAsync(reportId), Times.Once);
        _mockFileStorageService.Verify(f => f.ReadFileAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetReportById_WithEmptyFileContent_ReturnsErrorMessage()
    {
        // Arrange
        var reportId = "report-123";
        var report = new Report
        {
            Id = reportId,
            TeamId = "team-123",
            UserId = "user-123",
            FilePath = "/reports/report-123.txt",
            FileType = ".txt",
            GeneratedDate = DateTime.Now,
            Periodic = Periodic.Weekly
        };

        _mockReportRepository
            .Setup(r => r.GetByIdAsync(reportId))
            .ReturnsAsync(report);

        _mockFileStorageService
            .Setup(f => f.ReadFileAsync(report.FilePath))
            .ReturnsAsync(string.Empty);

        // Act
        var result = await _reportService.GetReportById(reportId);

        // Assert
        result.Should().NotBeNull();
        result.message.Should().Be("there is no content for this report");

        _mockReportRepository.Verify(r => r.GetByIdAsync(reportId), Times.Once);
        _mockFileStorageService.Verify(f => f.ReadFileAsync(report.FilePath), Times.Once);
    }

    [Fact]
    public async Task GetReportById_WithNullFileContent_ReturnsErrorMessage()
    {
        // Arrange
        var reportId = "report-123";
        var report = new Report
        {
            Id = reportId,
            TeamId = "team-123",
            UserId = "user-123",
            FilePath = "/reports/report-123.txt",
            FileType = ".txt",
            GeneratedDate = DateTime.Now,
            Periodic = Periodic.Monthly
        };

        _mockReportRepository
            .Setup(r => r.GetByIdAsync(reportId))
            .ReturnsAsync(report);

        _mockFileStorageService
            .Setup(f => f.ReadFileAsync(report.FilePath))
            .ReturnsAsync((string)null!);

        // Act
        var result = await _reportService.GetReportById(reportId);

        // Assert
        result.Should().NotBeNull();
        result.message.Should().Be("there is no content for this report");
    }

    #endregion

    #region DeleteReport Tests

    [Fact]
    public async Task DeleteReport_WithValidId_DeletesReportAndReturnsSuccessMessage()
    {
        // Arrange
        var reportId = "report-123";
        var report = new Report
        {
            Id = reportId,
            TeamId = "team-123",
            UserId = "user-123",
            FilePath = "/reports/report-123.txt",
            FileType = ".txt",
            GeneratedDate = DateTime.Now,
            Periodic = Periodic.Monthly
        };

        _mockReportRepository
            .Setup(r => r.GetByIdAsync(reportId))
            .ReturnsAsync(report);

        _mockReportRepository
            .Setup(r => r.Delete(report))
            .Callback<Report>(r => { /* Mock delete */ });

        _mockUnitOfWork
            .Setup(u => u.Save())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _reportService.DeleteReport(reportId);

        // Assert
        result.Should().Be("Report deleted successfully");

        _mockReportRepository.Verify(r => r.GetByIdAsync(reportId), Times.Once);
        _mockReportRepository.Verify(r => r.Delete(report), Times.Once);
        _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
    }

    [Fact]
    public async Task DeleteReport_WithNonExistentId_ReturnsEmptyString()
    {
        // Arrange
        var reportId = "non-existent-id";

        _mockReportRepository
            .Setup(r => r.GetByIdAsync(reportId))
            .ReturnsAsync((Report)null!);

        // Act
        var result = await _reportService.DeleteReport(reportId);

        // Assert
        result.Should().Be("");
    }

    #endregion

    #region GetAllReportsByTeamId Tests

    [Fact]
    public async Task GetAllReportsByTeamId_WithValidTeamId_ReturnsReportDtos()
    {
        // Arrange
        var teamId = "team-123";
        var reports = new List<Report>
        {
            new Report
            {
                Id = "report-1",
                TeamId = teamId,
                UserId = "user-123",
                FilePath = "/reports/report-1.txt",
                FileType = ".txt",
                GeneratedDate = DateTime.Now.AddDays(-1),
                Periodic = Periodic.Weekly
            },
            new Report
            {
                Id = "report-2",
                TeamId = teamId,
                UserId = "user-456",
                FilePath = "/reports/report-2.txt",
                FileType = ".txt",
                GeneratedDate = DateTime.Now.AddDays(-2),
                Periodic = Periodic.Monthly
            }
        };

        _mockReportRepository
            .Setup(r => r.FindAll(It.IsAny<System.Linq.Expressions.Expression<System.Func<Report, bool>>>()))
            .ReturnsAsync(reports);

        // Act
        var result = await _reportService.GetAllReportsByTeamId(teamId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        var reportDtoList = result.ToList();
        reportDtoList[0].Id.Should().Be("report-1");
        reportDtoList[0].TeamId.Should().Be(teamId);
        reportDtoList[0].Periodic.Should().Be("Weekly");
        reportDtoList[1].Id.Should().Be("report-2");
        reportDtoList[1].Periodic.Should().Be("Monthly");

        _mockReportRepository.Verify(
            r => r.FindAll(It.IsAny<System.Linq.Expressions.Expression<System.Func<Report, bool>>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllReportsByTeamId_WithNoReports_ReturnsEmptyEnumerable()
    {
        // Arrange
        var teamId = "team-no-reports";

        _mockReportRepository
            .Setup(r => r.FindAll(It.IsAny<System.Linq.Expressions.Expression<System.Func<Report, bool>>>()))
            .ReturnsAsync(new List<Report>());

        // Act
        var result = await _reportService.GetAllReportsByTeamId(teamId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        _mockReportRepository.Verify(
            r => r.FindAll(It.IsAny<System.Linq.Expressions.Expression<System.Func<Report, bool>>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllReportsByTeamId_MapsReportsToReportDtos()
    {
        // Arrange
        var teamId = "team-123";
        var generatedDate = new DateTime(2024, 1, 15, 10, 30, 0);
        var reports = new List<Report>
        {
            new Report
            {
                Id = "report-123",
                TeamId = teamId,
                UserId = "user-789",
                FilePath = "/reports/report.txt",
                FileType = ".txt",
                GeneratedDate = generatedDate,
                Periodic = Periodic.Quarterly
            }
        };

        _mockReportRepository
            .Setup(r => r.FindAll(It.IsAny<System.Linq.Expressions.Expression<System.Func<Report, bool>>>()))
            .ReturnsAsync(reports);

        // Act
        var result = await _reportService.GetAllReportsByTeamId(teamId);

        // Assert
        var reportDtoList = result.ToList();
        reportDtoList.Should().HaveCount(1);

        var reportDto = reportDtoList[0];
        reportDto.Id.Should().Be("report-123");
        reportDto.TeamId.Should().Be(teamId);
        reportDto.UserId.Should().Be("user-789");
        reportDto.Periodic.Should().Be("Quarterly");
        reportDto.GeneratedDate.Should().Be(generatedDate);
        reportDto.Content.Should().BeNull(); // Content is not fetched in the current implementation
    }

    #endregion

    #region AssignCompanyData Tests

    [Fact]
    public async Task AssignCompanyData_WithValidCompanyId_ReturnsTaskId()
    {
        // Arrange
        var companyId = "company-123";
        var expectedTaskId = "task-assign-123";

        _mockDataAssignService
            .Setup(s => s.DataAssign(companyId))
            .ReturnsAsync(expectedTaskId);

        // Act
        var result = await _reportService.AssignCompanyData(companyId);

        // Assert
        result.Should().Be(expectedTaskId);

        _mockDataAssignService.Verify(s => s.DataAssign(companyId), Times.Once);
    }

    #endregion

    #region GetAssignCompanyDataStatus Tests

    [Fact]
    public async Task GetAssignCompanyDataStatus_WithValidTaskId_ReturnsStatus()
    {
        // Arrange
        var taskId = "task-assign-123";
        var expectedStatus = "COMPLETED";

        _mockDataAssignService
            .Setup(s => s.DataAssignResult(taskId))
            .ReturnsAsync(expectedStatus);

        // Act
        var result = await _reportService.GetAssignCompanyDataStatus(taskId);

        // Assert
        result.Should().Be(expectedStatus);

        _mockDataAssignService.Verify(s => s.DataAssignResult(taskId), Times.Once);
    }

    #endregion
}