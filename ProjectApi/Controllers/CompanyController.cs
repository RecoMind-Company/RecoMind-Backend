using Core.Interfaces;
using Core.Model;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectApi.DTO;

namespace ProjectApi.Controllers
{
    [Route("api/[controller]s")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly IUnitOfWork<Company> companyUnitOfWork;
        private readonly UserManager<AppUser> userManager;

        public CompanyController(IUnitOfWork<Company> CompanyUnitOfWork, UserManager<AppUser> userManager)
        {
            companyUnitOfWork = CompanyUnitOfWork;
            this.userManager = userManager;
        }

        [HttpGet("GetCompanies")]
        public async Task<IActionResult> GetCompanies()
        {
            var company = await companyUnitOfWork.Entity.GetAllAsync();
            return Ok(company);
        }

        [HttpGet("GetCompanies/{userId}")]
        public async Task<IActionResult> GetCompanies(string userId)
        {
            
            var company = await companyUnitOfWork.Entity.FindAll(x=>x.UserId == userId);


            return Ok(company);
        }

        [HttpGet("GetCompany/{Id}")]
        public async Task<IActionResult> GetCompany(string Id)
        {
            var company = await companyUnitOfWork.Entity.GetAsync(Id);
            return Ok(company);
        }

        [HttpPost("addCompany")]
        public async Task<IActionResult> AddCompany(CompanyDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await userManager.GetUserAsync(User);
            var company = new Company
            {
                Id = Guid.NewGuid().ToString(),
                Name = dto.Name,
                Industry = dto.Industry,
                CompanyCode = dto.CompanyCode,
                CompanySize = dto.CompanySize,
                Country = dto.Country,
                UserId = user.Id,
            };

            await companyUnitOfWork.Entity.AddAsync(company);
            companyUnitOfWork.Save();
            return Ok("Added Successfully");
        }

        [HttpPut("UpdateCompany/{id}")]
        public async Task<IActionResult> UpdateCompany(string id, CompanyDTO dto)
        {
           
            var company = await companyUnitOfWork.Entity.GetAsync(id);

            if (company == null)
            {
                return NotFound();
            }
            company.Name = dto.Name;
            company.Industry = dto.Industry;
            company.CompanyCode = dto.CompanyCode;
            company.CompanySize = dto.CompanySize;
            company.Country = dto.Country;
            await companyUnitOfWork.Entity.UpdateAsync(company);
            companyUnitOfWork.Save();
            return Ok(company);

        }

        [HttpDelete("DeleteCompany/{id}")]
        public async Task<IActionResult> DeleteCompany(string id)
        {
            var company = await companyUnitOfWork.Entity.GetAsync(id);

            if (company == null)
            {
                return NotFound();

            }
            companyUnitOfWork.Entity.Delete(company);
            companyUnitOfWork.Save();
            return Ok(company);
        }
    }
}