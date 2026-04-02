using System.ComponentModel.DataAnnotations;

namespace Showcase_Chat.ViewModels
{
    public class Verify2FAViewModel
    {
        [Required(ErrorMessage = "Verificatiecode is verplicht")]
        [StringLength(7, MinimumLength = 6, ErrorMessage = "Code moet 6 cijfers bevatten")]
        [Display(Name = "Authenticatorcode")]
        public string Code { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
}
