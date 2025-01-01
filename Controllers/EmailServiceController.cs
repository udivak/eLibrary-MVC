using System.Net;
using System.Net.Mail;
using eLibrary.Models;
using Microsoft.Extensions.Options;

namespace eLibrary.Controllers;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body);
}

public class EmailServiceController : IEmailService
{
    private readonly EmailSettings _emailSettings;
    
    public EmailServiceController(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        _emailSettings.SenderEmail = "ireadit21@gmail.com";
        _emailSettings.SenderPassword = "crtw pxts bluy rxie\n";
        _emailSettings.SmtpHost = "smtp.gmail.com";
        _emailSettings.SmtpPort = 587;
        _emailSettings.EnableSsl = true;

        using (var message = new MailMessage())
        {
            message.From = new MailAddress(_emailSettings.SenderEmail);
            message.To.Add(new MailAddress(toEmail));
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            using (var smtpClient = new SmtpClient())
            {
                smtpClient.Host = _emailSettings.SmtpHost;
                smtpClient.Port = _emailSettings.SmtpPort;
                smtpClient.Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.SenderPassword);
                smtpClient.EnableSsl = _emailSettings.EnableSsl;

                await smtpClient.SendMailAsync(message);
            }
        }
    }
}