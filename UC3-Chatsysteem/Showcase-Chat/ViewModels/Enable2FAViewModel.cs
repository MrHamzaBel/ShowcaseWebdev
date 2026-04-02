namespace Showcase_Chat.ViewModels
{
    public class Enable2FAViewModel
    {
        // Base32-encoded secret key die in de authenticator app gescand wordt
        public string SharedKey { get; set; } = string.Empty;

        // QR-code als base64 PNG data URL
        public string QrCodeImageUrl { get; set; } = string.Empty;

        // otpauth:// URI voor de authenticator app
        public string AuthenticatorUri { get; set; } = string.Empty;
    }
}
