using System.ComponentModel.DataAnnotations;

namespace ShowcaseWebsite.ViewModels;

public class RegisterViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "E-mailadres")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Weergavenaam")]
    [MaxLength(50)]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Wachtwoord")]
    [MinLength(8, ErrorMessage = "Minimaal 8 tekens.")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Herhaal wachtwoord")]
    [Compare(nameof(Password), ErrorMessage = "Wachtwoorden komen niet overeen.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
