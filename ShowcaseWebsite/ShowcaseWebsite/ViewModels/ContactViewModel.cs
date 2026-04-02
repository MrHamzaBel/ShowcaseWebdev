using System.ComponentModel.DataAnnotations;

namespace ShowcaseWebsite.ViewModels;

public class ContactViewModel
{
    [Required(ErrorMessage = "Naam is verplicht.")]
    [MaxLength(100)]
    [Display(Name = "Naam")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-mailadres is verplicht.")]
    [EmailAddress(ErrorMessage = "Ongeldig e-mailadres.")]
    [Display(Name = "E-mailadres")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Onderwerp is verplicht.")]
    [MaxLength(200)]
    [Display(Name = "Onderwerp")]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bericht is verplicht.")]
    [MaxLength(2000)]
    [Display(Name = "Bericht")]
    public string Message { get; set; } = string.Empty;

    [Range(typeof(bool), "true", "true", ErrorMessage = "U moet akkoord gaan met het privacybeleid.")]
    [Display(Name = "Ik ga akkoord met het privacybeleid")]
    public bool AcceptPrivacy { get; set; }
}
