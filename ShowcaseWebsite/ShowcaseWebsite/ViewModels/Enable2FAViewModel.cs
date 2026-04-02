namespace ShowcaseWebsite.ViewModels;

public class Enable2FAViewModel
{
    /// <summary>De gedeelde sleutel als leesbare string voor de gebruiker.</summary>
    public string SharedKey { get; set; } = string.Empty;

    /// <summary>De authenticator URI die als QR-code getoond wordt.</summary>
    public string AuthenticatorUri { get; set; } = string.Empty;
}
