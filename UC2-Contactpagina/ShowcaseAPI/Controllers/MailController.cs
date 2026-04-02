using Microsoft.AspNetCore.Mvc;
using ShowcaseAPI.Models;
using System.Net;
using System.Net.Mail;

namespace ShowcaseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MailController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ILogger<MailController> _logger;

        public MailController(IConfiguration config, ILogger<MailController> logger)
        {
            _config = config;
            _logger = logger;
        }

        // POST api/mail
        // Ontvangt contactformulier-data en stuurt een mail via Mailtrap (sandbox)
        [HttpPost]
        public ActionResult Post([Bind("FirstName, LastName, Email, Phone")] Contactform form)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Lees Mailtrap-credentials uit configuratie (user-secrets in dev, env vars in prod)
                var host = _config["Mailtrap:Host"] ?? "sandbox.smtp.mailtrap.io";
                var port = int.Parse(_config["Mailtrap:Port"] ?? "2525");
                var username = _config["Mailtrap:Username"]
                    ?? throw new InvalidOperationException("Mailtrap:Username niet geconfigureerd");
                var password = _config["Mailtrap:Password"]
                    ?? throw new InvalidOperationException("Mailtrap:Password niet geconfigureerd");
                var to = _config["Mailtrap:ToAddress"] ?? "ontvanger@example.com";

                var body = $"Nieuw contactverzoek:\n\n" +
                           $"Naam:     {form.FirstName} {form.LastName}\n" +
                           $"E-mail:   {form.Email}\n" +
                           $"Telefoon: {form.Phone}";

                using var client = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = true
                };

                client.Send("noreply@showcase.nl", to, "Contactformulier inzending", body);

                _logger.LogInformation("Mail verstuurd voor {Email}", form.Email);
                return Ok();
            }
            catch (Exception ex)
            {
                // Log de fout intern; stuur GEEN details naar de client (geen stack trace)
                _logger.LogError(ex, "Fout bij versturen mail");
                return StatusCode(500, "Er is een fout opgetreden bij het versturen van de mail.");
            }
        }
    }
}
