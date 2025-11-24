namespace Authentication.Core.Interfaces;

public interface IEmailSender
{
    Task SendEmailAsync(string email, string subject, string body);
}
