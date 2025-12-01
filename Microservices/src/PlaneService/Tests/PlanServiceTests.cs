using AutoMapper;
using Core.DTOs;
using Core.Interfaces;
using Core.Models;
using Core.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit; 

namespace Tests
{    
    public class PlanServiceTests
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IUnitOfWork<Plan>> _unitOfWorkMock;
        private readonly Mock<IGenericRepository<Plan>> _entityRepositoryMock;
        private readonly PlanService _planService;

        private readonly string _testPlanId = "plan-123";
        private readonly Plan _testPlan = new Plan { Id = "plan-123", Name = "Basic Plan", TeamId = "team-456" };
        private readonly GetPlaneDto _testPlanDto = new GetPlaneDto { Id = "plan-123", Name = "Basic Plan DTO" };
        private readonly CreatePlanDto _createPlanDto = new CreatePlanDto { Name = "New Plan" };

        public PlanServiceTests()
        {
            _mapperMock = new Mock<IMapper>();
            _unitOfWorkMock = new Mock<IUnitOfWork<Plan>>();
            _entityRepositoryMock = new Mock<IGenericRepository<Plan>>();

            _unitOfWorkMock.Setup(uow => uow.Entity).Returns(_entityRepositoryMock.Object);

            _planService = new PlanService(_mapperMock.Object, _unitOfWorkMock.Object);
        }

       
        [Fact] 
        public async Task CreatePlan_ShouldCreateAndReturnPlanDto()
        {
            // Arrange
            _mapperMock.Setup(m => m.Map(It.IsAny<CreatePlanDto>(), It.IsAny<Plan>()))
                       .Callback<CreatePlanDto, Plan>((src, dest) => dest.Name = src.Name);

            _entityRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Plan>()))
                                 .ReturnsAsync((Plan p) => p);

            _mapperMock.Setup(m => m.Map(It.IsAny<Plan>(), It.IsAny<GetPlaneDto>()))
                       .Returns(_testPlanDto);

            // Act
            var result = await _planService.CreatePlan(_createPlanDto);

            // Assert
            Assert.NotNull(result);

            _unitOfWorkMock.Verify(uow => uow.Save(), Times.Once);
            _entityRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Plan>()), Times.Once);
            Assert.Equal(_testPlanDto.Id, result.Id);
        }
       
        [Fact]
        public async Task GetPlan_WithValidId_ShouldReturnPlanDto()
        {
            // Arrange           
            _entityRepositoryMock.Setup(repo => repo.GetByIdAsync(_testPlanId))
                                 .ReturnsAsync(_testPlan);

            _mapperMock.Setup(m => m.Map<GetPlaneDto>(_testPlan))
                       .Returns(_testPlanDto);

            // Act
            var result = await _planService.GetPlan(_testPlanId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_testPlanId, result.Id);
        }

        [Fact]
        public async Task GetPlan_WithInvalidId_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            _entityRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>()))
                                 .ReturnsAsync((Plan)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _planService.GetPlan("non-existent-id"));
        }

       
        [Fact]
        public async Task DeletePlan_WithValidId_ShouldDeleteAndReturnSuccessDto()
        {
            // Arrange
            _entityRepositoryMock.Setup(repo => repo.GetByIdAsync(_testPlanId))
                                 .ReturnsAsync(_testPlan);
          
            _entityRepositoryMock.Setup(repo => repo.Delete(It.IsAny<Plan>()))
                                 .Returns(_testPlan);
            // Act
            var result = await _planService.DeletePlan(_testPlanId);

            // Assert
            Assert.NotNull(result);
            _unitOfWorkMock.Verify(uow => uow.Save(), Times.Once); 
            _entityRepositoryMock.Verify(repo => repo.Delete(It.IsAny<Plan>()), Times.Once);
            Assert.Equal(_testPlanId, result.Id);
            Assert.Contains("Successfuly", result.Message);
        }
        [Fact]
        public async Task GetAllPlansByTeamId_ShouldReturnFilteredPlanDtos()
        {
            // Arrange
            var plans = new List<Plan>
            {
                _testPlan, 
                new Plan { Id = "plan-2", Name = "Pro Plan", TeamId = "team-456" }
            };
            var planDtos = new List<GetPlaneDto>
            {
                _testPlanDto,
                new GetPlaneDto { Id = "plan-2", Name = "Pro Plan DTO" }
            };
            string targetTeamId = "team-456";


            _entityRepositoryMock.Setup(repo => repo.FindAll(
                                         It.IsAny<System.Linq.Expressions.Expression<Func<Plan, bool>>>()))
                                 .ReturnsAsync(plans);


            _mapperMock.Setup(m => m.Map<IEnumerable<GetPlaneDto>>(plans))
                       .Returns(planDtos);

            // Act
            var result = await _planService.GetAllPlansByTeamId(targetTeamId);

            // Assert
            Assert.NotNull(result);

            Assert.Equal(2, result.Count());
            Assert.Contains(result, p => p.Id == _testPlanId);

            _entityRepositoryMock.Verify(repo => repo.FindAll(
                                         It.IsAny<System.Linq.Expressions.Expression<Func<Plan, bool>>>()), Times.Once);
        }

        [Fact]
        public async Task GetAllPlansByTeamId_WhenNoPlansFound_ShouldReturnEmptyList()
        {           
            // Act
            var result = await _planService.GetAllPlansByTeamId("non-existent-team");

            // Assert
            Assert.Empty(result);
        }
        
        [Fact]
        public async Task UpdatePlan_WithValidId_ShouldUpdateAndReturnNewPlanDto()
        {
            // Arrange
            var updateDto = new CreatePlanDto { Name = "Updated Plan Name"};
            var existingPlan = new Plan { Id = _testPlanId, Name = "Old Name", CreatedAt = DateTime.Now.AddDays(-5)};
            var updatedPlan = new Plan { Id = _testPlanId, Name = "Updated Plan Name", CreatedAt = existingPlan.CreatedAt, Status = existingPlan.Status };
            var updatedPlanDto = new GetPlaneDto { Id = _testPlanId, Name = "Updated Plan Name DTO" };


            _entityRepositoryMock.Setup(repo => repo.GetByIdAsync(_testPlanId))
                                 .ReturnsAsync(existingPlan);


            _mapperMock.Setup(m => m.Map<Plan>(updateDto))
                       .Returns(updatedPlan);


            _mapperMock.Setup(m => m.Map<GetPlaneDto>(updatedPlan))
                       .Returns(updatedPlanDto);


            _entityRepositoryMock.Setup(repo => repo.Update(It.IsAny<Plan>()));

            // Act
            var result = await _planService.UpdatePlan(_testPlanId, updateDto);

            // Assert
            Assert.NotNull(result);

            Assert.Equal(updatedPlanDto.Name, result.Name);

            _entityRepositoryMock.Verify(repo => repo.Update(
                It.Is<Plan>(p => p.Id == _testPlanId && p.Name == "Updated Plan Name")), Times.Once);

            _unitOfWorkMock.Verify(uow => uow.Save(), Times.Once);
        }

        [Fact]
        public async Task UpdatePlan_WithInvalidId_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            _entityRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>()))
                                 .ReturnsAsync((Plan)null);

            var updateDto = new CreatePlanDto { Name = "Updated Plan Name" };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _planService.UpdatePlan("non-existent-id", updateDto));
        }

    }
}
    
