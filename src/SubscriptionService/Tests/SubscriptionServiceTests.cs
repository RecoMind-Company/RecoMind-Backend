using AutoMapper;
using Moq;
using Core.DTOs;
using Core.Models;
using Core.Service;
using Core.Interfaces;
namespace Tests
{
    public class SubscriptionServiceTests
    {
        private readonly Mock<IUnitOfWork<Subscription>> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly SubscriptionService _subscriptionService;

        public SubscriptionServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork<Subscription>>();
            _mockMapper = new Mock<IMapper>();

            var mockRepository = new Mock<IGenericRepository<Subscription>>();
            _mockUnitOfWork.Setup(u => u.Entity).Returns(mockRepository.Object);

            _subscriptionService = new SubscriptionService(_mockUnitOfWork.Object, _mockMapper.Object);
        }        

        [Fact]
        public async Task CreateSubscription_ValidDto_AddsSubscriptionAndReturnsDto()
        {
            // Arrange
            var createDto = new CreateSubscriptionDto { PlanName = "Pro", BillingCycle = "Monthly" };

            var subscriptionModel = new Subscription { Price = 2000, StartDate = DateTime.Now, EndDate = DateTime.Now.AddMonths(1) };

            var expectedGetDto = new GetSubscriptionDto { PlanName = "Pro" };

            _mockMapper.Setup(m => m.Map(createDto, It.IsAny<Subscription>())).Returns(subscriptionModel);
            _mockMapper.Setup(m => m.Map<GetSubscriptionDto>(It.IsAny<Subscription>())).Returns(expectedGetDto);

            // Act
            var result = await _subscriptionService.CreateSubscription(createDto);

            // Assert
            Assert.Equal(expectedGetDto, result);
            // Verify persistence calls
            _mockUnitOfWork.Verify(u => u.Entity.AddAsync(It.IsAny<Subscription>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }

        [Fact]
        public async Task CreateSubscription_NullDto_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _subscriptionService.CreateSubscription(null));
        }

        [Fact]
        public async Task GetSubscriptionById_SubscriptionExists_ReturnsMappedDto()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid().ToString();
            var subscriptionModel = new Subscription { Id = subscriptionId };
            var expectedDto = new GetSubscriptionDto { Id = subscriptionId };

            _mockUnitOfWork.Setup(u => u.Entity.GetByIdAsync(subscriptionId))
                           .ReturnsAsync(subscriptionModel);

            _mockMapper.Setup(m => m.Map<GetSubscriptionDto>(subscriptionModel))
                       .Returns(expectedDto);

            // Act
            var result = await _subscriptionService.GetSubscriptionById(subscriptionId);

            // Assert
            Assert.Equal(expectedDto, result);
        }

        [Fact]
        public async Task GetSubscriptionById_SubscriptionNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid().ToString();
            _mockUnitOfWork.Setup(u => u.Entity.GetByIdAsync(nonExistentId))
                           .ReturnsAsync((Subscription)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _subscriptionService.GetSubscriptionById(nonExistentId));
        }

        [Fact]
        public async Task UpdateSubscription_ValidData_UpdatesPriceAndReturnsDto()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid().ToString();

            var updateDto = new CreateSubscriptionDto { PlanName = "Pro", BillingCycle = "SemiAnnual" };

            var existingModel = new Subscription { Id = subscriptionId, PlanName = "Free", Price = 0.0, IsActive = false };

            var expectedUpdatedModel = new Subscription
            {
                Id = subscriptionId,
                PlanName = "Pro",
                Price = 2000, 
            };

            var expectedUpdateDto = new UpdateSubscriptionDto { Id = subscriptionId, PlanName = "Pro" };

            // 1. Mock finding the existing subscription
            _mockUnitOfWork.Setup(u => u.Entity.GetByIdAsync(subscriptionId))
                           .ReturnsAsync(existingModel);

            // 2. Mock the mapping process (DTO onto the existing model
            _mockMapper.Setup(m => m.Map(updateDto, existingModel))
                       .Callback(() =>
                       {
                           existingModel.PlanName = updateDto.PlanName;
                       })
                       .Returns(existingModel); 

            // 3. Mock the final mapping to the return DTO (from the updated model state)
            _mockMapper.Setup(m => m.Map<UpdateSubscriptionDto>(It.IsAny<Subscription>()))
                       .Returns(expectedUpdateDto);

            // Act
            var result = await _subscriptionService.UpdateSubscription(subscriptionId, updateDto);

            // Assert
            Assert.Equal(expectedUpdateDto, result);

            // Verify persistence calls
            _mockUnitOfWork.Verify(u => u.Entity.Update(existingModel), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);                      
        }

        [Fact]
        public async Task UpdateSubscription_SubscriptionNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid().ToString();
            var updateDto = new CreateSubscriptionDto { PlanName = "Pro", BillingCycle = "Monthly" };

            _mockUnitOfWork.Setup(u => u.Entity.GetByIdAsync(nonExistentId))
                           .ReturnsAsync((Subscription)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _subscriptionService.UpdateSubscription(nonExistentId, updateDto));
            // Ensure persistence methods were not called
            _mockUnitOfWork.Verify(u => u.Entity.Update(It.IsAny<Subscription>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Never);
        }

        [Theory]
        [InlineData("id", null)]
        [InlineData(null, "dto")]
        [InlineData("", "dto")]
        public async Task UpdateSubscription_NullOrEmptyInput_ThrowsArgumentNullException(string id, string dto)
        {
            // Arrange
            CreateSubscriptionDto subscriptionDto = dto == "dto" ? new CreateSubscriptionDto() : null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _subscriptionService.UpdateSubscription(id, subscriptionDto));
        }

        [Fact]
        public async Task DeleteSubscription_SubscriptionExists_DeletesAndReturnsDto()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid().ToString();
            var subscriptionModel = new Subscription { Id = subscriptionId};
            var expectedDeleteDto = new DeleteSubscriptionDto { Id = subscriptionId };

            // 1. Mock GetByIdAsync to successfully return the existing model
            _mockUnitOfWork.Setup(r => r.Entity.GetByIdAsync(subscriptionId))
                           .ReturnsAsync(subscriptionModel);

            // 2. Mock the final mapping from Model to DTO
            _mockMapper.Setup(m => m.Map<DeleteSubscriptionDto>(subscriptionModel))
                       .Returns(expectedDeleteDto);

            // Act
            var result = await _subscriptionService.DeleteSubscription(subscriptionId);

            // Assert
            Assert.Equal(expectedDeleteDto, result);

            // Verify that the Delete method was called exactly once
            _mockUnitOfWork.Verify(r => r.Entity.Delete(subscriptionModel), Times.Once);

            // Verify that the Save method was called exactly once
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }

        [Fact]
        public async Task DeleteSubscription_SubscriptionNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid().ToString();

            // 1. Mock GetByIdAsync to return null (Subscription not found)
            _mockUnitOfWork.Setup(r => r.Entity.GetByIdAsync(nonExistentId))
                           .ReturnsAsync((Subscription)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _subscriptionService.DeleteSubscription(nonExistentId));

            // Verify that Delete and Save methods were NOT called
            _mockUnitOfWork.Verify(r => r.Entity.Delete(It.IsAny<Subscription>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task DeleteSubscription_NullOrEmptyId_ThrowsArgumentNullException(string subscriptionId)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _subscriptionService.DeleteSubscription(subscriptionId));

            // Verify that no repository calls were made
            _mockUnitOfWork.Verify(r => r.Entity.GetByIdAsync(It.IsAny<string>()), Times.Never);
        }
    }
}