using AutoMapper;
using Core.DTOs;
using Core.Service.Interface;
using Core.Service.Protos;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using RecoMindAuthenticationAPI.Grpc.Authentication;
using static Core.Service.Protos.subscriptionService;

namespace webApi.Grpc.GrpcImplementations
{
    public class CompanyServiceImpl : CompanyService.CompanyServiceBase
    {
        private readonly IMapper _mapper;
        private readonly ICompanyService _companyService;
        private readonly subscriptionService.subscriptionServiceClient _subscriptionServiceClient;
        private readonly RecoMindAuthenticationAPI.Grpc.Authentication.AuthenticationService.AuthenticationServiceClient _authenticationServiceClient;
        public CompanyServiceImpl(IMapper mapper,
            ICompanyService coreService,
            subscriptionService.subscriptionServiceClient subscriptionServiceClient,
            RecoMindAuthenticationAPI.Grpc.Authentication.AuthenticationService.AuthenticationServiceClient authenticationServiceClient)
        {
            _mapper = mapper;
            _companyService = coreService;
            _subscriptionServiceClient = subscriptionServiceClient;
            _authenticationServiceClient = authenticationServiceClient;
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
                }

                if (!string.IsNullOrEmpty(createDto.AdminId))
                {
                    var admin = _authenticationServiceClient.GetUserById(new GetUserByIdMessage { UserId = createDto.AdminId});

                    if (admin == null || !(admin.Role.ToLower().Equals("admin")) )
                        throw new RpcException(new Status(StatusCode.InvalidArgument, $"User with ID {createDto.SubscriptionId} Not Valid."));                  
                }

                var result = await _companyService.CreateCompanyAsync(createDto);
                return _mapper.Map<CompanyResponse>(result);
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

                if (!string.IsNullOrEmpty(updateDto.SubscriptionId))
                {
                    var subscription = _subscriptionServiceClient.getById(new getByIdRequest { Id = updateDto.SubscriptionId });

                    if (subscription == null)
                        throw new RpcException(new Status(StatusCode.InvalidArgument, $"Subscription with ID {updateDto.SubscriptionId} not found."));
                }

                if (!string.IsNullOrEmpty(updateDto.AdminId))
                {
                    var admin = _authenticationServiceClient.GetUserById(new GetUserByIdMessage { UserId = updateDto.AdminId });

                    if (admin == null || !(admin.Role.ToLower().Equals("admin")))
                        throw new RpcException(new Status(StatusCode.InvalidArgument, $"User with ID {updateDto.SubscriptionId} Not Valid."));
                }               

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
            catch (KeyNotFoundException)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Company with ID {request.Id} not found for update."));
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, $"An error occurred while Deleting the company: {ex.Message}"));
            }
        }

        public override async Task<GetAllCompaniesResponse> GetAll(Core.Service.Protos.Empty request, ServerCallContext context)
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
<<<<<<< HEAD
<<<<<<< Updated upstream
        public async override Task<CompanyResponse> GetAllByAdminId(GitByIdRequest request, ServerCallContext context)
        {
            try
            {
                var result = await _companyService.GetCompanyByAdminId(request.Id);
                if (result == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, $"Company with Admin Id {request.Id} not found."));
                }
                return _mapper.Map<CompanyResponse>(result);
            }
            catch (ArgumentException ex)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid argument: {ex.ParamName}"));
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, $"An error occurred while retrieving the company: {ex.Message}"));
=======

        public override async Task<CompanyResponse> GetCompanyByAdminId(GitByAdminIdRequest request, ServerCallContext context)
        {
            try
            {
                var item = await _companyService.GetCompanyByAdminId(request.AdminId);
                return _mapper.Map<CompanyResponse>(item); 
            }
            catch (KeyNotFoundException)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Resource not found."));
            }
            catch (ArgumentException)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument , "Resource not found."));
>>>>>>> Stashed changes
            }
        }
=======
>>>>>>> b55309eff502b485ffbac0fff343644a670244ed
        public override async Task<CompanyResponse> AssignSubscripion(AssignSubscriptionRequest request, ServerCallContext context)
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
                //Code = company.Code,
                Country = company.Country,
                Industry = company.Industry,
                //Size = company.Size,
                SubscriptionId = company.SubscriptionId
            };

            var updatedCompanyDTO = await _companyService.UpdateCompanyAsync(company.Id, model);

            return new CompanyResponse
            {
                Id = updatedCompanyDTO.Id,
                Name = updatedCompanyDTO.Name,
                //Code = updatedCompanyDTO.Code,
                Country = updatedCompanyDTO.Country,
                Industry = updatedCompanyDTO.Industry,
                //Size = updatedCompanyDTO.Size,
                SubscriptionId = updatedCompanyDTO.SubscriptionId
            };
        }
    }
}