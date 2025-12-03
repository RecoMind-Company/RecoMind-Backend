using AutoMapper;
using Core.DTOs;
using Core.Service.Interface;
using Core.Service.Protos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters.Xml;

namespace Company.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly ICompanyService _companyService;
        private readonly subscriptionService.subscriptionServiceClient _subscriptionServiceClient;

        public CompaniesController(ICompanyService companyService, subscriptionService.subscriptionServiceClient subscriptionServiceClient)
        {
            _companyService = companyService;
            _subscriptionServiceClient = subscriptionServiceClient;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<GetCompanyDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var items = await _companyService.GetAllCompaniesAsync();
            return Ok(items);
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
                    return NotFound($"Subscription Id {dto.SubscriptionId} Not Found ");

                var result1 = await _companyService.CreateCompanyAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = result1.Id }, result1);
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

            var company = await _companyService.GetCompanyByIdAsync(Dto.companyId);
            var subscription = _subscriptionServiceClient.getById(new getByIdRequest { Id = Dto.subscriptionId });

            if (company == null)
                return NotFound($"Company {Dto.companyId} Not Found ");

            if (subscription == null)
                return NotFound($"Subscription Id {Dto.subscriptionId} Not Found ");

            company.SubscriptionId = Dto.subscriptionId;

            var model = new CreateCompanyDTO
            {
                Name = company.Name,
                Code = company.Code,
                Country = company.Country,
                Industry = company.Industry,
                Size = company.Size,
                SubscriptionId = company.SubscriptionId
            };
            return Ok(await _companyService.UpdateCompanyAsync(company.Id, model));
        }
    }
}