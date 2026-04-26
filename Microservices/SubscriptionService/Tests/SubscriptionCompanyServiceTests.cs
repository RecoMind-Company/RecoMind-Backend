using Moq;
using Xunit;
using FluentAssertions;
using AutoMapper;
using Core.Service;
using Core.Interfaces;
using Core.Models;
using Core.DTOs;
using Core.Consts;
using Core.Service.Interface;
using System.Linq.Expressions;

namespace Tests.SubscriptionCompanyServiceTests
{
    public class SubscriptionCompanyServiceTests
    {
        private readonly Mock<IUnitOfWork<SubscriptionCompany>> _uowMock;
        private readonly Mock<IGenericRepository<SubscriptionCompany>> _repoMock;
        private readonly Mock<ISubscriptionTypeService> _subTypeServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly SubscriptionCompanyService _service;

        public SubscriptionCompanyServiceTests()
        {
            _uowMock = new Mock<IUnitOfWork<SubscriptionCompany>>();
            _repoMock = new Mock<IGenericRepository<SubscriptionCompany>>();
            _subTypeServiceMock = new Mock<ISubscriptionTypeService>();
            _mapperMock = new Mock<IMapper>();

            // إعداد الـ Repository داخل الـ UnitOfWork
            _uowMock.Setup(u => u.Entity).Returns(_repoMock.Object);

            _service = new SubscriptionCompanyService(
                _uowMock.Object,
                _mapperMock.Object,
                _subTypeServiceMock.Object);
        }

        #region CreateSubscription Tests
        [Fact]
        public async Task CreateSubscription_ValidData_ReturnsMappedDto()
        {
            // Arrange
            var dto = new CreateSubscriptionCompanyDto { PlanName = "Premium", BillingCycle = BillingCycle.Monthly };
            var planId = "plan-123";
            var planPrice = 100.0;

            _subTypeServiceMock.Setup(s => s.GetId(dto.PlanName)).ReturnsAsync(planId);
            _subTypeServiceMock.Setup(s => s.GetPrice(dto.PlanName)).ReturnsAsync(planPrice);

            _mapperMock.Setup(m => m.Map<GetSubscriptionCompanyDto>(It.IsAny<SubscriptionCompany>()))
                       .Returns(new GetSubscriptionCompanyDto { IsActive = true });

            // Act
            var result = await _service.CreateSubscription(dto);

            // Assert
            result.Should().NotBeNull();
            _repoMock.Verify(r => r.AddAsync(It.IsAny<SubscriptionCompany>()), Times.Once);
            _uowMock.Verify(u => u.Save(), Times.Once);
        }

        [Fact]
        public async Task CreateSubscription_NullDto_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateSubscription(null));
        }
        #endregion

        #region Price Calculation (SetPrice) Tests
        [Theory]
        [InlineData(BillingCycle.Monthly, 100, 100)]    // شهر واحد
        [InlineData(BillingCycle.SemiAnnual, 100, 600)] // 6 شهور
        [InlineData(BillingCycle.Annual, 100, 1200)]   // 12 شهر
        public async Task SetPrice_CalculatesCorrectlyBasedOnCycle(BillingCycle cycle, double basePrice, double expected)
        {
            // Arrange
            _subTypeServiceMock.Setup(s => s.GetPrice("TestPlan")).ReturnsAsync(basePrice);

            // Act
            var result = await _service.SetPrice(cycle, "TestPlan");

            // Assert
            result.Should().Be(expected);
        }
        #endregion

        #region Date Calculation (SetEndDate) Tests
        [Fact]
        public void SetEndDate_MonthlyCycle_AddsOneMonth()
        {
            // Act
            var endDate = _service.SetEndDate(BillingCycle.Monthly);

            // Assert
            endDate.Should().BeCloseTo(DateTime.UtcNow.AddMonths(1), TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void SetEndDate_AnnualCycle_AddsOneYear()
        {
            // Act
            var endDate = _service.SetEndDate(BillingCycle.Annual);

            // Assert
            endDate.Should().BeCloseTo(DateTime.UtcNow.AddYears(1), TimeSpan.FromSeconds(5));
        }
        #endregion

        #region Update & Delete Tests
        [Fact]
        public async Task UpdateSubscription_ExistingId_UpdatesAndSaves()
        {
            // Arrange
            var subId = "sub-001";
            var dto = new CreateSubscriptionCompanyDto { PlanName = "Basic", BillingCycle = BillingCycle.Annual };
            var existingSub = new SubscriptionCompany { Id = subId };

            _repoMock.Setup(r => r.GetByIdAsync(subId)).ReturnsAsync(existingSub);
            _subTypeServiceMock.Setup(s => s.GetId(dto.PlanName)).ReturnsAsync("new-plan-id");
            _subTypeServiceMock.Setup(s => s.GetPrice(dto.PlanName)).ReturnsAsync(50.0);

            // Act
            await _service.UpdateSubscription(subId, dto);

            // Assert
            _repoMock.Verify(r => r.Update(existingSub), Times.Once);
            _uowMock.Verify(u => u.Save(), Times.Once);
        }

        [Fact]
        public async Task DeleteSubscription_InvalidId_ThrowsKeyNotFoundException()
        {
            // Arrange
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<string>())).ReturnsAsync((SubscriptionCompany)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteSubscription("invalid-id"));
        }
        #endregion

        #region GetById Tests
        [Fact]
        public async Task GetSubscriptionById_ValidId_ReturnsDto()
        {
            // Arrange
            var subId = "test-id";
            var sub = new SubscriptionCompany { Id = subId };
            _repoMock.Setup(r => r.GetByIdAsync(subId)).ReturnsAsync(sub);
            _mapperMock.Setup(m => m.Map<GetSubscriptionCompanyDto>(sub))
                       .Returns(new GetSubscriptionCompanyDto { Id = subId });

            // Act
            var result = await _service.GetSubscriptionById(subId);

            // Assert
            result.Id.Should().Be(subId);
        }
        #endregion
    }
}