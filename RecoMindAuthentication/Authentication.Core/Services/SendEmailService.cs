using Authentication.Core.Interfaces;
using Authentication.Core.Models;
using Authentication.Core.Settings;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Authentication.Core.Services;
public interface ISendEmailService
{
    Task SendEmailAsync(string email);
}

public class SendEmailService(IOptions<EmailConfgSettings> options, IUnitOfWork<VerificationCode> codeUnitOfWork, IVerificationService verificationService) : ISendEmailService
{
    private readonly EmailConfgSettings _emailConfigSettings = options.Value;
    public async Task SendEmailAsync(string email)
    {
        try
        {
            var senderEmail = _emailConfigSettings.Email;
            var senderPassword = _emailConfigSettings.Password;
            var host = _emailConfigSettings.Host;
            var port = _emailConfigSettings.Port;

            var smtpClient = new SmtpClient(host, port);
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);

            string verificationCode;
            if (await codeUnitOfWork.Entity.Any(e => e.Email == email))
            {
                verificationCode = codeUnitOfWork.Entity.Find(x => x.Email == email).Code;
            }
            else
            {
                verificationCode = verificationService.GenerateCode(4);
                await verificationService.SaveCode(verificationCode, email);

            }
            // Generate a verification code

            var message = new MailMessage(from: senderEmail, to: email, subject: "Verification", body: $"Verifciation Code {verificationCode}, Please enter this code to verify your identity. Do not share it with anyone");
            await smtpClient.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }
}
