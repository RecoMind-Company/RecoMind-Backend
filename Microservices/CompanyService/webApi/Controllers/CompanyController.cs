using AutoMapper;
using Core.DTOs;
using Core.Interfaces;
using Core.Service.Interface;
using Core.Service.Protos;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters.Xml;
using RecoMindAuthenticationAPI.Grpc.Authentication;
using static RecoMindAuthenticationAPI.Grpc.Authentication.AuthenticationService;

namespace Company.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly ICompanyService _companyService;
        private readonly subscriptionService.subscriptionServiceClient _subscriptionServiceClient;
        private readonly IUnitOfWork<Core.Models.Company> _Repo;
        RecoMindAuthenticationAPI.Grpc.Authentication.AuthenticationService.AuthenticationServiceClient _authenticationServiceClient;

        public CompaniesController(ICompanyService companyService,
            subscriptionService.subscriptionServiceClient subscriptionServiceClient,
            IUnitOfWork<Core.Models.Company> repo ,
             RecoMindAuthenticationAPI.Grpc.Authentication.AuthenticationService.AuthenticationServiceClient authenticationServiceClient)
        {
            _companyService = companyService;
            _subscriptionServiceClient = subscriptionServiceClient;
            _Repo = repo;
            _authenticationServiceClient = authenticationServiceClient;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<GetCompanyDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var items = await _companyService.GetAllCompaniesAsync();
            return Ok(items);
        }

        [HttpGet("Admin/{AdminID}")]
        [ProducesResponseType(typeof(GetCompanyDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByAdminId(string AdminID)
        {
            try
            {
                var item = await _companyService.GetCompanyByAdminId(AdminID);
                return Ok(item);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GetCompanyDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var item = await _companyService.GetCompanyByIdAsync(id);
                return Ok(item);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(GetCompanyDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateCompanyDTO dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            if (!string.IsNullOrEmpty(dto.SubscriptionId))
            {
                var subscription = _subscriptionServiceClient.getById(new getByIdRequest { Id = dto.SubscriptionId });

                if (subscription == null)
                    throw new ArgumentException( $"Subscription with ID {dto.SubscriptionId} not found.");
            }

            if (!string.IsNullOrEmpty(dto.AdminId))
            {
                var admin = _authenticationServiceClient.GetUserById(new GetUserByIdMessage { UserId = dto.AdminId });

                if (admin == null || !(admin.Role.ToLower().Equals("admin")))
                    throw new ArgumentException($"User with ID {dto.SubscriptionId} Not Valid.");
            }

            var result2 = await _companyService.CreateCompanyAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result2.Id }, result2);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UpdateCompanyDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(string id, [FromBody] CreateCompanyDTO dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                var result = await _companyService.UpdateCompanyAsync(id, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(DeleteCompanyDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var result = await _companyService.DeleteCompanyAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("AssignSubscription")]
        [ProducesResponseType(typeof(UpdateCompanyDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignSubscription(AssignSubscriptionDto Dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var company = await _Repo.Entity.GetByIdAsync(Dto.companyId);          
            if (company == null)
                return NotFound($"Company {Dto.companyId} Not Found ");

            var subscription = _subscriptionServiceClient.getById(new getByIdRequest { Id = Dto.subscriptionId });
            if (subscription == null)
                return NotFound($"Subscription Id {Dto.subscriptionId} Not Found ");

            company.SubscriptionId = Dto.subscriptionId;

            var model = new Core.Models.Company
            {
                Id = company.Id,
                Name = company.Name,
                Code = company.Code,
                Country = company.Country,
                Industry = company.Industry,
                Size = company.Size,
                AdminId = company.AdminId,
                Description = company.Description, 
                CreatedAt = company.CreatedAt,
                SubscriptionId = company.SubscriptionId
            };
            var result = _Repo.Entity.UpdateAsync( model);
            _Repo.Save();
            
            UpdateCompanyDTO updateCompanyDTO = new UpdateCompanyDTO
            {
                Id = model.Id,
                Name = model.Name,
                Code = company.Code,
                Country = model.Country,
                Industry = model.Industry,
                Size = model.Size,
                AdminId = model.AdminId,
                Description = model.Description,
                SubscriptionId = model.SubscriptionId
            };
            return Ok(updateCompanyDTO);
        }
    }
}