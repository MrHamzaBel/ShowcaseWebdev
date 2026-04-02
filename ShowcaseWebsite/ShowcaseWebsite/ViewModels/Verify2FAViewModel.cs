using System.ComponentModel.DataAnnotations;

namespace ShowcaseWebsite.ViewModels;

public class Verify2FAViewModel
{
    [Required]
    [StringLength(7, MinimumLength = 6)]
    [DataType(DataType.Text)]
    [Display(Name = "Verificatiecode")]
    public string Code { get; set; } = string.Empty;
}
