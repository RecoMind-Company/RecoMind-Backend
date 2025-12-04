using DatabaseSetting.Core.Services;
using DatabaseSetting.WebApi.GrpcServices;
using Grpc.Core;
using DbSetting.Grpc;


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
                CreatedAt = result.CreatedAt.ToString("O")
            };
        }

        public override async Task<DbSettingListResponse> GetAllByCompanyId(GetAllByCompanyIdRequest request, ServerCallContext context)
        {
            var list = await _service.GetAllByCompanyIdAsync(request.CompanyId);
            var response = new DbSettingListResponse();

            foreach (var item in list)
            {
                response.Items.Add(new DbSettingResponse
                {
                    Id = item.Id,
                    CompanyId = item.CompanyId,
                    Name = item.Name,
                    DbType = item.DbType,
                    CreatedAt = item.CreatedAt.ToString("O")
                });
            }

            return response;
        }

        public override async Task<DbSettingConnectionResponse> GetConnectionById(GetByIdRequest request, ServerCallContext context)
        {
            var entity = await _service.GetConnectionByIdAsync(request.Id, request.CompanyId);

            if (entity == null)
                throw new RpcException(new Status(StatusCode.NotFound, "Connection not found"));

            return new DbSettingConnectionResponse
            {
                Id = entity.Id,
                ConnectionString = entity.ConnectionString
            };
        }
    }

}