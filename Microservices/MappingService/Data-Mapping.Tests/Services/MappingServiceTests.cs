using AutoMapper;
using Data_Mapping.Core.DTOs;
using Data_Mapping.Core.Interfaces;
using Data_Mapping.Core.Models;
using Data_Mapping.Core.Services;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Mapping.Tests.Services
{
    public class MappingServiceTests
    {
        private readonly Mock<IMappingRepository> _repositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly MappingService _sut;

        public MappingServiceTests()
        {
            _repositoryMock = new Mock<IMappingRepository>();
            _mapperMock = new Mock<IMapper>();

            _sut = new MappingService(
                _repositoryMock.Object,
                _mapperMock.Object
            );
        }

        //  Helpers
        private static ClientSchemaVector MakeTable(int id, string companyId,
            string tableName, List<string>? teamName = null) => new()
            {
                Id = id,
                CompanyId = companyId,
                TableName = tableName,
                TeamName = teamName
            };

        private static TableSummaryDto MakeDto(int id, string name) => new()
        {
            Id = id,
            Name = name
        };

        //  GetAvailableTablesAsync
        [Theory]
        [InlineData("", "HR")]
        [InlineData("company-1", "")]
        [InlineData("", "")]
        [InlineData(null, "HR")]
        [InlineData("company-1", null)]
        public async Task GetAvailableTablesAsync_WithNullOrEmptyInputs_ThrowsArgumentException(
            string companyId, string deptName)
        {
            // Act
            var act = async () => await _sut.GetAvailableTablesAsync(companyId, deptName);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*Company ID and Department Name*");
        }

        [Fact]
        public async Task GetAvailableTablesAsync_WhenRepositoryReturnsNull_ReturnsEmptyList()
        {
            // Arrange
            _repositoryMock
                .Setup(r => r.GetByCompanyAsync("company-1"))
                .ReturnsAsync((List<ClientSchemaVector>?)null!);

            // Act
            var result = await _sut.GetAvailableTablesAsync("company-1", "HR");

            // Assert
            result.Should().NotBeNull().And.BeEmpty();
            _mapperMock.Verify(m => m.Map<List<TableSummaryDto>>(It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public async Task GetAvailableTablesAsync_WhenNoTablesAssignedToAnyDept_ReturnsAllTables()
        {
            // Arrange
            // All 3 tables have no team assigned → all are "available"
            var allTables = new List<ClientSchemaVector>
        {
            MakeTable(1, "company-1", "Orders",   teamName: null),
            MakeTable(2, "company-1", "Products", teamName: null),
            MakeTable(3, "company-1", "Invoices", teamName: new List<string>()) // empty list counts as unassigned
        };

            var expectedDtos = new List<TableSummaryDto>
        {
            MakeDto(1, "Orders"),
            MakeDto(2, "Products"),
            MakeDto(3, "Invoices")
        };

            _repositoryMock
                .Setup(r => r.GetByCompanyAsync("company-1"))
                .ReturnsAsync(allTables);

            _mapperMock
                .Setup(m => m.Map<List<TableSummaryDto>>(It.Is<List<ClientSchemaVector>>(l => l.Count == 3)))
                .Returns(expectedDtos);

            // Act
            var result = await _sut.GetAvailableTablesAsync("company-1", "HR");

            // Assert
            result.Should().HaveCount(3);
            _repositoryMock.Verify(r => r.GetByCompanyAsync("company-1"), Times.Once);
        }

        [Fact]
        public async Task GetAvailableTablesAsync_WhenSomeTablesAssignedToDept_ReturnsOnlyUnassignedTables()
        {
            // Arrange
            // Table 1 is already assigned to "HR" → should be excluded
            // Table 2 is assigned to a different dept → still available for "HR"
            // Table 3 has no team → available
            var allTables = new List<ClientSchemaVector>
        {
            MakeTable(1, "company-1", "Orders",   teamName: new List<string> { "HR" }),
            MakeTable(2, "company-1", "Products", teamName: new List<string> { "Finance" }),
            MakeTable(3, "company-1", "Invoices", teamName: null)
        };

            var expectedDtos = new List<TableSummaryDto>
        {
            MakeDto(2, "Products"),
            MakeDto(3, "Invoices")
        };

            _repositoryMock
                .Setup(r => r.GetByCompanyAsync("company-1"))
                .ReturnsAsync(allTables);

            _mapperMock
                .Setup(m => m.Map<List<TableSummaryDto>>(It.Is<List<ClientSchemaVector>>(l =>
                    l.Count == 2 &&
                    l.All(t => t.TableName != "Orders"))))   // "Orders" must be filtered out
                .Returns(expectedDtos);

            // Act
            var result = await _sut.GetAvailableTablesAsync("company-1", "HR");

            // Assert
            result.Should().HaveCount(2);
            result.Should().NotContain(t => t.Name == "Orders");
        }

        [Fact]
        public async Task GetAvailableTablesAsync_WhenAllTablesAssignedToDept_ReturnsEmptyList()
        {
            // Arrange
            var allTables = new List<ClientSchemaVector>
        {
            MakeTable(1, "company-1", "Orders",   teamName: new List<string> { "HR" }),
            MakeTable(2, "company-1", "Products", teamName: new List<string> { "HR" })
        };

            _repositoryMock
                .Setup(r => r.GetByCompanyAsync("company-1"))
                .ReturnsAsync(allTables);

            _mapperMock
                .Setup(m => m.Map<List<TableSummaryDto>>(It.Is<List<ClientSchemaVector>>(l => l.Count == 0)))
                .Returns(new List<TableSummaryDto>());

            // Act
            var result = await _sut.GetAvailableTablesAsync("company-1", "HR");

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAvailableTablesAsync_WhenTableAssignedToMultipleDepts_ExcludesItForTargetDept()
        {
            // Arrange
            // Table 1 belongs to both "HR" AND "Finance" → not available for "HR"
            var allTables = new List<ClientSchemaVector>
        {
            MakeTable(1, "company-1", "SharedTable", teamName: new List<string> { "HR", "Finance" }),
            MakeTable(2, "company-1", "OtherTable",  teamName: null)
        };

            _repositoryMock
                .Setup(r => r.GetByCompanyAsync("company-1"))
                .ReturnsAsync(allTables);

            _mapperMock
                .Setup(m => m.Map<List<TableSummaryDto>>(It.Is<List<ClientSchemaVector>>(l =>
                    l.Count == 1 && l[0].TableName == "OtherTable")))
                .Returns(new List<TableSummaryDto> { MakeDto(2, "OtherTable") });

            // Act
            var result = await _sut.GetAvailableTablesAsync("company-1", "HR");

            // Assert
            result.Should().HaveCount(1);
            result[0].Name.Should().Be("OtherTable");
        }

        //  GetTablesByDeptAsync
        [Theory]
        [InlineData("", "HR")]
        [InlineData("company-1", "")]
        [InlineData(null, "HR")]
        [InlineData("company-1", null)]
        public async Task GetTablesByDeptAsync_WithNullOrEmptyInputs_ThrowsArgumentException(
            string companyId, string deptName)
        {
            // Act
            var act = async () => await _sut.GetTablesByDeptAsync(companyId, deptName);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*Company ID or Department Name*");
        }

        [Fact]
        public async Task GetTablesByDeptAsync_WhenDeptHasTables_ReturnsMappedDtos()
        {
            // Arrange
            var tables = new List<ClientSchemaVector>
        {
            MakeTable(1, "company-1", "Orders",   teamName: new List<string> { "HR" }),
            MakeTable(2, "company-1", "Products", teamName: new List<string> { "HR" })
        };

            var expectedDtos = new List<TableSummaryDto>
        {
            MakeDto(1, "Orders"),
            MakeDto(2, "Products")
        };

            _repositoryMock
                .Setup(r => r.GetByDeptNameAsync("company-1", "HR"))
                .ReturnsAsync(tables);

            _mapperMock
                .Setup(m => m.Map<List<TableSummaryDto>>(tables))
                .Returns(expectedDtos);

            // Act
            var result = await _sut.GetTablesByDeptAsync("company-1", "HR");

            // Assert
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(expectedDtos);
            _repositoryMock.Verify(r => r.GetByDeptNameAsync("company-1", "HR"), Times.Once);
        }

        [Fact]
        public async Task GetTablesByDeptAsync_WhenRepositoryReturnsNull_ReturnsMappedEmptyList()
        {
            // Arrange
            // Repository returns null → service should substitute an empty list before mapping
            _repositoryMock
                .Setup(r => r.GetByDeptNameAsync("company-1", "HR"))
                .ReturnsAsync((List<ClientSchemaVector>?)null!);

            _mapperMock
                .Setup(m => m.Map<List<TableSummaryDto>>(It.Is<List<ClientSchemaVector>>(l => l.Count == 0)))
                .Returns(new List<TableSummaryDto>());

            // Act
            var result = await _sut.GetTablesByDeptAsync("company-1", "HR");

            // Assert
            result.Should().NotBeNull().And.BeEmpty();
            // Must not crash; mapper receives an empty list (not null)
            _mapperMock.Verify(
                m => m.Map<List<TableSummaryDto>>(It.Is<List<ClientSchemaVector>>(l => l.Count == 0)),
                Times.Once);
        }

        [Fact]
        public async Task GetTablesByDeptAsync_WhenDeptHasNoTables_ReturnsEmptyList()
        {
            // Arrange
            _repositoryMock
                .Setup(r => r.GetByDeptNameAsync("company-1", "NonExistentDept"))
                .ReturnsAsync(new List<ClientSchemaVector>());

            _mapperMock
                .Setup(m => m.Map<List<TableSummaryDto>>(It.IsAny<List<ClientSchemaVector>>()))
                .Returns(new List<TableSummaryDto>());

            // Act
            var result = await _sut.GetTablesByDeptAsync("company-1", "NonExistentDept");

            // Assert
            result.Should().BeEmpty();
        }

        //  AddTablesToDeptAsync
        [Fact]
        public async Task AddTablesToDeptAsync_WhenTableNotFoundInRepository_SkipsItAndReturnsTrue()
        {
            // Arrange
            _repositoryMock
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((ClientSchemaVector?)null);

            // Act
            var result = await _sut.AddTablesToDeptAsync("company-1", "HR", new List<int> { 99 });

            // Assert
            result.Should().BeTrue();
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<ClientSchemaVector>()), Times.Never);
        }

        [Fact]
        public async Task AddTablesToDeptAsync_WhenDeptNotInTeamName_AddsDeptAndCallsUpdate()
        {
            // Arrange
            var table = MakeTable(1, "company-1", "Orders", teamName: new List<string> { "Finance" });

            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(table);
            _repositoryMock.Setup(r => r.UpdateAsync(table)).ReturnsAsync(true);

            // Act
            var result = await _sut.AddTablesToDeptAsync("company-1", "HR", new List<int> { 1 });

            // Assert
            result.Should().BeTrue();
            table.TeamName.Should().Contain("HR");
            _repositoryMock.Verify(r => r.UpdateAsync(table), Times.Once);
        }

        [Fact]
        public async Task AddTablesToDeptAsync_WhenDeptAlreadyInTeamName_SkipsUpdateToAvoidDuplication()
        {
            // Arrange
            // "HR" is already in the TeamName → must NOT add it again or call Update
            var table = MakeTable(1, "company-1", "Orders", teamName: new List<string> { "HR" });

            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(table);

            // Act
            var result = await _sut.AddTablesToDeptAsync("company-1", "HR", new List<int> { 1 });

            // Assert
            result.Should().BeTrue();
            table.TeamName.Should().ContainSingle(n => n == "HR"); // still exactly one "HR", not duplicated
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<ClientSchemaVector>()), Times.Never);
        }

        [Fact]
        public async Task AddTablesToDeptAsync_WhenTableHasNullTeamName_InitializesListAndAddsDept()
        {
            // Arrange
            // TeamName is null → service must initialize it before adding
            var table = MakeTable(1, "company-1", "Orders", teamName: null);

            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(table);
            _repositoryMock.Setup(r => r.UpdateAsync(table)).ReturnsAsync(true);

            // Act
            var result = await _sut.AddTablesToDeptAsync("company-1", "HR", new List<int> { 1 });

            // Assert
            result.Should().BeTrue();
            table.TeamName.Should().NotBeNull();
            table.TeamName!.Should().Contain("HR");
            _repositoryMock.Verify(r => r.UpdateAsync(table), Times.Once);
        }

        [Fact]
        public async Task AddTablesToDeptAsync_WithMixedTableIds_OnlyUpdatesFoundAndUnassignedTables()
        {
            // Arrange
            // Id 1 → exists, not yet assigned to "HR" → should be updated
            // Id 2 → not found in repo → should be skipped
            // Id 3 → exists but already assigned to "HR" → should NOT call Update
            var table1 = MakeTable(1, "company-1", "Orders", teamName: new List<string> { "Finance" });
            var table3 = MakeTable(3, "company-1", "Invoices", teamName: new List<string> { "HR" });

            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(table1);
            _repositoryMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync((ClientSchemaVector?)null);
            _repositoryMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(table3);
            _repositoryMock.Setup(r => r.UpdateAsync(table1)).ReturnsAsync(true);

            // Act
            var result = await _sut.AddTablesToDeptAsync("company-1", "HR", new List<int> { 1, 2, 3 });

            // Assert
            result.Should().BeTrue();
            table1.TeamName.Should().Contain("HR");
            _repositoryMock.Verify(r => r.UpdateAsync(table1), Times.Once);  // only table1
            _repositoryMock.Verify(r => r.UpdateAsync(table3), Times.Never); // table3 already had "HR"
        }

        [Fact]
        public async Task AddTablesToDeptAsync_WithEmptyTableIdsList_ReturnsTrueWithoutAnyRepositoryCall()
        {
            // Act
            var result = await _sut.AddTablesToDeptAsync("company-1", "HR", new List<int>());

            // Assert
            result.Should().BeTrue();
            _repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<ClientSchemaVector>()), Times.Never);
        }

        //  RemoveTablesFromDeptAsync
        [Fact]
        public async Task RemoveTablesFromDeptAsync_WhenTableNotFoundInRepository_SkipsAndReturnsTrue()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((ClientSchemaVector?)null);

            // Act
            var result = await _sut.RemoveTablesFromDeptAsync("company-1", "HR", new List<int> { 99 });

            // Assert
            result.Should().BeTrue();
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<ClientSchemaVector>()), Times.Never);
        }

        [Fact]
        public async Task RemoveTablesFromDeptAsync_WhenTableHasNullTeamName_SkipsUpdateAndReturnsTrue()
        {
            // Arrange
            // TeamName is null → nothing to remove → must NOT crash or call Update
            var table = MakeTable(1, "company-1", "Orders", teamName: null);

            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(table);

            // Act
            var result = await _sut.RemoveTablesFromDeptAsync("company-1", "HR", new List<int> { 1 });

            // Assert
            result.Should().BeTrue();
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<ClientSchemaVector>()), Times.Never);
        }

        [Fact]
        public async Task RemoveTablesFromDeptAsync_WhenDeptNotInTeamName_SkipsUpdateAndReturnsTrue()
        {
            // Arrange
            // "HR" is NOT in the team list → nothing to remove → Update must NOT be called
            var table = MakeTable(1, "company-1", "Orders", teamName: new List<string> { "Finance" });

            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(table);

            // Act
            var result = await _sut.RemoveTablesFromDeptAsync("company-1", "HR", new List<int> { 1 });

            // Assert
            result.Should().BeTrue();
            table.TeamName.Should().NotContain("HR");
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<ClientSchemaVector>()), Times.Never);
        }

        [Fact]
        public async Task RemoveTablesFromDeptAsync_WhenDeptExistsInTeamName_RemovesDeptAndCallsUpdate()
        {
            // Arrange
            var table = MakeTable(1, "company-1", "Orders", teamName: new List<string> { "HR", "Finance" });

            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(table);
            _repositoryMock.Setup(r => r.UpdateAsync(table)).ReturnsAsync(true);

            // Act
            var result = await _sut.RemoveTablesFromDeptAsync("company-1", "HR", new List<int> { 1 });

            // Assert
            result.Should().BeTrue();
            table.TeamName.Should().NotContain("HR");
            table.TeamName.Should().Contain("Finance"); // other teams must remain untouched
            _repositoryMock.Verify(r => r.UpdateAsync(table), Times.Once);
        }

        [Fact]
        public async Task RemoveTablesFromDeptAsync_WithMixedTableIds_OnlyUpdatesEligibleTables()
        {
            // Arrange
            // Id 1 → has "HR" → should be removed and updated
            // Id 2 → not found → should be skipped
            // Id 3 → has no "HR" → should be skipped
            var table1 = MakeTable(1, "company-1", "Orders", teamName: new List<string> { "HR" });
            var table3 = MakeTable(3, "company-1", "Invoices", teamName: new List<string> { "Finance" });

            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(table1);
            _repositoryMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync((ClientSchemaVector?)null);
            _repositoryMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(table3);
            _repositoryMock.Setup(r => r.UpdateAsync(table1)).ReturnsAsync(true);

            // Act
            var result = await _sut.RemoveTablesFromDeptAsync("company-1", "HR", new List<int> { 1, 2, 3 });

            // Assert
            result.Should().BeTrue();
            table1.TeamName.Should().NotContain("HR");
            _repositoryMock.Verify(r => r.UpdateAsync(table1), Times.Once);
            _repositoryMock.Verify(r => r.UpdateAsync(table3), Times.Never);
        }

        [Fact]
        public async Task RemoveTablesFromDeptAsync_WithEmptyTableIdsList_ReturnsTrueWithoutAnyRepositoryCall()
        {
            // Act
            var result = await _sut.RemoveTablesFromDeptAsync("company-1", "HR", new List<int>());

            // Assert
            result.Should().BeTrue();
            _repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<ClientSchemaVector>()), Times.Never);
        }
    }
}
