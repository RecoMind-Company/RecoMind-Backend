using AutoMapper;
using FluentAssertions;
using Moq;
using DatabaseSetting.Core.DTOs;
using DatabaseSetting.Core.Entities;
using DatabaseSetting.Core.Interfaces;
using DatabaseSetting.Core.Result;
using DatabaseSetting.Core.Services;

namespace DatabaseSetting.Tests.Services
{
    public class DbSettingServiceTests
    {
        private readonly Mock<IDbSettingRepository> _repoMock;
        private readonly Mock<IEncryptionService> _encryptMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly DbSettingService _sut;

        public DbSettingServiceTests()
        {
            _repoMock = new Mock<IDbSettingRepository>();
            _encryptMock = new Mock<IEncryptionService>();
            _mapperMock = new Mock<IMapper>();

            _sut = new DbSettingService(
                _repoMock.Object,
                _encryptMock.Object,
                _mapperMock.Object
                );
        }

        // Helper methods to create test data
        private static DbSettingModel MakeModel(
            string id = "db-1",
            string companyId = "comp-1",
            string password = "ENCRYPTED_PW",
            string server = "localhost",
            string dbName = "MyDb",
            string user = "sa") =>
            new()
            {
                Id = id,
                CompanyId = companyId,
                Server = server,
                DbName = dbName,
                User = user,
                Password = password,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

        private static CreateDbSettingDto MakeCreateDto(
            string server = "localhost",
            string dbName = "MyDb",
            string user = "sa",
            string password = "plain-pw",
            string name = "Main DB",
            string dbType = "SqlServer") =>
            new()
            {
                Server = server,
                DbName = dbName,
                User = user,
                Password = password,
                Name = name,
                DbType = dbType
            };

        // GetByCompanyIdAsync
        #region GetByCompanyIdAsync

        [Fact]
        public async Task GetByCompanyIdAsync_ShouldReturnNotFound_WhenNoSettingExistsForCompany()
        {
            // Arrange
            _repoMock.Setup(r => r.GetByCompanyIdAsync("comp-1"))
                     .ReturnsAsync((DbSettingModel?)null);

            // Act
            var result = await _sut.GetByCompanyIdAsync("comp-1");

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(DbSettingErrors.NotFound);
        }

        [Fact]
        public async Task GetByCompanyIdAsync_ShouldReturnMappedDto_WhenSettingExists()
        {
            // Arrange
            var model = MakeModel();
            var expectedDto = new DbSettingResponseDto { Id = model.Id, CompanyId = model.CompanyId };

            _repoMock.Setup(r => r.GetByCompanyIdAsync("comp-1")).ReturnsAsync(model);
            _mapperMock.Setup(m => m.Map<DbSettingResponseDto>(model)).Returns(expectedDto);

            // Act
            var result = await _sut.GetByCompanyIdAsync("comp-1");

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(expectedDto);
        }

        #endregion

        // GetByCompanyIdForAiAsync
        #region GetByCompanyIdForAiAsync

        [Fact]
        public async Task GetByCompanyIdForAiAsync_ShouldReturnNotFound_WhenNoSettingExistsForCompany()
        {
            // Arrange
            _repoMock.Setup(r => r.GetByCompanyIdAsync("comp-1"))
                     .ReturnsAsync((DbSettingModel?)null);

            // Act
            var result = await _sut.GetByCompanyIdForAiAsync("comp-1");

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(DbSettingErrors.NotFound);
        }

        [Fact]
        public async Task GetByCompanyIdForAiAsync_ShouldReturnDecryptedPassword_WhenSettingExists()
        {
            // Arrange
            var model = MakeModel(password: "ENCRYPTED_PW");
            var aiDto = new DbSettingResponseForAiDto { Id = model.Id };   // Password starts empty

            _repoMock.Setup(r => r.GetByCompanyIdAsync("comp-1")).ReturnsAsync(model);
            _mapperMock.Setup(m => m.Map<DbSettingResponseForAiDto>(model)).Returns(aiDto);

            _encryptMock.Setup(e => e.Decrypt("ENCRYPTED_PW")).Returns("plain-pw");

            // Act
            var result = await _sut.GetByCompanyIdForAiAsync("comp-1");

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value!.Password.Should().Be("plain-pw");
            _encryptMock.Verify(e => e.Decrypt("ENCRYPTED_PW"), Times.Once);
        }

        [Fact]
        public async Task GetByCompanyIdForAiAsync_ShouldNeverCallEncrypt_OnReadPath()
        {
            // Arrange
            var model = MakeModel();
            _repoMock.Setup(r => r.GetByCompanyIdAsync(It.IsAny<string>())).ReturnsAsync(model);
            _mapperMock.Setup(m => m.Map<DbSettingResponseForAiDto>(model))
                       .Returns(new DbSettingResponseForAiDto());
            _encryptMock.Setup(e => e.Decrypt(It.IsAny<string>())).Returns("x");

            // Act
            await _sut.GetByCompanyIdForAiAsync("comp-1");

            // Assert
            _encryptMock.Verify(e => e.Encrypt(It.IsAny<string>()), Times.Never);
        }

        #endregion

        // GetByIdAsync
        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNotFound_WhenNoEntityExistsWithThatId()
        {
            // Arrange
            _repoMock.Setup(r => r.GetByIdAsync("db-1"))
                     .ReturnsAsync((DbSettingModel?)null);

            // Act
            var result = await _sut.GetByIdAsync("db-1", "comp-1");

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(DbSettingErrors.NotFound);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNotFound_WhenEntityBelongsToADifferentCompany()
        {
            // Arrange
            var model = MakeModel(companyId: "other-comp");
            _repoMock.Setup(r => r.GetByIdAsync("db-1")).ReturnsAsync(model);

            // Act
            var result = await _sut.GetByIdAsync("db-1", "comp-1");

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(DbSettingErrors.NotFound);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnMappedDto_WhenEntityExistsAndBelongsToCompany()
        {
            // Arrange
            var model = MakeModel();
            var expectedDto = new DbSettingResponseDto { Id = model.Id };

            _repoMock.Setup(r => r.GetByIdAsync("db-1")).ReturnsAsync(model);
            _mapperMock.Setup(m => m.Map<DbSettingResponseDto>(model)).Returns(expectedDto);

            // Act
            var result = await _sut.GetByIdAsync("db-1", "comp-1");

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(expectedDto);
        }

        #endregion

        // CreateAsync
        #region CreateAsync

        [Fact]
        public async Task CreateAsync_ShouldReturnAlreadyExists_WhenCompanyAlreadyHasSetting()
        {
            // Arrange
            var existingModel = MakeModel();
            _repoMock.Setup(r => r.GetByCompanyIdAsync("comp-1")).ReturnsAsync(existingModel);

            // Act
            var result = await _sut.CreateAsync(MakeCreateDto(), "comp-1");

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(DbSettingErrors.AlreadyExists);
            _repoMock.Verify(r => r.CreateAsync(It.IsAny<DbSettingModel>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_ShouldEncryptPassword_BeforePersisting()
        {
            // Arrange
            var dto = MakeCreateDto(password: "plain-pw");
            var newModel = new DbSettingModel();

            _repoMock.Setup(r => r.GetByCompanyIdAsync("comp-1"))
                     .ReturnsAsync((DbSettingModel?)null);
            _mapperMock.Setup(m => m.Map<DbSettingModel>(dto)).Returns(newModel);
            _encryptMock.Setup(e => e.Encrypt("plain-pw")).Returns("ENC_PW");
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<DbSettingModel>()))
                     .ReturnsAsync(newModel);
            _mapperMock.Setup(m => m.Map<DbSettingResponseDto>(newModel))
                       .Returns(new DbSettingResponseDto());

            // Capture what the service actually sends to the repository
            DbSettingModel? capturedModel = null;
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<DbSettingModel>()))
                     .Callback<DbSettingModel>(m => capturedModel = m)
                     .ReturnsAsync(newModel);

            // Act
            var result = await _sut.CreateAsync(dto, "comp-1");

            // Assert
            result.IsSuccess.Should().BeTrue();
            capturedModel!.Password.Should().Be("ENC_PW");
            _encryptMock.Verify(e => e.Encrypt("plain-pw"), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldSetIdCompanyIdAndTimestamps_BeforePersisting()
        {
            // Arrange
            var dto = MakeCreateDto();
            var newModel = new DbSettingModel();

            _repoMock.Setup(r => r.GetByCompanyIdAsync("comp-1"))
                     .ReturnsAsync((DbSettingModel?)null);
            _mapperMock.Setup(m => m.Map<DbSettingModel>(dto)).Returns(newModel);
            _encryptMock.Setup(e => e.Encrypt(It.IsAny<string>())).Returns("ENC");
            _mapperMock.Setup(m => m.Map<DbSettingResponseDto>(newModel))
                       .Returns(new DbSettingResponseDto());

            DbSettingModel? capturedModel = null;
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<DbSettingModel>()))
                     .Callback<DbSettingModel>(m => capturedModel = m)
                     .ReturnsAsync(newModel);

            // Act
            await _sut.CreateAsync(dto, "comp-1");

            // Assert
            capturedModel!.Id.Should().NotBeNullOrWhiteSpace();
            capturedModel.CompanyId.Should().Be("comp-1");
            capturedModel.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            capturedModel.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnMappedDto_FromSavedEntity()
        {
            // Arrange
            var dto = MakeCreateDto();
            var newModel = new DbSettingModel();
            var expectedDto = new DbSettingResponseDto { Id = "db-1", CompanyId = "comp-1" };

            _repoMock.Setup(r => r.GetByCompanyIdAsync("comp-1"))
                     .ReturnsAsync((DbSettingModel?)null);
            _mapperMock.Setup(m => m.Map<DbSettingModel>(dto)).Returns(newModel);
            _encryptMock.Setup(e => e.Encrypt(It.IsAny<string>())).Returns("ENC");
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<DbSettingModel>())).ReturnsAsync(newModel);
            _mapperMock.Setup(m => m.Map<DbSettingResponseDto>(newModel)).Returns(expectedDto);

            // Act
            var result = await _sut.CreateAsync(dto, "comp-1");

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(expectedDto);
        }

        [Fact]
        public async Task CreateAsync_ShouldNeverCallDecrypt_OnWritePath()
        {
            // Arrange
            var dto = MakeCreateDto();
            var newModel = new DbSettingModel();

            _repoMock.Setup(r => r.GetByCompanyIdAsync(It.IsAny<string>()))
                     .ReturnsAsync((DbSettingModel?)null);
            _mapperMock.Setup(m => m.Map<DbSettingModel>(dto)).Returns(newModel);
            _encryptMock.Setup(e => e.Encrypt(It.IsAny<string>())).Returns("ENC");
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<DbSettingModel>())).ReturnsAsync(newModel);
            _mapperMock.Setup(m => m.Map<DbSettingResponseDto>(It.IsAny<DbSettingModel>()))
                       .Returns(new DbSettingResponseDto());

