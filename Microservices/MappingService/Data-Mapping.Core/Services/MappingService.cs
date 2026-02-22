using AutoMapper;
using Data_Mapping.Core.DTOs;
using Data_Mapping.Core.Interfaces;
using Data_Mapping.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Mapping.Core.Services
{
    public class MappingService : IMappingService
    {
        private readonly IMappingRepository _repository;
        private readonly IMapper _mapper;

        public MappingService(IMappingRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<TableSummaryDto>> GetAvailableTablesAsync(string companyId, string deptName)
        {
            if (string.IsNullOrEmpty(companyId) || string.IsNullOrEmpty(deptName))
                throw new ArgumentException("Company ID and Department Name cannot be null or empty.");

            var allTables = await _repository.GetByCompanyAsync(companyId);

            if (allTables == null) return new List<TableSummaryDto>();

            var unassignedTables = allTables
                .Where(t => t.TeamName == null || !t.TeamName.Contains(deptName))
                .ToList();

            return _mapper.Map<List<TableSummaryDto>>(unassignedTables);
        }

        public async Task<List<TableSummaryDto>> GetTablesByDeptAsync(string companyId, string deptName)
        {
            if (string.IsNullOrEmpty(companyId) || string.IsNullOrEmpty(deptName))
                throw new ArgumentException("Company ID or Department Name cannot be null or empty.");

            var tables = await _repository.GetByDeptNameAsync(companyId, deptName);
            return _mapper.Map<List<TableSummaryDto>>(tables ?? new List<ClientSchemaVector>());
        }

        public async Task<bool> AddTablesToDeptAsync(string companyId, string deptName, List<int> tableIds)
        {
            foreach (var id in tableIds)
            {
                var table = await _repository.GetByIdAsync(id);
                if (table != null)
                {
                    table.TeamName ??= new List<string>();

                    if (!table.TeamName.Contains(deptName))
                    {
                        table.TeamName.Add(deptName);
                        await _repository.UpdateAsync(table);
                    }
                }
            }
            return true;
        }

        public async Task<bool> RemoveTablesFromDeptAsync(string companyId, string deptName, List<int> tableIds)
        {
            foreach (var id in tableIds)
            {
                var table = await _repository.GetByIdAsync(id);
                if (table != null && table.TeamName != null && table.TeamName.Contains(deptName))
                {
                    table.TeamName.Remove(deptName);
                    await _repository.UpdateAsync(table);
                }
            }
            return true;
        }
    }
}