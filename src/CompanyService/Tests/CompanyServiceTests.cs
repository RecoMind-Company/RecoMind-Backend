using AutoMapper;
using Core.DTOs;
using Core.Interfaces;
using Core.Models;
using Infrastructure.Service;
using Moq;

namespace Tests
{
    public class CompanyServiceTests
    {        
        readonly Mock<IUnitOfWork<Company>> _mockCompanyUnitOfWork;
        readonly Mock<IMapper> _mockMapper;
        readonly CompanyService _companyService;
        public CompanyServiceTests()
        {
            _mockCompanyUnitOfWork = new Moq.Mock<IUnitOfWork<Company>>();
            _mockMapper = new Moq.Mock<IMapper>();
            _companyService=new CompanyService( _mockCompanyUnitOfWork.Object , _mockMapper.Object);
        }

        [Fact]
        public async Task CreateCompanyAsync_WhenInputModelIsValid_ShouldAddNewRecordAndReturnMappedDTO()
        {
            // Arrange
            var createDTO = new CreateCompanyDTO
            {
                Name = " Test ",
                Industry = "IT",
                Country = "USA",
                Size = "100-500",
                Code = "TC100"
            };

            var companyModel = new Company { Id = "generated-id", Name = "Test Company" };
            var getDTO = new GetCompanyDTO { Id = "generated-id", Name = "Test Company" };

            _mockMapper.Setup(m => m.Map<Company>(It.IsAny<CreateCompanyDTO>())).Returns(companyModel);
            _mockMapper.Setup(m => m.Map<GetCompanyDTO>(It.IsAny<Company>())).Returns(getDTO);

            _mockCompanyUnitOfWork.Setup(uow => uow.Entity.AddAsync(companyModel))
                .ReturnsAsync(companyModel);

            // Act
            var result = await _companyService.CreateCompanyAsync(createDTO);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(getDTO.Id, result.Id);
            Assert.Equal(getDTO.Name, result.Name);

            _mockCompanyUnitOfWork.Verify(uow => uow.Entity.AddAsync(companyModel), Times.Once);
            _mockCompanyUnitOfWork.Verify(uow => uow.Save(), Times.Once);
        }

        [Fact]
        public async Task CreateCompanyAsync_WhenInputModelIsNull_ShouldThrowArgumentNullException()
        {            
            // Act And Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _companyService.CreateCompanyAsync(null));

            // Verify no calls were made to dependencies
            _mockMapper.Verify(m => m.Map<Company>(It.IsAny<CreateCompanyDTO>()), Times.Never);
            _mockCompanyUnitOfWork.Verify(u => u.Entity.AddAsync(It.IsAny<Company>()), Times.Never);
            _mockCompanyUnitOfWork.Verify(u => u.Save(), Times.Never);
        }

        [Fact]
        public async Task GetAllCompaniesAsync_WhenCompanyTableIsEmpty_ShouldReturnEmptyList()
        {
            //Arrange                                    
            _mockCompanyUnitOfWork.Setup(uow => uow.Entity.GetAllAsync());
                
            //Act
            var result = await _companyService.GetAllCompaniesAsync();

            //Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllCompaniesAsync_WhenListIsNotEmpty_ShouldReturnMappedDTOList()
        {
            // Arrange
            var companies = new List<Company>
            {
                new Company { Id = "1", Name = "Company A" },
                new Company { Id = "2", Name = "Company B" }
            };

            var mappedDTOs = new List<GetCompanyDTO>
            {
                new GetCompanyDTO { Id = "1", Name = "Company A" },
                new GetCompanyDTO { Id = "2", Name = "Company B" }
            };

            _mockCompanyUnitOfWork.Setup(u => u.Entity.GetAllAsync())
                    .ReturnsAsync(companies);

            _mockMapper.Setup(m => m.Map<IEnumerable<GetCompanyDTO>>(companies))
                       .Returns(mappedDTOs);

            // Act
            var result = await _companyService.GetAllCompaniesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());

            Assert.Equal("1", result.First().Id);
            Assert.Equal("Company A", result.First().Name);

            _mockCompanyUnitOfWork.Verify(u => u.Entity.GetAllAsync(), Times.Once);
            _mockMapper.Verify(m => m.Map<IEnumerable<GetCompanyDTO>>(companies), Times.Once);
        }

        [Fact]
        public async Task GetCompanyByIdAsync_WhenCompanyExists_ShouldReturnMappedDTO()
        {
            // Arrange
            var companyId = "123";
            var company = new Company { Id = companyId, Name = "Test Company" };
            _mockCompanyUnitOfWork.Setup(uow => uow.Entity.GetByIdNoTrackingAsync(companyId))
                .ReturnsAsync(company);

            var expectedDto = new GetCompanyDTO { Id = companyId, Name = "Test Company" };
            _mockMapper.Setup(m => m.Map<GetCompanyDTO>(company)).Returns(expectedDto);

            // Act
            var result = await _companyService.GetCompanyByIdAsync(companyId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDto.Id, result.Id);
            Assert.Equal(expectedDto.Name, result.Name);
        }

