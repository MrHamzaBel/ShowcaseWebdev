using System.ComponentModel.DataAnnotations;

namespace ShowcaseWebsite.ViewModels;

public class LoginViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "E-mailadres")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Wachtwoord")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Onthoud mij")]
    public bool RememberMe { get; set; }
}
