using AutoMapper;
using FluentAssertions;
using Moq;
using Team.Core.DTOs;
using Team.Core.Interfaces;
using Team.Core.Models;
using Team.Core.Result;
using Team.Core.Services;

namespace Team.Tests.Services
{
    public class TeamServiceTests
    {
        private readonly Mock<ITeamRepository> _repoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IAuthGrpcService> _grpcMock;
        private readonly TeamService _teamService;

        public TeamServiceTests()
        {
            _repoMock = new Mock<ITeamRepository>();
            _mapperMock = new Mock<IMapper>();
            _grpcMock = new Mock<IAuthGrpcService>();

            _teamService = new TeamService(_grpcMock.Object, _repoMock.Object, _mapperMock.Object);
        }

        // Helper: builds a minimal valid TeamModel belonging to a given company
        private static TeamModel MakeTeam(string id = "t1", string companyId = "c1", string name = "Team A")
            => new TeamModel { Id = id, CompanyId = companyId, Name = name };

        // Create
        #region CreateTeamAsync

        [Fact]
        public async Task CreateTeamAsync_ShouldReturnNameAlreadyExists_WhenNameIsTaken()
        {
            // Arrange
            const string companyId = "comp-123";
            var dto = new CreateTeamDto { Name = "Existing Team" };

            _repoMock.Setup(r => r.ExistsByNameAsync(companyId, dto.Name))
                     .ReturnsAsync(true);

            // Act
            var result = await _teamService.CreateTeamAsync(companyId, dto);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(TeamErrors.NameAlreadyExists.Code);

            _repoMock.Verify(r => r.CreateAsync(It.IsAny<TeamModel>()), Times.Never);
        }

        [Fact]
        public async Task CreateTeamAsync_ShouldReturnMappedResponse_WhenNameIsAvailable()
        {
            // Arrange
            const string companyId = "comp-123";
            var dto = new CreateTeamDto { Name = "New Team" };
            var teamModel = new TeamModel { Name = dto.Name };
            var expectedResponse = new TeamResponseDto { Name = dto.Name };

            _repoMock.Setup(r => r.ExistsByNameAsync(companyId, dto.Name)).ReturnsAsync(false);
            _mapperMock.Setup(m => m.Map<TeamModel>(dto)).Returns(teamModel);
            _mapperMock.Setup(m => m.Map<TeamResponseDto>(teamModel)).Returns(expectedResponse);

            // the service correctly sets Id, CompanyId, and CreatedAt after mapping.
            TeamModel? capturedTeam = null;
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<TeamModel>()))
                     .Callback<TeamModel>(t => capturedTeam = t);

            // Act
            var result = await _teamService.CreateTeamAsync(companyId, dto);

            // Assert — returned DTO
            result.IsSuccess.Should().BeTrue();
            result.Value.Name.Should().Be(dto.Name);

            // Assert — business rules applied to the model before persistence
            _repoMock.Verify(r => r.CreateAsync(It.IsAny<TeamModel>()), Times.Once);
            capturedTeam.Should().NotBeNull();
            capturedTeam!.CompanyId.Should().Be(companyId);
            capturedTeam.Id.Should().NotBeNullOrWhiteSpace();
            capturedTeam.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        #endregion

