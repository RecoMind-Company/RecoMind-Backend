using AutoMapper;
using Core.Const;
using Core.DTOs;
using Core.Interfaces;
using Core.Models;
using Core.Service;
using Infrastructure.Service;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class BillingCycleServiceTests
    {
        readonly Mock<IUnitOfWork<Company>> _companyUnitOfWorkMock;
        readonly Mock<IMapper> _mapperMock;
        readonly BillingCycleService _serviceBillingCycle;

        public BillingCycleServiceTests()
        {
            _companyUnitOfWorkMock = new Mock<IUnitOfWork<Company>>();
            _mapperMock = new Mock<IMapper>();
            _serviceBillingCycle = new BillingCycleService(_companyUnitOfWorkMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task AssignBillingCycle_WhenCompanyIdIsNullOrEmpty_ShouldThrowArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _serviceBillingCycle.AssignBillingCycle(null, "Monthly"));
            await Assert.ThrowsAsync<ArgumentException>(() => _serviceBillingCycle.AssignBillingCycle("", "Monthly"));
        }

        [Fact]
        public async Task AssignBillingCycle_WhenCycleNameIsNullOrEmpty_ShouldThrowArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _serviceBillingCycle.AssignBillingCycle("123", null));
            await Assert.ThrowsAsync<ArgumentException>(() => _serviceBillingCycle.AssignBillingCycle("123", ""));
        }

        [Fact]
        public async Task AssignBillingCycle_WhenCompanyNotFound_ShouldThrowKeyNotFoundException()
        {
            _companyUnitOfWorkMock.Setup(u => u.Entity.GetByIdAsync("123"))
                           .ReturnsAsync((Company)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _serviceBillingCycle.AssignBillingCycle("123", "Monthly"));
        }

        [Fact]
        public async Task AssignBillingCycle_WhenCycleNameIsInvalid_ShouldThrowArgumentException()
        {
            var company = new Company { Id = "123", Name = "Test" };
            _companyUnitOfWorkMock.Setup(u => u.Entity.GetByIdAsync("123")).ReturnsAsync(company);

            _serviceBillingCycle.CheckBillingCycleName("InvalidCycle");

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _serviceBillingCycle.AssignBillingCycle("123", "InvalidCycle"));

            Assert.Contains("InvalidCycle", ex.Message);
        }

        [Fact]
        public async Task AssignBillingCycle_WhenValid_ShouldUpdateBillingAndReturnDTO()
        {
            var company = new Company { Id = "123", Name = "Test", Billing = "OldCycle" };
            var dto = new GetCompanyDTO { Id = "123", Name = "Test" };

            _companyUnitOfWorkMock.Setup(u => u.Entity.GetByIdAsync("123"))
                .ReturnsAsync(company);
            _companyUnitOfWorkMock.Setup(u => u.Entity.UpdateAsync(company))
                .ReturnsAsync(company);
            _mapperMock.Setup(m => m.Map<GetCompanyDTO>(company))
                .Returns(dto);
            _serviceBillingCycle.CheckBillingCycleName("Monthly");                

            var result = await _serviceBillingCycle.AssignBillingCycle("123", "Monthly");

            Assert.Equal("Monthly", company.Billing);
            Assert.Equal(dto.Id, result.Id);
            Assert.Equal(dto.Name, result.Name);

            _companyUnitOfWorkMock.Verify(u => u.Entity.UpdateAsync(company), Times.Once);
        }

        [Fact]
        public void GetAllBillingCycles_ShouldReturnAllEnumNames()
        {
            // Arrange
            var expected = Enum.GetNames(typeof(BillingCycle));

            // Act
            var result = _serviceBillingCycle.GetAllBillingCycles();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected.Length, result.Count());
            foreach (var cycle in expected)
            {
                Assert.Contains(cycle, result);
            }
        }
    }
}
