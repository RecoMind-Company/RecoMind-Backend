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
    public class DatabaseSettingController : ControllerBase
    {
        private readonly IUnitOfWork<DatabaseSettings> dataBaseSettingUnitOfWork;
        private readonly UserManager<AppUser> userManager;



        public DatabaseSettingController(IUnitOfWork<DatabaseSettings> DataBaseSettingUnitOfWork, UserManager<AppUser> userManager)
        {
            dataBaseSettingUnitOfWork = DataBaseSettingUnitOfWork;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetDatabaseSettings()
        {

            var databasesetting = await dataBaseSettingUnitOfWork.Entity.GetAllAsync();
            return Ok(databasesetting);
        }

        [HttpGet("GetDatabaseSetting/{id}")]
        public async Task<IActionResult> GetDatabaseSetting(string id)
        {

            var databasesetting =await dataBaseSettingUnitOfWork.Entity.GetAsync(id);
            return Ok(databasesetting);
        }

        [HttpGet("GetDatabaseSettingsCompany/{componyId}")]
        public async Task<IActionResult> GetDatabaseSettingsCompany(string componyId)
        {

            var databasesetting = await dataBaseSettingUnitOfWork.Entity.FindAll(x => x.CompanyId == componyId);
            return Ok(databasesetting);
        }

        [HttpPost("addDatabaseSetting")]
        public async Task<IActionResult> addDatabaseSetting(DatabaseSettingsDTO DTO)
        {

            if (!ModelState.IsValid)
                return BadRequest();

            var userId = await userManager.GetUserAsync(User);

            var databasesetting = new DatabaseSettings
            {
                Id = Guid.NewGuid().ToString(),
                Server = DTO.Server,
                DatabaseName = DTO.DatabaseName,
                Password = DTO.Password,
                User = DTO.User,
                CompanyId = DTO.CompanyId,  
            };
            await dataBaseSettingUnitOfWork.Entity.AddAsync(databasesetting);
            dataBaseSettingUnitOfWork.Save();
            return Ok(databasesetting);

        }

        [HttpPut("UpdateDatabaseSettings/{id}")]
        public async Task<IActionResult> UpdateDatabaseSettings(string id, DatabaseSettingsDTO dbSetting)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var databaseSettings = await dataBaseSettingUnitOfWork.Entity.GetAsync(id);
            if (databaseSettings == null)
                return NotFound("Database Setting not found");

            databaseSettings.DatabaseName = dbSetting.DatabaseName;
            databaseSettings.Password = dbSetting.Password;
            databaseSettings.User = dbSetting.User;
            databaseSettings.Server = dbSetting.Server;

            await dataBaseSettingUnitOfWork.Entity.UpdateAsync(databaseSettings);
            dataBaseSettingUnitOfWork.Save();

            return Ok(databaseSettings);
        }

        [HttpDelete("DeleteDatabaseSetting/{id}")]
        public async Task<IActionResult> DeleteDatabaseSetting(string id)
        {
            var databaseSetting = await dataBaseSettingUnitOfWork.Entity.GetAsync(id);

            if (databaseSetting == null)
            {
                return NotFound();
            }
            dataBaseSettingUnitOfWork.Entity.Delete(databaseSetting);
            dataBaseSettingUnitOfWork.Save();
            return Ok(databaseSetting);


        }
    }
}