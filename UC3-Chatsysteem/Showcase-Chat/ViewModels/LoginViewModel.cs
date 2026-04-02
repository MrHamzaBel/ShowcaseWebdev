using System.ComponentModel.DataAnnotations;

namespace Showcase_Chat.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "E-mailadres is verplicht")]
        [EmailAddress]
        [Display(Name = "E-mailadres")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Wachtwoord is verplicht")]
        [DataType(DataType.Password)]
        [Display(Name = "Wachtwoord")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Onthoud mij")]
        public bool RememberMe { get; set; }
    }
}
