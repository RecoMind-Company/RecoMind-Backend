using Authentication.Core.DTOs;
using Authentication.Core.Models;
using Authentication.Core.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Authentication.Core.Services;

public interface IAccountService
{
    Task<BaseToReturnDto> EditProfile(ProfileDto profile, string email);
    Task<ProfileToReturnDto> GetProfile(string email);
    Task<BaseToReturnDto> DeleteUser(string id);
}

public class AccountService(UserManager<AppUser> userManager, IWebHostEnvironment env, IOptions<PhotoSettings> options) : IAccountService
{
    private readonly PhotoSettings photoSettings = options.Value;


    public async Task<BaseToReturnDto> EditProfile(ProfileDto profile, string email)
    {
        // Get user by email
        var user = await userManager.FindByEmailAsync(email);
        // validate user
        if (user is null)
            return new BaseToReturnDto { Message = "this user is not exsist!" };

        string photoUrl = user.PhotoUrl;

        // Check if the photo feild is not null then send it to PhotoHandler
        if (profile.Photo is not null)
        {
            var CreatePhoto = await PhotoHandler(profile.Photo, env);
            if (!CreatePhoto.Success)
                return new BaseToReturnDto { Message = CreatePhoto.Message };
            photoUrl = CreatePhoto.Message;
        }

        // Update the user 
        user.FullName = profile.Name ?? user.FullName;
        user.Email = profile.Email ?? user.Email;
        user.PhotoUrl = photoUrl;
        user.PhoneNumber = profile.Phone ?? user.PhoneNumber;
        var updateUser = await userManager.UpdateAsync(user);

        // check if success 
        if (!updateUser.Succeeded)
            return new BaseToReturnDto { Message = string.Join(',', updateUser.Errors) };

        // return BaseToReturnDto
        return new BaseToReturnDto { Success = true, Message = "User profile updated successfully" };
    }

    public async Task<ProfileToReturnDto> GetProfile(string email)
    {
        var profileToReturn = new ProfileToReturnDto();

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return null;

        profileToReturn.Email = user.Email;
        profileToReturn.Name = user.FullName;

        if (user.PhotoUrl is not null)
            profileToReturn.Photo = photoSettings.BaseUrl + user.PhotoUrl;

        if (user.PhoneNumber is not null)
            profileToReturn.Phone = user.PhoneNumber;
        return profileToReturn;

    }

    private async Task<BaseToReturnDto> PhotoHandler(IFormFile file, IWebHostEnvironment env)
    {
        // Validate file extension
        List<string> validExstensions = new List<string> { ".jpg", ".png" };
        var fileExtension = Path.GetExtension(file.FileName);
        if (!validExstensions.Contains(fileExtension))
            return new BaseToReturnDto { Message = $"This photo extension is not supported please add file with these extensions {string.Join(',', validExstensions)}" };
        // validate file size
        long size = file.Length;
        if (size > (5 * 1024 * 1024))
            return new BaseToReturnDto { Message = "Photo is larger than 5mb!" };
        // Create file name and save it to the folder
        string fileName = Guid.NewGuid().ToString() + fileExtension;
        string physicalPathToSave = Path.Combine(env.ContentRootPath, "StaticFiles", "Images");
        using var stream = new FileStream(Path.Combine(physicalPathToSave, fileName), FileMode.Create);
        await file.CopyToAsync(stream);
        // Create and return the dynamic path to save it in database
        var dynamicPath = Path.Combine(photoSettings.VirtualPathUrl, fileName);
        return new BaseToReturnDto { Success = true, Message = dynamicPath };
    }
    public async Task<BaseToReturnDto> DeleteUser(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
            return new BaseToReturnDto { Message = "this user is not exsist!" };
        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
            return new BaseToReturnDto { Message = string.Join(',', result.Errors) };
        return new BaseToReturnDto { Success = true, Message = "User deleted successfully" };
    }
}
