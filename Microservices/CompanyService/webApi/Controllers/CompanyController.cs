using Core.DTOs;
using Core.Interfaces;
using Core.Service.Interface;
using Core.Service.Protos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Company.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "admin")]
    public class CompaniesController : ControllerBase
    {
        private readonly ICompanyService _companyService;
        private readonly subscriptionService.subscriptionServiceClient _subscriptionServiceClient;
        private readonly IUnitOfWork<Core.Models.Company> _Repo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CompaniesController(ICompanyService companyService,
            subscriptionService.subscriptionServiceClient subscriptionServiceClient,
            IUnitOfWork<Core.Models.Company> repo,
             IHttpContextAccessor httpContextAccessor)
        {
            _companyService = companyService;
            _subscriptionServiceClient = subscriptionServiceClient;
            _Repo = repo;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("getAll")]
        [ProducesResponseType(typeof(IEnumerable<GetCompanyDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var items = await _companyService.GetAllCompaniesAsync();
            return Ok(items);
        }

        [HttpGet("GetByAdminId")]
        // extract admin id from token  - the auth level in this endpoint is --> [ (Admin OR Manager ) AND Admin ]
        [ProducesResponseType(typeof(GetCompanyDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByAdminId()
        {
            var AdminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                var item = await _companyService.GetCompanyByAdminId(AdminId);
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

        [HttpGet("GetByCompanyId/{id}")]
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

        [HttpPost("create")]
        [ProducesResponseType(typeof(GetCompanyDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateCompanyDTO dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            // call subscription service to validate subscription id

            if (!string.IsNullOrEmpty(dto.SubscriptionId))
            {
                var subscription = _subscriptionServiceClient.getById(new getByIdRequest { Id = dto.SubscriptionId });

                if (subscription == null)
                    throw new ArgumentException($"Subscription with ID {dto.SubscriptionId} not found.");
            }
             
            var AdminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result2 = await _companyService.CreateCompanyAsync(dto , AdminId);
            return Ok(result2);
        }

        [HttpPut("Update/{id}")]
        [ProducesResponseType(typeof(UpdateCompanyDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(string id, [FromBody] CreateCompanyDTO dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                if (!string.IsNullOrEmpty(dto.SubscriptionId))
                {
                    var subscription = _subscriptionServiceClient.getById(new getByIdRequest { Id = dto.SubscriptionId });

                    if (subscription == null)
                        throw new ArgumentException($"Subscription with ID {dto.SubscriptionId} not found.");
                }
                var AdminId = User.FindFirstValue(ClaimTypes.NameIdentifier)??string.Empty;
                var result = await _companyService.UpdateCompanyAsync(id , AdminId , dto);
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
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpDelete("Delete/{id}")]
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
            try
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

                
                var result = await _Repo.Entity.UpdateAsync(company);

                _Repo.Save();

                UpdateCompanyDTO updateCompanyDTO = new UpdateCompanyDTO
                {
                    Id = company.Id,
                    Name = company.Name,
                    Code = company.Code,
                    Country = company.Country,
                    Industry = company.Industry,
                    Size = company.Size,
                    Description = company.Description,
                    SubscriptionId = company.SubscriptionId
                };
                return Ok(updateCompanyDTO);
            }
            catch(KeyNotFoundException ex) 
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch(Exception ex) 
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}