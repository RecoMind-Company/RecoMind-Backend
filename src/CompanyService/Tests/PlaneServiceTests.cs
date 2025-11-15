using AutoMapper;
using Core.Const;
using Core.DTOs;
using Core.Interfaces;
using Core.Models;
using Core.Service;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class PlaneServiceTests
    {
        readonly Mock<IUnitOfWork<Company>> _unitOfWorkMock;
        readonly Mock<IMapper> _mapperMock;
        readonly PlaneService _planeService;
        public PlaneServiceTests()
        {
            _mapperMock = new Mock<IMapper>();
            _unitOfWorkMock = new Mock<IUnitOfWork<Company>>();
            _planeService = new PlaneService(_unitOfWorkMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task AssignPlane_WhenTheIdNullOrWhiteSpace_ShouldThrowArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _planeService.AssignPlane(null, "pro"));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _planeService.AssignPlane("", "pro"));
        }

        [Fact]
        public async Task AssignPlane_WhenPlaneNameIsNullOrEmpty_ShouldThrowArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _planeService.AssignPlane("123", null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _planeService.AssignPlane("123", ""));
        }

        [Fact]
        public async Task AssignPlane_WhenCompanyNotFound_ShouldThrowKeyNotFoundException()
        {
            _unitOfWorkMock.Setup(u => u.Entity.GetByIdAsync("123"))
                           .ReturnsAsync((Company)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _planeService.AssignPlane("123", "pro"));
        }

        [Fact]
        public async Task AssignPlane_WhenPlaneNameIsInvalid_ShouldThrowArgumentException()
        {
            var company = new Company { Id = "123", Name = "test" };
            _unitOfWorkMock.Setup(u => u.Entity.GetByIdAsync("123")).ReturnsAsync(company);

            _planeService.CheckPlanName("InvalidPlane");

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _planeService.AssignPlane("123", "InvalidPlane"));

            Assert.Contains("InvalidPlane", ex.Message);
        }

        [Fact]
        public async Task AssignPlane_WhenValid_ShouldUpdatePlaneAndReturnDTO()
        {
            var company = new Company { Id = "123", Name = "Test", PlanType = "pro" };
            var dto = new GetCompanyDTO { Id = "123", Name = "Test" };

            _unitOfWorkMock.Setup(u => u.Entity.GetByIdAsync("123"))
                .ReturnsAsync(company);
            _unitOfWorkMock.Setup(u => u.Entity.UpdateAsync(company))
                .ReturnsAsync(company);
            _mapperMock.Setup(m => m.Map<GetCompanyDTO>(company))
                .Returns(dto);
            _planeService.CheckPlanName("pro");

            var result = await _planeService.AssignPlane("123", "pro");

            Assert.Equal("pro", company.PlanType.ToLower());
            Assert.Equal(dto.Id, result.Id);
            Assert.Equal(dto.Name, result.Name);

            _unitOfWorkMock.Verify(u => u.Entity.UpdateAsync(company), Times.Once);
        }

        [Fact]
        public void GetAllPlans_ShouldReturnAllEnumNames()
        {
            // Arrange
            var expected = Enum.GetNames(typeof(CompanyPlanType));

            // Act
            var result = _planeService.GetAllPlans();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected.Length, result.Count());
            foreach (var plane in expected)
            {
                Assert.Contains(plane, result);
            }
        }
    }
}
