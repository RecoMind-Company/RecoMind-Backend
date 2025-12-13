using DatabaseSetting.Core.Services;
using DatabaseSetting.WebApi.GrpcServices;
using Grpc.Core;
using DbSetting.Grpc;
using AutoMapper;


namespace DatabaseSetting.WebApi.GrpcServices
{
    public class DbSettingGrpcServiceImpl : DbSettingGrpcService.DbSettingGrpcServiceBase
    {
        private readonly IDbSettingService _service;
        public DbSettingGrpcServiceImpl(IDbSettingService service)
        {
            _service = service;
        }

        public override async Task<DbSettingResponse> GetById(GetByIdRequest request, ServerCallContext context)
        {
            var result = await _service.GetByIdAsync(request.Id, request.CompanyId);

            if (result == null)
                throw new RpcException(new Status(StatusCode.NotFound, "DbSetting not found"));

            return new DbSettingResponse
            {
                Id = result.Id,
                CompanyId = result.CompanyId,
                Name = result.Name,
                DbType = result.DbType,
                CreatedAt = result.CreatedAt.ToString("o") // ISO 8601
            };
        }

        public override async Task<DbSettingResponse> GetByCompanyId(GetByCompanyIdRequest request, ServerCallContext context)
        {
            var result = await _service.GetByCompanyIdAsync(request.CompanyId);

            if (result == null)
                throw new RpcException(new Status(StatusCode.NotFound, "DbSetting not found"));

            return new DbSettingResponse
            {
                Id = result.Id,
                CompanyId = result.CompanyId,
                Name = result.Name,
                DbType = result.DbType,
                CreatedAt = result.CreatedAt.ToString("o")
            };
        }
    }
}