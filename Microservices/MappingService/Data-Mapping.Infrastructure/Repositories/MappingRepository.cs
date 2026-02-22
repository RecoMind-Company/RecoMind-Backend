using Data_Mapping.Core.Interfaces;
using Data_Mapping.Core.Models;
using Data_Mapping.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Mapping.Infrastructure.Repositories
{
    public class MappingRepository : IMappingRepository
    {
        private readonly MappingDbContext _context;

        public MappingRepository(MappingDbContext context)
        {
            _context = context;
        }

        public async Task<ClientSchemaVector?> GetByIdAsync(int id)
            => await _context.ClientSchemaVectors.FindAsync(id);

        public async Task<List<ClientSchemaVector>> GetByCompanyAsync(string companyId)
        {
            return await _context.ClientSchemaVectors
                .Where(t => t.CompanyId == companyId)
                .ToListAsync();
        }
        public async Task<List<ClientSchemaVector>> GetByDeptNameAsync(string companyId, string deptName)
        {
            return await _context.ClientSchemaVectors
                .Where(t => t.CompanyId == companyId && t.TeamName != null)
                .Where(t => t.TeamName.Any(n => n.ToLower() == deptName.ToLower()))
                .ToListAsync();
        }

        public async Task<bool> UpdateAsync(ClientSchemaVector table)
        {
            _context.ClientSchemaVectors.Update(table);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
