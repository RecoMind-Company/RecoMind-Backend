using Authentication.Core.Interfaces;
using Authentication.Core.Models;

namespace Authentication.Core.Services;
public interface IVerificationEmailService
{
    Task SendVerificationCodeEmail(string email);
}
public class VerificationEmailService(IUnitOfWork<VerificationCode> codeUnitOfWork, IVerificationService verificationService, IEmailSender emailSender) : IVerificationEmailService
{
    public async Task SendVerificationCodeEmail(string email)
    {
        VerificationCode verificationCode = await codeUnitOfWork.Entity.Find(x => x.Email == email);
        string verificationCodeValue;
        if (verificationCode is not null && verificationCode.IsActive)
        {
            verificationCodeValue = verificationCode.Code;
        }
        // Update code if isn't active
        else if (verificationCode is not null)
        {
            verificationCodeValue = verificationService.GenerateCode(4);
            verificationCode.Code = verificationCodeValue;
            verificationCode.CreateAt = DateTime.UtcNow;
            codeUnitOfWork.Entity.UpdateAsync(verificationCode);
            await codeUnitOfWork.Save();
        }
        else
        {
            verificationCodeValue = verificationService.GenerateCode(4);
            await verificationService.SaveCode(verificationCodeValue, email);
        }
        var subject = "Recomind.com - Account Verification!";
        var body = $@"
            Welcome to Recomind.com! 
            This is your Verification Code {verificationCodeValue},
            Please enter this code to verify your identity. 
            'DO NOT SHARE IT' with anyone";
        await emailSender.SendEmailAsync(email, subject, body);
    }
}
