using AutoMapper;
using Core.DTOs;
using Core.Service.Interface;
using Core.Service.Protos;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using static Core.Service.Protos.subscriptionService;

namespace webApi.Grpc.GrpcImplementations
{
    public class CompanyServiceImpl : CompanyService.CompanyServiceBase
    {
        private readonly IMapper _mapper;
        private readonly ICompanyService _companyService;
        private readonly subscriptionService.subscriptionServiceClient _subscriptionServiceClient;


        public CompanyServiceImpl(IMapper mapper, ICompanyService coreService , subscriptionService.subscriptionServiceClient subscriptionServiceClient)
        {
            _mapper = mapper;
            _companyService = coreService;
            _subscriptionServiceClient = subscriptionServiceClient;
        }

        public override async Task<CompanyResponse> Create(CreateCompanyRequest request, ServerCallContext context)
        {
            try
            {
                var createDto = _mapper.Map<CreateCompanyDTO>(request);

                if (!string.IsNullOrEmpty(createDto.SubscriptionId))
                {
                    var subscription = _subscriptionServiceClient.getById(new getByIdRequest { Id = createDto.SubscriptionId });

                    if (subscription == null)
                        throw new RpcException(new Status(StatusCode.InvalidArgument, $"Subscription with ID {createDto.SubscriptionId} not found."));
                    
                    var result1 = await _companyService.CreateCompanyAsync(createDto);
                    return _mapper.Map<CompanyResponse>(result1);
                }

                var result2 = await _companyService.CreateCompanyAsync(createDto);
                return _mapper.Map<CompanyResponse>(result2);
            }
            catch (ArgumentNullException ex)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid argument: {ex.ParamName}"));
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, $"An error occurred while creating the company: \n \t\t {ex.Message}"));
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

        public override async Task<GetAllCompaniesResponse> GetAll(Core.Service.Protos.Empty  request, ServerCallContext context)
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
        public override async Task<CompanyResponse> AssignSubscripion(AssignSubscriptionRequest request,ServerCallContext context)
        {
            var company = await _companyService.GetCompanyByIdAsync(request.CompanyId);

            if (company == null)
            {
                throw new RpcException(new Status(
                    StatusCode.NotFound,
                    $"Company {request.CompanyId} Not Found"
                ));
            }
           
            try
            {
               var subscription = _subscriptionServiceClient.getById(
                   new getByIdRequest { Id = request.SubscriptionId });

                if (subscription == null)
                {
                     throw new RpcException(new Status(
                       StatusCode.NotFound,
                       $"Subscription Id {request.SubscriptionId} Not Found"
                    ));
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
            {
                throw ex;
            }
            catch (System.Exception)
            {
                throw new RpcException(new Status(
                    StatusCode.Internal,
                    "Error communicating with Subscription Service."
                ));
            }

            var model = new CreateCompanyDTO
            {
                Name = company.Name,
                Code = company.Code,
                Country = company.Country,
                Industry = company.Industry,
                Size = company.Size,
                SubscriptionId = company.SubscriptionId
            };

            var updatedCompanyDTO = await _companyService.UpdateCompanyAsync(company.Id, model);

            return new CompanyResponse
            {
                Id = updatedCompanyDTO.Id,
                Name = updatedCompanyDTO.Name,
                Code = updatedCompanyDTO.Code,
                Country = updatedCompanyDTO.Country,
                Industry = updatedCompanyDTO.Industry,
                Size = updatedCompanyDTO.Size,
                SubscriptionId = updatedCompanyDTO.SubscriptionId
            };
        }
    }
}