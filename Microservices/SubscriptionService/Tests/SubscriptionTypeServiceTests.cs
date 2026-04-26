using Moq;
using Xunit;
using FluentAssertions;
using AutoMapper;
using Core.Service;
using Core.Interfaces;
using Core.Models;
using Core.DTOs.SubscriptionTypeDto;
using System.Linq.Expressions;

namespace Tests.SubscriptionTypeServiceTests
{
    public class SubscriptionTypeServiceTests
    {
        private readonly Mock<IUnitOfWork<SubscriptionType>> _uowMock;
        private readonly Mock<IGenericRepository<SubscriptionType>> _repoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly SubscriptionTypeSevice _service;

        public SubscriptionTypeServiceTests()
        {
            _uowMock = new Mock<IUnitOfWork<SubscriptionType>>();
            _repoMock = new Mock<IGenericRepository<SubscriptionType>>();
            _mapperMock = new Mock<IMapper>();

            // ربط الـ Repository بالـ UnitOfWork Mock
            _uowMock.Setup(u => u.Entity).Returns(_repoMock.Object);

            _service = new SubscriptionTypeSevice(_uowMock.Object, _mapperMock.Object);
        }

        #region AddSubscriptionPlan Tests
        [Fact]
        public async Task AddSubscriptionPlan_ValidDto_ReturnsGetDto()
        {
            // Arrange
            var createDto = new CreateDto { PlanName = "Gold", Price = 100 };
            var model = new SubscriptionType { PlanName = "Gold", Price = 100 };
            var expectedGetDto = new GetDto { PlanName = "Gold", Price = 100 };

            _mapperMock.Setup(m => m.Map<SubscriptionType>(createDto)).Returns(model);
            _mapperMock.Setup(m => m.Map<GetDto>(model)).Returns(expectedGetDto);

            // Act
            var result = await _service.AddSubscriptionPlan(createDto);

            // Assert
            result.Should().NotBeNull();
            result.PlanName.Should().Be(createDto.PlanName);
            _repoMock.Verify(r => r.AddAsync(It.IsAny<SubscriptionType>()), Times.Once);
            _uowMock.Verify(u => u.Save(), Times.Once);
        }
        #endregion

        #region CheckPlanName Tests
        [Fact]
        public async Task CheckPlanName_Exists_ReturnsTrue()
        {
            // Arrange
            _repoMock.Setup(r => r.Find(It.IsAny<Expression<Func<SubscriptionType, bool>>>()))
                     .ReturnsAsync(new SubscriptionType());

            // Act
            var result = await _service.CheckPlanName("Basic");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task CheckPlanName_NotExists_ReturnsFalse()
        {
            // Arrange
            _repoMock.Setup(r => r.Find(It.IsAny<Expression<Func<SubscriptionType, bool>>>()))
                     .ReturnsAsync((SubscriptionType)null);

            // Act
            var result = await _service.CheckPlanName("NonExistent");

            // Assert
            result.Should().BeFalse();
        }
        #endregion

        #region DeleteSubscriptionType Tests
        [Fact]
        public async Task DeleteSubscriptionType_Exists_ReturnsDeleteDto()
        {
            // Arrange
            var planName = "Pro";
            var existingPlan = new SubscriptionType { PlanName = planName };
            _repoMock.Setup(r => r.Find(It.IsAny<Expression<Func<SubscriptionType, bool>>>()))
                     .ReturnsAsync(existingPlan);

            // Act
            var result = await _service.DeleteSubscriptionType(planName);

            // Assert
            result.Message.Should().Contain("succeeded");
            _repoMock.Verify(r => r.Delete(existingPlan), Times.Once);
            _uowMock.Verify(u => u.Save(), Times.Once);
        }

        [Fact]
        public async Task DeleteSubscriptionType_NotExists_ThrowsArgumentException()
        {
            // Arrange
            _repoMock.Setup(r => r.Find(It.IsAny<Expression<Func<SubscriptionType, bool>>>()))
                     .ReturnsAsync((SubscriptionType)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.DeleteSubscriptionType("Fake"));
        }
        #endregion

        #region GetPrice & GetId Tests
        [Fact]
        public async Task GetPrice_Exists_ReturnsCorrectPrice()
        {
            // Arrange
            var plan = new SubscriptionType { PlanName = "Standard", Price = 50.5 };
            _repoMock.Setup(r => r.Find(It.IsAny<Expression<Func<SubscriptionType, bool>>>()))
                     .ReturnsAsync(plan);

            // Act
            var result = await _service.GetPrice("Standard");

            // Assert
            result.Should().Be(50.5);
        }

        [Fact]
        public async Task GetId_Exists_ReturnsCorrectId()
        {
            // Arrange
            var plan = new SubscriptionType { SubscriptionTypeId = "guid-123", PlanName = "Standard" };
            _repoMock.Setup(r => r.Find(It.IsAny<Expression<Func<SubscriptionType, bool>>>()))
                     .ReturnsAsync(plan);

            // Act
            var result = await _service.GetId("Standard");

            // Assert
            result.Should().Be("guid-123");
        }
        #endregion

        #region UpdateSubscriptionType Tests
        [Fact]
        public async Task UpdateSubscriptionType_ValidUpdate_ReturnsUpdatedDto()
        {
            // Arrange
            var oldName = "OldPlan";
            var updateDto = new CreateDto { PlanName = "NewPlan", Price = 200 };
            var existingEntity = new SubscriptionType { SubscriptionTypeId = "1", PlanName = oldName, Price = 100 };

            _repoMock.Setup(r => r.Find(It.IsAny<Expression<Func<SubscriptionType, bool>>>()))
                     .ReturnsAsync(existingEntity);

            // Act
            var result = await _service.UpdateSubscriptionType(oldName, updateDto);

            // Assert
            result.PlanName.Should().Be("NewPlan");
            result.Price.Should().Be(200);
            _uowMock.Verify(u => u.Save(), Times.Once);
        }

        [Fact]
        public async Task UpdateSubscriptionType_PlanNotFound_ThrowsArgumentException()
        {
            // Arrange
            _repoMock.Setup(r => r.Find(It.IsAny<Expression<Func<SubscriptionType, bool>>>()))
                     .ReturnsAsync((SubscriptionType)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdateSubscriptionType("Ghost", new CreateDto()));
        }
        #endregion

        #region GetAllSubscriptionPlan Tests
        [Fact]
        public async Task GetAllSubscriptionPlan_ShouldReturnList()
        {
            // Arrange
            var list = new List<SubscriptionType> { new SubscriptionType(), new SubscriptionType() };
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(list);
            _mapperMock.Setup(m => m.Map<IEnumerable<GetDto>>(list))
                       .Returns(new List<GetDto> { new GetDto(), new GetDto() });

            // Act
            var result = await _service.GetAllSubscriptionPlan();

            // Assert
            result.Should().HaveCount(2);
            _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
        }
        #endregion
    }
}