            // Act
            await _sut.CreateAsync(dto, "comp-1");

            // Assert
            _encryptMock.Verify(e => e.Decrypt(It.IsAny<string>()), Times.Never);
        }

        #endregion

        // UpdateAsync
        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_ShouldReturnNotFound_WhenEntityDoesNotExist()
        {
            // Arrange
            _repoMock.Setup(r => r.GetByIdAsync("db-1"))
                     .ReturnsAsync((DbSettingModel?)null);

            // Act
            var result = await _sut.UpdateAsync("db-1", "comp-1", new UpdateDbSettingDto());

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(DbSettingErrors.NotFound);
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<DbSettingModel>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnNotFound_WhenEntityBelongsToADifferentCompany()
        {
            // Arrange
            var model = MakeModel(companyId: "other-comp");
            _repoMock.Setup(r => r.GetByIdAsync("db-1")).ReturnsAsync(model);

            // Act
            var result = await _sut.UpdateAsync("db-1", "comp-1", new UpdateDbSettingDto());

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(DbSettingErrors.NotFound);
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<DbSettingModel>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldEncryptNewPassword_WhenPasswordIsProvided()
        {
            // Arrange
            var model = MakeModel();
            var updateDto = new UpdateDbSettingDto { Password = "new-plain-pw" };
            var savedModel = MakeModel();

            _repoMock.Setup(r => r.GetByIdAsync("db-1")).ReturnsAsync(model);
            _encryptMock.Setup(e => e.Encrypt("new-plain-pw")).Returns("NEW_ENC_PW");
            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<DbSettingModel>())).ReturnsAsync(savedModel);
            _mapperMock.Setup(m => m.Map<DbSettingResponseDto>(savedModel))
                       .Returns(new DbSettingResponseDto());

            DbSettingModel? capturedModel = null;
            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<DbSettingModel>()))
                     .Callback<DbSettingModel>(m => capturedModel = m)
                     .ReturnsAsync(savedModel);

            // Act
            var result = await _sut.UpdateAsync("db-1", "comp-1", updateDto);

            // Assert
            result.IsSuccess.Should().BeTrue();
            capturedModel!.Password.Should().Be("NEW_ENC_PW");
            _encryptMock.Verify(e => e.Encrypt("new-plain-pw"), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldNotEncryptPassword_WhenPasswordIsNullOrEmpty()
        {
            // Arrange
            var model = MakeModel(password: "OLD_ENC_PW");
            var updateDto = new UpdateDbSettingDto { Password = null };
            var savedModel = MakeModel(password: "OLD_ENC_PW");

            _repoMock.Setup(r => r.GetByIdAsync("db-1")).ReturnsAsync(model);
            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<DbSettingModel>())).ReturnsAsync(savedModel);
            _mapperMock.Setup(m => m.Map<DbSettingResponseDto>(savedModel))
                       .Returns(new DbSettingResponseDto());

            // Act
            await _sut.UpdateAsync("db-1", "comp-1", updateDto);

            // Assert
            _encryptMock.Verify(e => e.Encrypt(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldSetUpdatedAt_BeforePersisting()
        {
            // Arrange
            var model = MakeModel();
            var updateDto = new UpdateDbSettingDto { Name = "Renamed" };
            var savedModel = MakeModel();

            _repoMock.Setup(r => r.GetByIdAsync("db-1")).ReturnsAsync(model);
            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<DbSettingModel>())).ReturnsAsync(savedModel);
            _mapperMock.Setup(m => m.Map<DbSettingResponseDto>(savedModel))
                       .Returns(new DbSettingResponseDto());

            DbSettingModel? capturedModel = null;
            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<DbSettingModel>()))
                     .Callback<DbSettingModel>(m => capturedModel = m)
                     .ReturnsAsync(savedModel);

            // Act
            await _sut.UpdateAsync("db-1", "comp-1", updateDto);

            // Assert
            capturedModel!.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnMappedDto_FromSavedEntity()
        {
            // Arrange
            var model = MakeModel();
            var updateDto = new UpdateDbSettingDto { Name = "Renamed" };
            var savedModel = MakeModel();
            var expectedDto = new DbSettingResponseDto { Id = "db-1", Name = "Renamed" };

            _repoMock.Setup(r => r.GetByIdAsync("db-1")).ReturnsAsync(model);
            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<DbSettingModel>())).ReturnsAsync(savedModel);
            _mapperMock.Setup(m => m.Map<DbSettingResponseDto>(savedModel)).Returns(expectedDto);

            // Act
            var result = await _sut.UpdateAsync("db-1", "comp-1", updateDto);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(expectedDto);
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<DbSettingModel>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldNotEncryptPassword_WhenPasswordIsEmptyString()
        {
            // Arrange
            var model = MakeModel(password: "OLD_ENC_PW");
            var updateDto = new UpdateDbSettingDto { Password = "" };
            var savedModel = MakeModel();

            _repoMock.Setup(r => r.GetByIdAsync("db-1")).ReturnsAsync(model);
            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<DbSettingModel>())).ReturnsAsync(savedModel);
            _mapperMock.Setup(m => m.Map<DbSettingResponseDto>(savedModel))
                       .Returns(new DbSettingResponseDto());

            // Act
            await _sut.UpdateAsync("db-1", "comp-1", updateDto);

            // Assert
            _encryptMock.Verify(e => e.Encrypt(It.IsAny<string>()), Times.Never);
        }

        #endregion

        // DeleteAsync
        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_ShouldReturnNotFound_WhenEntityDoesNotExist()
        {
            // Arrange
            _repoMock.Setup(r => r.GetByIdAsync("db-1"))
                     .ReturnsAsync((DbSettingModel?)null);

            // Act
            var result = await _sut.DeleteAsync("db-1", "comp-1");

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(DbSettingErrors.NotFound);
            _repoMock.Verify(r => r.DeleteAsync(It.IsAny<DbSettingModel>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnNotFound_WhenEntityBelongsToADifferentCompany()
        {
            // Arrange
            var model = MakeModel(companyId: "other-comp");
            _repoMock.Setup(r => r.GetByIdAsync("db-1")).ReturnsAsync(model);

            // Act
            var result = await _sut.DeleteAsync("db-1", "comp-1");

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(DbSettingErrors.NotFound);
            _repoMock.Verify(r => r.DeleteAsync(It.IsAny<DbSettingModel>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnSuccess_WhenEntityExistsAndBelongsToCompany()
        {
            // Arrange
            var model = MakeModel();
            _repoMock.Setup(r => r.GetByIdAsync("db-1")).ReturnsAsync(model);
            _repoMock.Setup(r => r.DeleteAsync(model)).ReturnsAsync(true);

            // Act
            var result = await _sut.DeleteAsync("db-1", "comp-1");

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeTrue();
            _repoMock.Verify(r => r.DeleteAsync(model), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldPassTheCorrectEntityToRepository()
        {
            // Arrange
            var model = MakeModel();
            _repoMock.Setup(r => r.GetByIdAsync("db-1")).ReturnsAsync(model);

            DbSettingModel? capturedEntity = null;
            _repoMock.Setup(r => r.DeleteAsync(It.IsAny<DbSettingModel>()))
                     .Callback<DbSettingModel>(m => capturedEntity = m)
                     .ReturnsAsync(true);

            // Act
            await _sut.DeleteAsync("db-1", "comp-1");

            // Assert
            capturedEntity.Should().BeSameAs(model);
        }

        #endregion
    }
}