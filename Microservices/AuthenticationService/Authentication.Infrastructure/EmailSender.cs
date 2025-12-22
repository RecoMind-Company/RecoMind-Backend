using Authentication.Core.Interfaces;
using Authentication.Core.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Net.Mail;

namespace Authentication.Infrastructure;
public class EmailSender(IOptions<EmailConfgSettings> options, ILogger<EmailSender> logger) : IEmailSender
{
    private readonly EmailConfgSettings _emailConfigSettings = options.Value;
    public async Task SendEmailAsync(string email, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("RECOMIND", _emailConfigSettings.Email));
        message.To.Add(new MailboxAddress(new MailAddress(email).User, email));
        message.Subject = subject;

        // بناء جسم الإيميل (Body Builder)
        var bodyBuilder = new BodyBuilder();

        // رابط الصورة المباشر من ImgBB
        string imageUrl = "https://i.ibb.co/r2cVnWP7/gamil-Banner.png";

        // الـ HTML هنا بسيط ومنظم لضمان الـ Responsive على الموبايل
        bodyBuilder.HtmlBody = $@"
        <div style='max-width: 600px; margin: auto; font-family: Arial, sans-serif; border: 1px solid #eee;'>
            <img src='{imageUrl}' alt='RecoMind Banner' style='width: 100%; height: auto; display: block;' />
            <div style='padding: 20px; color: #333; line-height: 1.6;'>
                {body}
            </div>
            <div style='padding: 10px; text-align: center; font-size: 12px; color: #999; background: #f9f9f9;'>
                © 2025 RecoMind - All Rights Reserved
            </div>
        </div>";

        message.Body = bodyBuilder.ToMessageBody();

        using (var client = new MailKit.Net.Smtp.SmtpClient())
        {
            try
            {
                // الاتصال بالسيرفر (MailKit بيحتاج SecureSocketOptions)
                await client.ConnectAsync(_emailConfigSettings.Host, _emailConfigSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);

                // تسجيل الدخول
                await client.AuthenticateAsync(_emailConfigSettings.Email, _emailConfigSettings.Password);

                // الإرسال
                await client.SendAsync(message);

                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "MailKit failed to send email to {Email}", email);
                throw;
            }
        }
    }
}
