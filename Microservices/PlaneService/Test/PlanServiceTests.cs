using AutoMapper;
using Core.DTOs.PlanDtos;
using Core.DTOs.PlnaTypeDtos;
using Core.Interfaces;
using Core.Models;
using Core.Service;
using Core.Service.Interface;
using Core.Service.Interface.AI;
using Infrastructure.GrpcClients.Team;
using Moq;
using System.Linq.Expressions;

namespace Test
{
    public class PlanServiceTests
    {
        private readonly Mock<IUnitOfWork<Plan>> _unitOfWorkMock;
        private readonly Mock<IGenericRepository<Plan>> _repoMock;
        private readonly Mock<IPlanType> _planTypeServiceMock;
        private readonly Mock<IStatus> _statusServiceMock;
        private readonly Mock<ITeamGrpcClient> _teamGrpcClientMock;
        private readonly Mock<IPlanEventPublisher> _planEventPublisherMock;
        private readonly Mock<IPlanGeneratorService> _planGeneratorService;
        private readonly Mock<IQuestGrpcClient> _questGrpcClientMock;
        private readonly IMapper _mapper;
        private readonly PlanService _planService;

        public PlanServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork<Plan>>();
            _repoMock = new Mock<IGenericRepository<Plan>>();
            _repoMock.Setup(r => r.AddAsync(It.IsAny<Plan>())).ReturnsAsync((Plan p) => p);
            _unitOfWorkMock.Setup(u => u.Entity).Returns(_repoMock.Object);
            _planTypeServiceMock = new Mock<IPlanType>();
            _statusServiceMock = new Mock<IStatus>();
            _teamGrpcClientMock = new Mock<ITeamGrpcClient>();
            _planEventPublisherMock = new Mock<IPlanEventPublisher>();
            _planGeneratorService = new Mock<IPlanGeneratorService>();
            _questGrpcClientMock = new Mock<IQuestGrpcClient>();