        [Fact]
        public async Task GetCompanyByIdAsync_WhenCompanyDoesNotExist_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var companyId = "999";
            _mockCompanyUnitOfWork.Setup(uow => uow.Entity.GetByIdNoTrackingAsync(companyId))
                .ReturnsAsync((Company)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _companyService.GetCompanyByIdAsync(companyId));

            Assert.Contains(companyId, exception.Message);
        }

        [Fact]
        public async Task GetCompanyByIdAsync_WhenCompanyIdIsNull_ShouldThrowArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _companyService.GetCompanyByIdAsync(null));
        }

        [Fact]
        public async Task UpdateCompanyAsync_WhenCompanyExists_ShouldReturnUpdatedDTO()
        {
            // Arrange
            var companyId = "123";
            var existingCompany = new Company { Id = companyId, Name = "Old Name" };
            var createDto = new CreateCompanyDTO { Name = "New Name" };
            var updatedEntity = new Company { Id = companyId, Name = "New Name" };
            var expectedDto = new UpdateCompanyDTO { Id = companyId, Name = "New Name" };

            _mockCompanyUnitOfWork.Setup(uow => uow.Entity.GetByIdNoTrackingAsync(companyId))
                .ReturnsAsync(existingCompany);
            _mockMapper.Setup(m => m.Map<Company>(createDto)).Returns(updatedEntity);
            _mockMapper.Setup(m => m.Map<UpdateCompanyDTO>(updatedEntity)).Returns(expectedDto);

            _mockCompanyUnitOfWork.Setup(uow => uow.Entity.UpdateAsync(It.IsAny<Company>()))
                .ReturnsAsync(updatedEntity);

            // Act
            var result = await _companyService.UpdateCompanyAsync(companyId, createDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDto.Id, result.Id);
            Assert.Equal(expectedDto.Name, result.Name);
            _mockCompanyUnitOfWork.Verify(uow => uow.Entity.UpdateAsync(updatedEntity), Times.Once);
            _mockCompanyUnitOfWork.Verify(uow => uow.Save(), Times.Once);
        }

        [Fact]
        public async Task UpdateCompanyAsync_WhenCompanyDoesNotExist_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var companyId = "999";
            var createDto = new CreateCompanyDTO { Name = "New Name" };

            _mockCompanyUnitOfWork.Setup(uow => uow.Entity.GetByIdNoTrackingAsync(companyId))
                .ReturnsAsync((Company)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _companyService.UpdateCompanyAsync(companyId, createDto));

            Assert.Contains(companyId, exception.Message);
        }

        [Fact]
        public async Task UpdateCompanyAsync_WhenCompanyIdIsNull_ShouldThrowArgumentException()
        {
            // Arrange
            var createDto = new CreateCompanyDTO { Name = "New Name" };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _companyService.UpdateCompanyAsync(null, createDto));
        }

        [Fact]
        public async Task DeleteCompanyAsync_WhenIdIsNullOrEmpty_ShouldThrowArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _companyService.DeleteCompanyAsync(null));
            await Assert.ThrowsAsync<ArgumentException>(() => _companyService.DeleteCompanyAsync(""));
        }

        [Fact]
        public async Task DeleteCompanyAsync_WhenCompanyDoesNotExist_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            string companyId = "123";
            _mockCompanyUnitOfWork.Setup(u => u.Entity.GetByIdNoTrackingAsync(companyId))
                           .ReturnsAsync((Company)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _companyService.DeleteCompanyAsync(companyId));
        }

        [Fact]
        public async Task DeleteCompanyAsync_WhenCompanyExists_ShouldDeleteCompanyAndReturnDTO()
        {
            // Arrange
            string companyId = "123";
            var company = new Company { Id = companyId, Name = "Test Company" };
            var deleteDto = new DeleteCompanyDTO { Id = companyId, Massage = "Test Company" };

            _mockCompanyUnitOfWork.Setup(u => u.Entity.GetByIdNoTrackingAsync(companyId))
                           .ReturnsAsync(company);
            _mockMapper.Setup(m => m.Map<DeleteCompanyDTO>(company))
                       .Returns(deleteDto);
           
            _mockCompanyUnitOfWork.Setup(u => u.Entity.Delete(company));
            _mockCompanyUnitOfWork.Setup(u => u.Save());

            // Act
            var result = await _companyService.DeleteCompanyAsync(companyId);

            // Assert
            Assert.Equal(deleteDto.Id, result.Id);
            Assert.Equal(deleteDto.Massage, result.Massage);
            _mockCompanyUnitOfWork.Verify(u => u.Entity.Delete(company), Times.Once);
            _mockCompanyUnitOfWork.Verify(u => u.Save(), Times.Once);
        }
    }
}