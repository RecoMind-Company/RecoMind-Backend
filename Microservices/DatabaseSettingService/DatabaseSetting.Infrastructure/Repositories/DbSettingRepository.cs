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
        public DbSettingRepository(AppDbContext context) => _context = context;

        public async Task<DbSettingModel?> GetByCompanyIdAsync(string companyId)
        {
            return await _context.DbSettings
                 .AsNoTracking()
                 .FirstOrDefaultAsync(x => x.CompanyId == companyId);
        }

        public async Task<DbSettingModel?> GetByIdAsync(string id)
        {
            return await _context.DbSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<DbSettingModel> CreateAsync(DbSettingModel model)
        {
            await _context.DbSettings.AddAsync(model);
            await _context.SaveChangesAsync();

            return model;
        }

        public async Task<DbSettingModel> UpdateAsync(DbSettingModel model)
        {
            _context.DbSettings.Update(model);
            await _context.SaveChangesAsync();

            return model;
        }

        public async Task<bool> DeleteAsync(DbSettingModel model)
        {
            _context.DbSettings.Remove(model);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
