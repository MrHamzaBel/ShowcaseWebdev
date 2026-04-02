using System.Net;
using System.Net.Mail;

namespace ShowcaseWebsite.Services;

/// <summary>
/// Verstuurt e-mail via Mailtrap SMTP.
/// Credentials komen uit user-secrets of appsettings (nooit hardcoded).
/// </summary>
public class MailService : IMailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<MailService> _logger;

    public MailService(IConfiguration config, ILogger<MailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAsync(string fromName, string fromEmail, string subject, string body)
    {
        // Lees SMTP-instellingen uit configuratie
        string host = _config["Mailtrap:Host"] ?? "sandbox.smtp.mailtrap.io";
        int port = int.Parse(_config["Mailtrap:Port"] ?? "587");
        string username = _config["Mailtrap:Username"] ?? "";
        string password = _config["Mailtrap:Password"] ?? "";
        string toEmail = _config["Mailtrap:ToEmail"] ?? "ontvanger@showcase.nl";

        using var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(username, password),
            EnableSsl = true
        };

        var mail = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };
        mail.To.Add(toEmail);

        try
        {
            await client.SendMailAsync(mail);
            _logger.LogInformation("Mail verstuurd van {From} naar {To}", fromEmail, toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mail versturen mislukt");
            throw;
        }
    }
}
