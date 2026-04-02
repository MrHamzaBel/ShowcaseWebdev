using System.ComponentModel.DataAnnotations;

namespace Showcase_Chat.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Weergavenaam is verplicht")]
        [StringLength(50)]
        [Display(Name = "Weergavenaam")]
        public string DisplayName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-mailadres is verplicht")]
        [EmailAddress(ErrorMessage = "Ongeldig e-mailadres")]
        [Display(Name = "E-mailadres")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Wachtwoord is verplicht")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Wachtwoord moet minimaal 8 tekens bevatten")]
        [DataType(DataType.Password)]
        [Display(Name = "Wachtwoord")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bevestig je wachtwoord")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Wachtwoorden komen niet overeen")]
        [Display(Name = "Bevestig wachtwoord")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
