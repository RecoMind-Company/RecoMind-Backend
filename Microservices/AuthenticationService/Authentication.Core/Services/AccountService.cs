using Authentication.Core.DTOs;
using Authentication.Core.Interfaces;
using Authentication.Core.Models;
using Authentication.Core.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Authentication.Core.Services;

public interface IAccountService
{
    Task<BaseToReturnDto> EditProfile(ProfileDto profile, string email);
    Task<ProfileToReturnDto?> GetProfile(string email);
    Task<BaseToReturnDto> DeleteUser(string id);
}

public class AccountService : IAccountService
{
    private readonly PhotoSettings photoSettings;
    private readonly UserManager<AppUser> userManager;
    private readonly IGenericRepository<UsersJobTitle> jobTitleRepo;
    private readonly IUnitOfWork<UsersJobTitle> unitOfWork;

    public AccountService(UserManager<AppUser> userManager,
                                IOptions<PhotoSettings> options,
                                IGenericRepository<UsersJobTitle> jobTitleRepo,
                                IUnitOfWork<UsersJobTitle> unitOfWork)
    {
        this.userManager = userManager;
        this.jobTitleRepo = jobTitleRepo;
        this.unitOfWork = unitOfWork;
        photoSettings = options.Value;
    }

    public async Task<BaseToReturnDto> EditProfile(ProfileDto profile, string email)
    {
        // Get user by email
        var user = await userManager.FindByEmailAsync(email);
        // validate user
        if (user is null)
            return new BaseToReturnDto { Message = "this user is not exsist!" };

        string? photoUrl = user.PhotoUrl;

        // Check if the photo feild is not null then send it to PhotoHandler
        if (profile.Photo is not null)
        {
            var CreatePhoto = await PhotoHandler(profile.Photo);
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
        if (profile.JobTitle is not null)
        {
            var userJobTitle = await jobTitleRepo.Find(ut => ut.UserId == user.Id);
            if (userJobTitle is null)
            {
                var newUserJobTitle = new UsersJobTitle
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = user.Id,
                    JobTitle = profile.JobTitle
                };
                await jobTitleRepo.AddAsync(newUserJobTitle);
            }
            else
            {
                userJobTitle.JobTitle = profile.JobTitle;
                jobTitleRepo.UpdateAsync(userJobTitle);
            }
            await unitOfWork.Save();
        }
        // return BaseToReturnDto
        return new BaseToReturnDto { Success = true, Message = "User profile updated successfully" };
    }

    public async Task<ProfileToReturnDto?> GetProfile(string email)
    {
        var profileToReturn = new ProfileToReturnDto();

        var user = await userManager.FindByEmailAsync(email);
        var userJobTitle = await jobTitleRepo.Find(ut => ut.UserId == user!.Id);
        if (user is null)
            return null;

        profileToReturn.Id = user.Id;
        profileToReturn.Email = user.Email ?? string.Empty;
        profileToReturn.Name = user.FullName ?? string.Empty;

        if (user.PhotoUrl is not null)
            profileToReturn.Photo = photoSettings.BaseUrl?.TrimEnd('/') + "/" + user.PhotoUrl.TrimStart('/');

        profileToReturn.Phone = user.PhoneNumber ?? string.Empty;
        profileToReturn.JobTitle = userJobTitle?.JobTitle ?? string.Empty;
        return profileToReturn;

    }

    private async Task<BaseToReturnDto> PhotoHandler(IFormFile file)
    {
        // Validate file extension
        var validExstensions = new List<string> { ".jpg", ".jpeg", ".png" };
        var fileExtension = (Path.GetExtension(file.FileName) ?? string.Empty).ToLowerInvariant();
        if (!validExstensions.Contains(fileExtension))
            return new BaseToReturnDto { Message = $"This photo extension is not supported please add file with these extensions {string.Join(',', validExstensions)}" };
        // validate file size
        long size = file.Length;
        if (size > (5 * 1024 * 1024))
            return new BaseToReturnDto { Message = "Photo is larger than 5mb!" };

        // Create file name and ensure folder exists
        string fileName = Guid.NewGuid().ToString() + fileExtension;
        string physicalPathToSave = Path.Combine(Directory.GetCurrentDirectory(), "StaticFiles", "Images");
        try
        {
            if (!Directory.Exists(physicalPathToSave))
                Directory.CreateDirectory(physicalPathToSave);

            var fullPath = Path.Combine(physicalPathToSave, fileName);
            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);
        }
        catch (Exception ex)
        {
            // return safe message for caller; detailed logs should be written to logs (not shown here)
            return new BaseToReturnDto { Message = "Failed to save photo on server: " + ex.Message };
        }

        // Create and return the dynamic path to save it in database (use URL style path)
        var virtualPath = photoSettings.VirtualPathUrl?.TrimEnd('/') ?? "/UserProfileImage";
        var dynamicPath = $"{virtualPath.TrimEnd('/')}/{fileName}";
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
