using AutoMapper;
using Core.DTOs;
using Core.Service.Interface;
using Infrastructure.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Campany.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController(ICompanyService _companyService
        , IBillingCycleServiice _billingCycleServiice
        , IPlaneService _planeService) : ControllerBase
    {                
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var items = await _companyService.GetAllCompaniesAsync();
                return Ok(items);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try 
            { 
                var item = await _companyService.GetCompanyByIdAsync(id);
                return Ok(item);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCompanyDTO dto)
        {
            try 
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                var result = await _companyService.CreateCompanyAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id ,[FromBody]CreateCompanyDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _companyService.UpdateCompanyAsync( id , dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try 
            { 
                var result = await _companyService.DeleteCompanyAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("billing-cycles")]
        public IActionResult GetBillingCycles() 
        {
            return Ok(_billingCycleServiice.GetAllBillingCycles());
        }

        [HttpGet("plans")]
        public IActionResult GetPlans()
        {
            return Ok(_planeService.GetAllPlans());
        }

        [HttpPost("{id}/assign-billing-cycle")]
        public async Task<IActionResult> AssignBillingCycle(string id, string cycleName)
        {
            try 
            { 
                var result = await _billingCycleServiice.AssignBillingCycle(id, cycleName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/assign-plan")]
        public async Task<IActionResult> AssignPlan(string id, string planName)
        {
            try 
            { 
                var result = await _planeService.AssignPlane(id, planName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