        // Read
        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNotFound_WhenTeamDoesNotExist()
        {
            // Arrange
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<string>()))
                     .ReturnsAsync((TeamModel?)null);

            // Act
            var result = await _teamService.GetByIdAsync("invalid-id");

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(TeamErrors.NotFound.Code);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnMappedDto_WhenTeamExists()
        {
            // Arrange
            var team = MakeTeam();
            var expectedDto = new TeamResponseDto { Id = team.Id, Name = team.Name };

            _repoMock.Setup(r => r.GetByIdAsync(team.Id)).ReturnsAsync(team);
            _mapperMock.Setup(m => m.Map<TeamResponseDto>(team)).Returns(expectedDto);

            // Act
            var result = await _teamService.GetByIdAsync(team.Id);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(expectedDto);
        }

        #endregion

        #region GetTeamByTeamLeadIdAsync

        [Fact]
        public async Task GetTeamByTeamLeadIdAsync_ShouldReturnNotFound_WhenTeamDoesNotExist()
        {
            // Arrange
            _repoMock.Setup(r => r.GetTeamByTeamLeadIdAsync(It.IsAny<string>()))
                     .ReturnsAsync((TeamModel?)null);

            // Act
            var result = await _teamService.GetTeamByTeamLeadIdAsync("lead-1");

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(TeamErrors.NotFound.Code);
        }

        [Fact]
        public async Task GetTeamByTeamLeadIdAsync_ShouldReturnUserTeamInfo_WhenTeamExists()
        {
            // Arrange
            var team = MakeTeam();
            _repoMock.Setup(r => r.GetTeamByTeamLeadIdAsync("lead-1")).ReturnsAsync(team);

            // Act
            var result = await _teamService.GetTeamByTeamLeadIdAsync("lead-1");

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.TeamId.Should().Be(team.Id);
            result.Value.CompanyId.Should().Be(team.CompanyId);
            result.Value.TeamName.Should().Be(team.Name);
        }

        #endregion

        #region GetTeamByEmployeeIdAsync

        [Fact]
        public async Task GetTeamByEmployeeIdAsync_ShouldReturnNotFound_WhenEmployeeHasNoTeam()
        {
            // Arrange
            _repoMock.Setup(r => r.GetTeamByEmployeeIdAsync(It.IsAny<string>()))
                     .ReturnsAsync((TeamModel?)null);

            // Act
            var result = await _teamService.GetTeamByEmployeeIdAsync("emp-99");

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(TeamErrors.NotFound.Code);
        }

        #endregion

        #region GetByCompanyIdAsync / GetForAiAsync

        [Fact]
        public async Task GetForAiAsync_ShouldReturnEmptyList_WhenRepositoryReturnsNull()
        {
            // Arrange
            _repoMock.Setup(r => r.GetByCompanyIdAsync(It.IsAny<string>()))
                     .ReturnsAsync((List<TeamModel>?)null);

            // Act
            var result = await _teamService.GetForAiAsync("comp-1");

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByCompanyIdAsync_ShouldReturnEmptyList_WhenRepositoryReturnsNull()
        {
            // Arrange
            _repoMock.Setup(r => r.GetByCompanyIdAsync(It.IsAny<string>()))
                     .ReturnsAsync((List<TeamModel>?)null);

            // Act
            var result = await _teamService.GetByCompanyIdAsync("comp-1");

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        // Update
        #region UpdateTeamAsync

        [Fact]
        public async Task UpdateTeamAsync_ShouldReturnNotFound_WhenTeamBelongsToADifferentCompany()
        {
            // Arrange
            var existingTeam = MakeTeam(companyId: "original-comp");
            _repoMock.Setup(r => r.GetByIdAsync("t1")).ReturnsAsync(existingTeam);

            // Act
            var result = await _teamService.UpdateTeamAsync("t1", "different-comp", new UpdateTeamDto());

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(TeamErrors.NotFound.Code);
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<TeamModel>()), Times.Never);
        }

        [Fact]
        public async Task UpdateTeamAsync_ShouldReturnNotFound_WhenTeamDoesNotExist()
        {
            // Arrange
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<string>()))
                     .ReturnsAsync((TeamModel?)null);

            // Act
            var result = await _teamService.UpdateTeamAsync("t1", "c1", new UpdateTeamDto());

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(TeamErrors.NotFound.Code);
        }

        [Fact]
        public async Task UpdateTeamAsync_ShouldReturnNameAlreadyExists_WhenNewNameIsTakenByAnotherTeam()
        {
            // Arrange
            var existingTeam = MakeTeam(name: "Old Name");
            var updateDto = new UpdateTeamDto { Name = "Taken Name" };

            _repoMock.Setup(r => r.GetByIdAsync("t1")).ReturnsAsync(existingTeam);
            _repoMock.Setup(r => r.ExistsByNameAsync("c1", "Taken Name")).ReturnsAsync(true);

            // Act
            var result = await _teamService.UpdateTeamAsync("t1", "c1", updateDto);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(TeamErrors.NameAlreadyExists.Code);
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<TeamModel>()), Times.Never);
        }

        [Fact]
        public async Task UpdateTeamAsync_ShouldNotCheckNameUniqueness_WhenNameIsUnchanged()
        {
            // Arrange
            var existingTeam = MakeTeam(name: "Same Name");
            var updateDto = new UpdateTeamDto { Name = "Same Name" };
            var expectedDto = new TeamResponseDto { Name = "Same Name" };

            _repoMock.Setup(r => r.GetByIdAsync("t1")).ReturnsAsync(existingTeam);
            _mapperMock.Setup(m => m.Map<TeamResponseDto>(existingTeam)).Returns(expectedDto);

            // Act
            var result = await _teamService.UpdateTeamAsync("t1", "c1", updateDto);

            // Assert
            result.IsSuccess.Should().BeTrue();
            _repoMock.Verify(r => r.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task UpdateTeamAsync_ShouldNotCheckNameUniqueness_WhenDtoNameIsNull()
        {
            // Arrange
            var existingTeam = MakeTeam(name: "Existing Name");
            var updateDto = new UpdateTeamDto { Name = null };
            var expectedDto = new TeamResponseDto { Name = "Existing Name" };

            _repoMock.Setup(r => r.GetByIdAsync("t1")).ReturnsAsync(existingTeam);
            _mapperMock.Setup(m => m.Map<TeamResponseDto>(existingTeam)).Returns(expectedDto);

            // Act
            var result = await _teamService.UpdateTeamAsync("t1", "c1", updateDto);

            // Assert
            result.IsSuccess.Should().BeTrue();
            _repoMock.Verify(r => r.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task UpdateTeamAsync_ShouldSetUpdatedAtAndPersist_WhenDataIsValid()
        {
            // Arrange
            var existingTeam = MakeTeam(name: "Old Name");
            var updateDto = new UpdateTeamDto { Name = "New Name" };
            var expectedDto = new TeamResponseDto { Name = "New Name" };

            _repoMock.Setup(r => r.GetByIdAsync("t1")).ReturnsAsync(existingTeam);
            _repoMock.Setup(r => r.ExistsByNameAsync("c1", "New Name")).ReturnsAsync(false);
            _mapperMock.Setup(m => m.Map(updateDto, existingTeam));
            _mapperMock.Setup(m => m.Map<TeamResponseDto>(existingTeam)).Returns(expectedDto);

            TeamModel? capturedTeam = null;
            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<TeamModel>()))
                     .Callback<TeamModel>(t => capturedTeam = t);

            // Act
            var result = await _teamService.UpdateTeamAsync("t1", "c1", updateDto);

            // Assert — returned DTO
            result.IsSuccess.Should().BeTrue();
            result.Value.Name.Should().Be("New Name");

            // Assert — UpdatedAt timestamp was set by the service before persisting
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<TeamModel>()), Times.Once);
            capturedTeam.Should().NotBeNull();
            capturedTeam!.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        #endregion

        // Delete
        #region DeleteTeamAsync

        [Fact]
        public async Task DeleteTeamAsync_ShouldReturnSuccess_WhenTeamExistsAndBelongsToCompany()
        {
            // Arrange
            var team = MakeTeam();
            _repoMock.Setup(r => r.GetByIdAsync("t1")).ReturnsAsync(team);
            _repoMock.Setup(r => r.DeleteAsync("t1")).ReturnsAsync(true);

            // Act
            var result = await _teamService.DeleteTeamAsync("t1", "c1");

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeTrue();
            _repoMock.Verify(r => r.DeleteAsync("t1"), Times.Once);
        }

        // FIX #6: the not-found / wrong-company path for delete was missing.
        [Fact]
        public async Task DeleteTeamAsync_ShouldReturnNotFound_WhenTeamBelongsToADifferentCompany()
        {
            // Arrange
            var team = MakeTeam(companyId: "other-comp");
            _repoMock.Setup(r => r.GetByIdAsync("t1")).ReturnsAsync(team);

            // Act
            var result = await _teamService.DeleteTeamAsync("t1", "c1");

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(TeamErrors.NotFound.Code);
            _repoMock.Verify(r => r.DeleteAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task DeleteTeamAsync_ShouldReturnNotFound_WhenTeamDoesNotExist()
        {
            // Arrange
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<string>()))
                     .ReturnsAsync((TeamModel?)null);

            // Act
            var result = await _teamService.DeleteTeamAsync("t1", "c1");

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(TeamErrors.NotFound.Code);
            _repoMock.Verify(r => r.DeleteAsync(It.IsAny<string>()), Times.Never);
        }

        #endregion

        // Employee Management
        #region AddEmployeeAsync

        [Fact]
        public async Task AddEmployeeAsync_ShouldReturnNotFound_WhenTeamDoesNotBelongToCompany()
        {
            // Arrange
            var team = MakeTeam(companyId: "other-comp");
            _repoMock.Setup(r => r.GetByIdAsync("t1")).ReturnsAsync(team);

            // Act
            var result = await _teamService.AddEmployeeAsync("t1", "c1", "emp-1");

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(TeamErrors.NotFound.Code);
            _repoMock.Verify(r => r.AddEmployeeToTeamAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AddEmployeeAsync_ShouldReturnEmployeeAlreadyInTeam_WhenEmployeeIsAlreadyMember()
        {
            // Arrange
            var team = MakeTeam();
            _repoMock.Setup(r => r.GetByIdAsync("t1")).ReturnsAsync(team);
            _repoMock.Setup(r => r.IsEmployeeInTeam("t1", "emp-1")).ReturnsAsync(true);

            // Act
            var result = await _teamService.AddEmployeeAsync("t1", "c1", "emp-1");

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(TeamErrors.EmployeeAlreadyInTeam.Code);
            _repoMock.Verify(r => r.AddEmployeeToTeamAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AddEmployeeAsync_ShouldReturnSuccess_WhenEmployeeIsNotAlreadyInTeam()
        {
            // Arrange
            var team = MakeTeam();
            _repoMock.Setup(r => r.GetByIdAsync("t1")).ReturnsAsync(team);
            _repoMock.Setup(r => r.IsEmployeeInTeam("t1", "emp-1")).ReturnsAsync(false);
            _repoMock.Setup(r => r.AddEmployeeToTeamAsync("t1", "emp-1")).ReturnsAsync(true);

            // Act
            var result = await _teamService.AddEmployeeAsync("t1", "c1", "emp-1");

            // Assert
            result.IsSuccess.Should().BeTrue();
            _repoMock.Verify(r => r.AddEmployeeToTeamAsync("t1", "emp-1"), Times.Once);
        }

        #endregion

        #region RemoveEmployeeAsync

        [Fact]
        public async Task RemoveEmployeeAsync_ShouldReturnNotFound_WhenTeamDoesNotExist()
        {
            // Arrange
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<string>()))
                     .ReturnsAsync((TeamModel?)null);

            // Act
            var result = await _teamService.RemoveEmployeeAsync("t1", "c1", "emp-1");

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(TeamErrors.NotFound.Code);
            _repoMock.Verify(r => r.RemoveEmployeeFromTeamAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RemoveEmployeeAsync_ShouldReturnNotFound_WhenTeamBelongsToADifferentCompany()
        {
            // Arrange
            var team = MakeTeam(companyId: "other-comp");
            _repoMock.Setup(r => r.GetByIdAsync("t1")).ReturnsAsync(team);

            // Act
            var result = await _teamService.RemoveEmployeeAsync("t1", "c1", "emp-1");

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(TeamErrors.NotFound.Code);
            _repoMock.Verify(r => r.RemoveEmployeeFromTeamAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RemoveEmployeeAsync_ShouldReturnSuccess_WhenTeamAndCompanyAreValid()
        {
            // Arrange
            var team = MakeTeam();
            _repoMock.Setup(r => r.GetByIdAsync("t1")).ReturnsAsync(team);
            _repoMock.Setup(r => r.RemoveEmployeeFromTeamAsync("t1", "emp-1")).ReturnsAsync(true);

            // Act
            var result = await _teamService.RemoveEmployeeAsync("t1", "c1", "emp-1");

            // Assert
            result.IsSuccess.Should().BeTrue();
            _repoMock.Verify(r => r.RemoveEmployeeFromTeamAsync("t1", "emp-1"), Times.Once);
        }

        #endregion

        // Job Titles (gRPC)
        #region GetTeamMemberJobTitlesAsync

        [Fact]
        public async Task GetTeamMemberJobTitlesAsync_ShouldReturnEmpty_WhenTeamHasNoMembers()
        {
            // Arrange
            _repoMock.Setup(r => r.IsTeamBelongsToCompanyAsync("t1", "c1")).ReturnsAsync(true);
            _repoMock.Setup(r => r.GetTeamMemberIdsAsync("t1")).ReturnsAsync(new List<string>());

            // Act
            var result = await _teamService.GetTeamMemberJobTitlesAsync("t1", "c1");

            // Assert
            result.Should().BeEmpty();
            _grpcMock.Verify(g => g.GetTeamEmployeesJobTitlesAsync(It.IsAny<List<string>>()), Times.Never);
        }

        [Fact]
        public async Task GetTeamMemberJobTitlesAsync_ShouldReturnEmpty_WhenTeamDoesNotBelongToCompany()
        {
            // Arrange
            _repoMock.Setup(r => r.IsTeamBelongsToCompanyAsync("t1", "c1")).ReturnsAsync(false);
            _repoMock.Setup(r => r.GetTeamMemberIdsAsync("t1")).ReturnsAsync(new List<string> { "e1" });

            // Act
            var result = await _teamService.GetTeamMemberJobTitlesAsync("t1", "c1");

            // Assert
            result.Should().BeEmpty();
            _grpcMock.Verify(g => g.GetTeamEmployeesJobTitlesAsync(It.IsAny<List<string>>()), Times.Never);
        }

        [Fact]
        public async Task GetTeamMemberJobTitlesAsync_ShouldReturnEmpty_WhenGrpcCallFails()
        {
            // Arrange
            var employeeIds = new List<string> { "e1" };
            var failureResult = Result<List<UserJobTitleDto>>.Failure(
                new Error("gRPC call failed", "AuthService.Error"));

            _repoMock.Setup(r => r.IsTeamBelongsToCompanyAsync("t1", "c1")).ReturnsAsync(true);
            _repoMock.Setup(r => r.GetTeamMemberIdsAsync("t1")).ReturnsAsync(employeeIds);

            _grpcMock.Setup(g => g.GetTeamEmployeesJobTitlesAsync(It.IsAny<List<string>>()))
                     .ReturnsAsync(failureResult);

            // Act
            var result = await _teamService.GetTeamMemberJobTitlesAsync("t1", "c1");

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetTeamMemberJobTitlesAsync_ShouldReturnNonEmptyUserJobTitles_FromGrpc()
        {
            // Arrange
            var employeeIds = new List<string> { "e1", "e2", "e3", "e4" };
            var grpcPayload = new List<UserJobTitleDto>
            {
                new() { UserId = "e1", JobTitle = "Junior Developer" },
                new() { UserId = "e2", JobTitle = "Senior Developer" },
                new() { UserId = "e3", JobTitle = "Manager" },
                new() { UserId = "e4", JobTitle = "" }           // blank — must be excluded
            };
            var grpcResponse = Result<List<UserJobTitleDto>>.Success(grpcPayload);

            _repoMock.Setup(r => r.IsTeamBelongsToCompanyAsync("t1", "c1")).ReturnsAsync(true);
            _repoMock.Setup(r => r.GetTeamMemberIdsAsync("t1")).ReturnsAsync(employeeIds);

            _grpcMock.Setup(g => g.GetTeamEmployeesJobTitlesAsync(It.IsAny<List<string>>()))
                     .ReturnsAsync(grpcResponse);

            // Act
            var result = await _teamService.GetTeamMemberJobTitlesAsync("t1", "c1");

            // Assert
            result.Should().HaveCount(3);

            result.Should().ContainEquivalentOf(new UserJobTitleDto
            {
                UserId = "e1",
                JobTitle = "Junior Developer"
            });

            result.Should().ContainEquivalentOf(new UserJobTitleDto
            {
                UserId = "e2",
                JobTitle = "Senior Developer"
            });

            result.Should().ContainEquivalentOf(new UserJobTitleDto
            {
                UserId = "e3",
                JobTitle = "Manager"
            });

            result.Should().NotContain(x => string.IsNullOrWhiteSpace(x.JobTitle));
        }

        [Fact]
        public async Task GetTeamMemberJobTitlesAsync_ShouldReturnEmpty_WhenGrpcReturnsNullValue()
        {
            // Arrange
            var employeeIds = new List<string> { "e1" };
            var grpcResponse = Result<List<UserJobTitleDto>>.Success(null!);

            _repoMock.Setup(r => r.IsTeamBelongsToCompanyAsync("t1", "c1")).ReturnsAsync(true);
            _repoMock.Setup(r => r.GetTeamMemberIdsAsync("t1")).ReturnsAsync(employeeIds);
            _grpcMock.Setup(g => g.GetTeamEmployeesJobTitlesAsync(It.IsAny<List<string>>()))
                     .ReturnsAsync(grpcResponse);

            // Act
            var result = await _teamService.GetTeamMemberJobTitlesAsync("t1", "c1");

            // Assert
            result.Should().BeEmpty();
        }

        #endregion
    }
}