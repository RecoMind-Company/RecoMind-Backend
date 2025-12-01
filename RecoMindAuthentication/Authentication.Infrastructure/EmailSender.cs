using Authentication.Core.Interfaces;
using Authentication.Core.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Authentication.Infrastructure;
public class EmailSender(IOptions<EmailConfgSettings> options, ILogger<EmailSender> logger) : IEmailSender
{
    private readonly EmailConfgSettings _emailConfigSettings = options.Value;
    public async Task SendEmailAsync(string email, string subject, string body)
    {
        try
        {
            var senderEmail = _emailConfigSettings.Email;
            var password = _emailConfigSettings.Password;
            var host = _emailConfigSettings.Host;
            var port = _emailConfigSettings.Port;

            var smtpClient = new SmtpClient(host, port);
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(senderEmail, password);

            var message = new MailMessage(from: senderEmail, to: email, subject, body);

            await smtpClient.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {Email}.", email);
            throw;
        }
    }
}