            var config = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.AddProfile<Core.Mapping.PlanMapper>();
            }, Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);

            _mapper = config.CreateMapper();

            _planService = new PlanService(
                _unitOfWorkMock.Object,
                _mapper,
                _statusServiceMock.Object,
                _planTypeServiceMock.Object,
                _teamGrpcClientMock.Object,
                _planEventPublisherMock.Object,
                _planGeneratorService.Object,
                _questGrpcClientMock.Object
            );
        }

        #region Create plan tests
        [Fact]
        public async Task CreateNewPlanAsync_WhenValidPlanIsProvided_ShouldCreatePlan()
        {
            // Arrange
            var dto = new AddPlanDto { PlanType = "Monthly" };
            var planType = new GetPlanTypeDto { NumOfMonths = 1 };

            _planTypeServiceMock.Setup(x => x.GetPlanTypeByName(It.IsAny<string>()))
                .ReturnsAsync(planType);
            _teamGrpcClientMock.Setup(x => x.GetTeamNameById(It.IsAny<string>()))
                .ReturnsAsync(Result<string>.Success("TeamA"));

            // Act
            var result = await _planService.CreatePlan(dto, "comp1", "user1");

            // Assert
            Assert.True(result.IsSuccess);
            _unitOfWorkMock.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public async Task CreateNewPlanAsync_WhenInvalidPlanIsProvided_ShouldReturnError()
        {
            // Arrange: PlanType not found
            _planTypeServiceMock.Setup(x => x.GetPlanTypeByName(It.IsAny<string>()))
                .ReturnsAsync((GetPlanTypeDto)null);

            // Act
            var result = await _planService.CreatePlan(new AddPlanDto { PlanType = "None" }, "c", "u");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Invalid plan type provided.", result.Error);
        }

        [Fact]
        public async Task CreateNewPlanAsync_WhenValidPlanTypeIsProvided_ShouldCreatePlan()
        {
            // Arrange
            var planType = new GetPlanTypeDto { NumOfMonths = 12 };
            _planTypeServiceMock.Setup(x => x.GetPlanTypeByName("yearly")).ReturnsAsync(planType);
            _teamGrpcClientMock.Setup(x => x.GetTeamNameById(It.IsAny<string>())).ReturnsAsync(Result<string>.Success("T1"));

            // Act
            var result = await _planService.CreatePlan(new AddPlanDto { PlanType = "yearly" }, "c", "u");

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task GetPlanAsync_WhenInValidPlanTypeIsProvided_ShouldReturnError()
        {
            // Arrange
            _planTypeServiceMock.Setup(x => x.GetPlanTypeByName(It.IsAny<string>())).ReturnsAsync((GetPlanTypeDto)null);

            // Act
            var result = await _planService.GetPlanEndDate("fake");

            // Assert
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task CreateNewPlanAsync_WhenUserIsPartOfATeam_ShouldCreatePlan()
        {
            // Arrange
            _planTypeServiceMock.Setup(x => x.GetPlanTypeByName(It.IsAny<string>())).ReturnsAsync(new GetPlanTypeDto { NumOfMonths = 1 });
            _teamGrpcClientMock.Setup(x => x.GetTeamNameById(It.IsAny<string>())).ReturnsAsync(Result<string>.Success("MyTeam"));

            // Act
            var result = await _planService.CreatePlan(new AddPlanDto { PlanType = "Monthly" }, "c", "u");

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task CreateNewPlanAsync_WhenUserIsNotPartOfATeam_ShouldReturnError()
        {
            // Arrange
            _planTypeServiceMock.Setup(x => x.GetPlanTypeByName(It.IsAny<string>())).ReturnsAsync(new GetPlanTypeDto { NumOfMonths = 1 });
            _teamGrpcClientMock.Setup(x => x.GetTeamNameById(It.IsAny<string>())).ReturnsAsync(Result<string>.Failure("No Team Found"));

            // Act
            var result = await _planService.CreatePlan(new AddPlanDto { PlanType = "Monthly" }, "c", "u");

            // Assert
            Assert.False(result.IsSuccess);
        }
        #endregion

        #region Get plan tests
        [Fact]
        public async Task GetPlanAsync_WhenValidPlanIdIsProvided_ShouldReturnPlan()
        {
            // Arrange
            var plan = new Plan { Id = "1", Company_Id = "C1" };
            _repoMock.Setup(x => x.Find(It.IsAny<Expression<Func<Plan, bool>>>())).ReturnsAsync(plan);
            _repoMock.Setup(x => x.AddAsync(It.IsAny<Plan>())).ReturnsAsync((Plan p) => p);

            // Act
            var result = await _planService.GetPlanById("1", "C1");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
        }

        [Fact]
        public async Task GetPlanAsync_WhenInValidPlanIdIsProvided_ShouldReturnError()
        {
            // Arrange
            _unitOfWorkMock.Setup(x => x.Entity.Find(It.IsAny<Expression<Func<Plan, bool>>>())).ReturnsAsync((Plan)null);

            // Act
            var result = await _planService.GetPlanById("999", "C1");

            // Assert
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task GetAllPlansAsync_WhenPlansExist_ShouldReturnListOfPlans()
        {
            // Arrange
            var plans = new List<Plan> { new Plan { Id = "1" }, new Plan { Id = "2" } };
            _unitOfWorkMock.Setup(x => x.Entity.FindAll(It.IsAny<Expression<Func<Plan, bool>>>())).ReturnsAsync(plans);

            // Act
            var result = await _planService.GetAllPlans("C1");

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAllPlansAsync_WhenNoPlansExist_ShouldReturnEmptyList()
        {
            // Arrange
            _repoMock.Setup(x => x.FindAll(It.IsAny<Expression<Func<Plan, bool>>>())).ReturnsAsync(new List<Plan>());

            // Act
            var result = await _planService.GetAllPlans("C1");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetPlansAsync_WhenCompanyIdIsProvidedHasPlans_ShouldReturnListOfPlansForCompany()
        {
            // Arrange
            var plans = new List<Plan> { new Plan { Company_Id = "C1" } };
            _unitOfWorkMock.Setup(x => x.Entity.FindAll(It.IsAny<Expression<Func<Plan, bool>>>())).ReturnsAsync(plans);

            // Act
            var result = await _planService.GetAllPlans("C1");

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task GetPlansAsync_WhenCompanyIdIsProvidedHasNoPlans_ShouldReturnEmptyList()
        {
            // Act
            var result = await _planService.GetAllPlans("UnknownCompany");

            // Assert
            Assert.Empty(result);
        }
        #endregion

        #region Update plan tests
        [Fact]
        public async Task UpdatePlanAsync_WhenValidPlanIdAndUpdateDataIsProvided_ShouldUpdatePlan()
        {
            // Arrange
            var existingPlan = new Plan { Id = "1", PlanType = "Old", StartDate = DateTime.UtcNow };
            var updateDto = new UpdatePlanDto { PlanId = "1", PlanType = "New", Status = "Active" };

            _unitOfWorkMock.Setup(x => x.Entity.Find(It.IsAny<Expression<Func<Plan, bool>>>())).ReturnsAsync(existingPlan);
            _planTypeServiceMock.Setup(x => x.GetPlanTypeByName(It.IsAny<string>())).ReturnsAsync(new GetPlanTypeDto { NumOfMonths = 2 });
            _statusServiceMock.Setup(x => x.GetStatusByName(It.IsAny<string>())).ReturnsAsync("Active");
            _teamGrpcClientMock.Setup(x => x.GetTeamNameById(It.IsAny<string>())).ReturnsAsync(Result<string>.Success("Team"));
            _repoMock.Setup(x => x.UpdateAsync(It.IsAny<Plan>())).ReturnsAsync(existingPlan);

            // Act
            var result = await _planService.UpdatePlan("comp1", "user1", updateDto);

            // Assert
            Assert.True(result.IsSuccess);
            _unitOfWorkMock.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public async Task UpdatePlanAsync_WhenInValidPlanIdIsProvided_ShouldReturnError()
        {
            // Arrange
            _unitOfWorkMock.Setup(x => x.Entity.Find(It.IsAny<Expression<Func<Plan, bool>>>())).ReturnsAsync((Plan)null);

            // Act
            var result = await _planService.UpdatePlan("c", "u", new UpdatePlanDto { PlanId = "invalid" });

            // Assert
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task UpdatePlanAsync_WhenInvalidUpdateDataIsProvided_ShouldReturnError()
        {
            // Arrange: Valid plan but invalid Status
            _unitOfWorkMock.Setup(x => x.Entity.Find(It.IsAny<Expression<Func<Plan, bool>>>())).ReturnsAsync(new Plan());
            _planTypeServiceMock.Setup(x => x.GetPlanTypeByName(It.IsAny<string>())).ReturnsAsync(new GetPlanTypeDto { NumOfMonths = 1 });
            _statusServiceMock.Setup(x => x.GetStatusByName(It.IsAny<string>())).ReturnsAsync((string)null);

            // Act
            var result = await _planService.UpdatePlan("c", "u", new UpdatePlanDto { Status = "Wrong" });

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Invalid status provided.", result.Error);
        }
        #endregion

        #region Delete plan tests
        [Fact]
        public async Task DeletePlanAsync_WhenValidPlanIdIsProvided_ShouldDeletePlan()
        {
            // Arrange
            var plan = new Plan { Id = "1" };
            _unitOfWorkMock.Setup(x => x.Entity.Find(It.IsAny<Expression<Func<Plan, bool>>>())).ReturnsAsync(plan);

            // Act
            var result = await _planService.DeletePlan("1", "C1");

            // Assert
            Assert.True(result);
            _unitOfWorkMock.Verify(x => x.Entity.Delete(plan), Times.Once);
            _unitOfWorkMock.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public async Task DeletePlanAsync_WhenInValidPlanIdIsProvided_ShouldReturnError()
        {
            // Arrange
            _unitOfWorkMock.Setup(x => x.Entity.Find(It.IsAny<Expression<Func<Plan, bool>>>())).ReturnsAsync((Plan)null);

            // Act
            var result = await _planService.DeletePlan("99", "C1");

            // Assert
            Assert.False(result);
        }
        #endregion
    }
}