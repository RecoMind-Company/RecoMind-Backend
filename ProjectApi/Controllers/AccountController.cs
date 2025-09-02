using Core.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectAPI.DTO;
using System.Net;

namespace ProjectAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize("UserRole")]
    public class AccountController : ControllerBase
    {

        private readonly UserManager<AppUser> userManager;

        public AccountController(UserManager<AppUser> userManager )
        {
           
            this.userManager = userManager;

        }


        [HttpGet("GetUsers")]
        [Authorize("AdminRole")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await userManager.Users.ToListAsync();
            if (users == null)
                return NotFound("Not Found Any Users");

            var user = users.Select(x => new GetUsersDTO
            {
                Id = x.Id,
                Email = x.Email,
                FirstName = x.Name.Split(" ").First(),
                LastName = x.Name.Split(" ").Last(),
                PhoneNamber = x.PhoneNumber,
                UrlPhoto = x.Photo,
            }).ToList();

            return Ok(user);
        }


        [HttpGet("GetUser")]
        public async Task<IActionResult> GetUser()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return NotFound("This User Not Registed");

            //long compressedSize = new FileInfo($"wwwroot/ProfilePhoto/{user.Photo}").Length;

            var mapUser = new GetUsersDTO
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.Name.Split(" ").First(),
                LastName = user.Name.Split(" ").Last(),

                PhoneNamber = user.PhoneNumber,
                UrlPhoto = user.Photo,

            };
            return Ok(mapUser);
        }

        [HttpPut("EditUser")]
        public async Task<IActionResult> UpdateUser(UpdateUserDTO dto)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return NotFound("This User Not Registed");


            //long originalSize = dto.Photo.Length;

            //if (dto.Photo != null)
            //{
            //    var compressedImage = await service.CompressAndSaveImageAsync(dto.Photo, "ProfilePhoto", 800, 50);
            //    user.Photo = compressedImage;
            //}

            user.Email = dto.Email ?? user.Email;
            user.Name = dto.FirstName + " " + dto.LastName ?? user.Name ;
            user.PhoneNumber = dto.PhoneNamber ?? user.PhoneNumber;

            await userManager.UpdateAsync(user);
            var mapUser = new GetUsersDTO
            {
                Id = user.Id,

                Email = user.Email,
                FirstName = user.Name.Split(" ").First(),
                LastName = user.Name.Split(" ").Last(),
                PhoneNamber = user.PhoneNumber,
                UrlPhoto = user.Photo,

            };

            return Ok(mapUser);
        }


        [HttpDelete]
        public async Task<IActionResult> DeleteUser()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null) 
                return NotFound("This User Not Registed");

            await userManager.DeleteAsync(user);
            return Ok("User Is Deleted !");



        }
    }
}
