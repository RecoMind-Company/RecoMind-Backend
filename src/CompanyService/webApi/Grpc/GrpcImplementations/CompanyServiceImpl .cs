using AutoMapper;
using Core.DTOs;
using Core.Service.Interface;
using Core.Service.Protos;

using Grpc.Core;

namespace webApi.Grpc.GrpcImplementations
{
    public class CompanyServiceImpl : CompanyService.CompanyServiceBase
    {
        private readonly IMapper _mapper;
        private readonly ICompanyService _companyService;

        public CompanyServiceImpl(IMapper mapper, ICompanyService coreService)
        {
            _mapper = mapper;
            _companyService = coreService;
        }

        public override async Task<CompanyResponse> Create(CreateCompanyRequest request, ServerCallContext context)
        {
            try
            {
                var createDto = _mapper.Map<CreateCompanyDTO>(request);

                var createdCompanyDto = await _companyService.CreateCompanyAsync(createDto);

                return _mapper.Map<CompanyResponse>(createdCompanyDto);
            }
            catch (ArgumentNullException ex)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid argument: {ex.ParamName}"));
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, $"An error occurred while creating the company: {ex.Message}"));
            }
        }

        public override async Task<CompanyResponse> GetById(GitByIdRequest request, ServerCallContext context)
        {
            try
            {
                var companyDto = await _companyService.GetCompanyByIdAsync(request.Id);

                if (companyDto == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, $"Company with ID {request.Id} not found."));
                }

                return _mapper.Map<CompanyResponse>(companyDto);
            }
            catch (ArgumentException ex)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid argument: {ex.ParamName}"));
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, $"An error occurred while retrieving the company: {ex.Message}"));
            }
        }

        public override async Task<CompanyResponse> Update(UpdateCompanyRequest request, ServerCallContext context)
        {
            try
            {
                var updateDto = _mapper.Map<CreateCompanyDTO>(request);

                var updatedCompanyDto = await _companyService.UpdateCompanyAsync(request.Id, updateDto);

                if (updatedCompanyDto == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, $"Company with ID {request.Id} not found for update."));
                }

                return _mapper.Map<CompanyResponse>(updatedCompanyDto);
            }
            catch (ArgumentException ex)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid argument: {ex.ParamName}"));
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, $"An error occurred while updating the company: {ex.Message}"));
            }
        }

        public override async Task<DeleteCompanyResponse> Delete(DeleteCompanyRequest request, ServerCallContext context)
        {
            try
            {
                var result = await _companyService.DeleteCompanyAsync(request.Id);
                return _mapper.Map<DeleteCompanyResponse>(result);
            }
            catch (ArgumentException ex)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid argument: {ex.ParamName}"));
            }
            catch (KeyNotFoundException ex)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Company with ID {request.Id} not found for update."));
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, $"An error occurred while Deleting the company: {ex.Message}"));
            }
        }

        public override async Task<GetAllCompaniesResponse> GetAll(Empty request, ServerCallContext context)
        {
            try
            {
                var companiesList = await _companyService.GetAllCompaniesAsync();

                var response = new GetAllCompaniesResponse();

                response.Items.AddRange(_mapper.Map<IEnumerable<CompanyResponse>>(companiesList));

                return response;
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, $"An error occurred while Deleting the company: {ex.Message}"));
            }
        }
    }
}