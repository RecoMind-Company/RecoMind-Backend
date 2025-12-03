using Authentication.Core.DTOs;
using Authentication.Core.Interfaces;
using Authentication.Core.Models;
using System.Security.Cryptography;

namespace Authentication.Core.Services;

public interface IVerificationService
{
    string GenerateCode(int length);
    Task SaveCode(string code, string email);
    Task<VerificationCode?> GetCodeByEmail(string email);
    Task<BaseToReturnDto> IsCodeValid(string code, string email);
    //Task<verificationCodeToReturnDto> VerifiyCode(string code, string email);
}

public class VerificationService(IUnitOfWork<VerificationCode> unitOfWork) : IVerificationService
{
    private readonly IGenericRepository<VerificationCode> CodeRepo = unitOfWork.Entity;
    public string GenerateCode(int length)
    {
        var code = new char[length];
        for (int i = 0; i < length; i++)
        {
            int randomNumber = RandomNumberGenerator.GetInt32(0, 10);
            code[i] = (char)('0' + randomNumber);
        }
        return new string(code);
    }
    public async Task SaveCode(string code, string email)
    {
        var varificationCode = new VerificationCode { Code = code, Email = email, CreateAt = DateTime.UtcNow };
        await CodeRepo.AddAsync(varificationCode);
        await unitOfWork.Save();

    }

    public async Task<VerificationCode?> GetCodeByEmail(string email)
    {
        var code = await CodeRepo.Find(e => e.Email == email);
        return code;
    }

    public async Task<BaseToReturnDto> IsCodeValid(string code, string email)
    {
        var validCode = await GetCodeByEmail(email);
        if (validCode is not null && validCode.IsActive && validCode.Code == code)
        {
            // Can remove the code after validation
            CodeRepo.Delete(validCode);
            await unitOfWork.Save();
            return new BaseToReturnDto { Success = true };
        }
        return new BaseToReturnDto { Message = "The code is Invalid!" };
    }


}
