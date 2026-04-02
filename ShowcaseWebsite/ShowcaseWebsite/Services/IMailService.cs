namespace ShowcaseWebsite.Services;

/// <summary>
/// Abstractie voor het versturen van e-mail.
/// Injecteerbaar en vervangbaar door een nep-implementatie in tests.
/// </summary>
public interface IMailService
{
    Task SendAsync(string fromName, string fromEmail, string subject, string body);
}
