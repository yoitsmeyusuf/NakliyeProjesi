namespace NakliyeApp.Services;
using System.Net;
using System.Net.Mail;
using NakliyeApp.Models;

public interface IEmailService
{
    Task SendEmailConfirmationAsync(AppUser user, string verificationLink);
    Task SendPasswordResetAsync(AppUser user, string resetLink);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = _logger;
    }

    public async Task SendEmailConfirmationAsync(AppUser user, string verificationLink)
    {
        var subject = "E-posta Doğrulama";
        var body = $"""
            Merhaba {user.FullName},<br/>
            Lütfen e-posta adresinizi doğrulamak için <a href="{verificationLink}">buraya tıklayın</a>.
        """;

        await SendEmailAsync(user.Email, subject, body);
    }

    public async Task SendPasswordResetAsync(AppUser user, string resetLink)
    {
        var subject = "Şifre Sıfırlama";
        var body = $"""
            Merhaba {user.FullName},<br/>
            Şifrenizi sıfırlamak için <a href="{resetLink}">buraya tıklayın</a>. Link 1 saat geçerlidir.
        """;

        await SendEmailAsync(user.Email, subject, body);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var smtpClient = new SmtpClient(_config["Smtp:Host"])
        {
            Port = int.Parse(_config["Smtp:Port"]),
            Credentials = new NetworkCredential(_config["Smtp:Username"], _config["Smtp:Password"]),
            EnableSsl = true,
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_config["Smtp:From"]),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true,
        };
        mailMessage.To.Add(toEmail);

        try
        {
            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation("E-posta başarıyla gönderildi: {ToEmail}", toEmail);
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "SMTP hatası: {Message}", ex.Message);
            throw new Exception("E-posta gönderilemedi. Lütfen daha sonra tekrar deneyin.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "E-posta gönderim hatası: {Message}", ex.Message);
            throw;
        }
    }
}
