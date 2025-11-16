using DatabaseSetting.Core.Entities;
using DatabaseSetting.Core.Interfaces;
using DatabaseSetting.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseSetting.Infrastructure.Repositories
{
    public class DbSettingRepository : IDbSettingRepository
    {
        private readonly AppDbContext _context;

        public DbSettingRepository(AppDbContext context) 
        {
            _context = context;
        }

        public async Task<IEnumerable<DbSettingModel>> GetAllByCompanyIdAsync(string companyId)
        {
            return await _context.DbSettings
                .Where(ds => ds.CompanyId == companyId)
                .ToListAsync();
        }

        public async Task<DbSettingModel?> GetByIdAsync(string id, string companyId)
        {
            return await _context.DbSettings
                   .FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId);
        }


        public async Task<DbSettingModel> CreateAsync(DbSettingModel model)
        {
            await _context.DbSettings.AddAsync(model);
            await _context.SaveChangesAsync();

            return model;
        }

        public async Task<bool> DeleteAsync(string id, string companyId)
        {
            var entity = await GetByIdAsync(id, companyId);

            if (entity == null)
                return false;

            _context.DbSettings.Remove(entity);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<DbSettingModel> UpdateAsync(DbSettingModel model)
        {
            _context.DbSettings.Update(model);
            await _context.SaveChangesAsync();

            return model;
        }
    }
}
