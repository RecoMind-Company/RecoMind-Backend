using DatabaseSetting.Core.DTOs;
using DatabaseSetting.Core.Entities;
using DatabaseSetting.Core.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DatabaseSetting.Core.Services
{
    public class DbSettingService : IDbSettingService
    {
        private readonly IDbSettingRepository _repository;
        private readonly IEncryptionService _encryptionService;
        public DbSettingService(IDbSettingRepository repository, IEncryptionService encryptionService)
        {
            _repository = repository;
            _encryptionService = encryptionService;
        }


        public async Task<IEnumerable<DbSettingResponseForAi>> GetAllByCompanyIdForAiAsync(string companyId)
        {
            var entities = await _repository.GetAllByCompanyIdAsync(companyId);

            if (entities == null || !entities.Any())
                return new List<DbSettingResponseForAi>();

            return entities.Select(entity => new DbSettingResponseForAi
            {
                Id = entity.Id,
                CompanyId = entity.CompanyId,
                Server = entity.Server,
                DbName = entity.DbName,
                User = entity.User,
                Password = entity.Password
            });
        }

        public async Task<IEnumerable<DbSettingResponse>> GetAllByCompanyIdAsync(string companyId)
        {
            var entities = await _repository.GetAllByCompanyIdAsync(companyId);

            if (entities == null || !entities.Any())
                return new List<DbSettingResponse>();

            return entities.Select(entity => new DbSettingResponse
            {
                Id = entity.Id,
                CompanyId = entity.CompanyId,
                DbType = entity.DbType,
                Name = entity.Name,
                CreatedAt = entity.CreatedAt
            });
        }
        


        public async Task<DbSettingResponse?> GetByIdAsync(string id, string companyId)
        {
            var entity = await _repository.GetByIdAsync(id, companyId);

            if (entity == null)
                return null;

            return MapToResponse(entity);
        }

        public async Task<DbSettingModel> GetConnectionByIdAsync(string id, string companyId)
        {
            var entity = await _repository.GetByIdAsync(id, companyId);

            if (entity == null)
                return null;

            entity.ConnectionString = entity.ConnectionString;

            return entity;
        }


        public async Task<DbSettingResponse> CreateAsync(CreateDbSettingModel request, string companyId)
        {
            var model = new DbSettingModel
            {
                Id = Guid.NewGuid().ToString(),
                CompanyId = companyId,
                Name = request.Name,
                DbType = request.DbType,

                Server = request.Server,
                DbName = request.DbName,
                User = request.User,
                Password = request.Password,

                ConnectionString = $"Server={request.Server};Database={request.DbName};User Id={request.User};Password={request.Password};",

                CreatedAt = DateTime.UtcNow
            };

            var saved = await _repository.CreateAsync(model);

            return new DbSettingResponse
            {
                Id = saved.Id,
                CompanyId = saved.CompanyId,
                DbType = saved.DbType,
                Name = saved.Name,
                CreatedAt = saved.CreatedAt
            };
        }

        public async Task<DbSettingResponse?> UpdateAsync(string id, string companyId, UpdateDbSettingModel request)
        {
            var entity = await _repository.GetByIdAsync(id, companyId);

            if (entity == null)
                return null;

            entity.Name = request.Name;
            entity.DbType = request.DbType;

            entity.Server = request.Server;
            entity.DbName = request.DbName;
            entity.User = request.User;
            entity.Password = request.Password;

            entity.ConnectionString = $"Server={request.Server};Database={request.DbName};User Id={request.User};Password={request.Password};";

            var updated = await _repository.UpdateAsync(entity);

            return MapToResponse(updated);
        }


        public async Task<bool> DeleteAsync(string id, string companyId)
        {
            return await _repository.DeleteAsync(id, companyId);
        }


        private DbSettingResponse MapToResponse(DbSettingModel entity) => new DbSettingResponse
        {
            Id = entity.Id,
            CompanyId = entity.CompanyId,
            DbType = entity.DbType,
            Name = entity.Name,
            CreatedAt = entity.CreatedAt,
        };
    }
}
