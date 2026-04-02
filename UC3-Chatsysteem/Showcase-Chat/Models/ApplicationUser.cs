using Microsoft.AspNetCore.Identity;

namespace Showcase_Chat.Models
{
    // Uitbreiding van IdentityUser met een zichtbare displaynaam
    public class ApplicationUser : IdentityUser
    {
        public string DisplayName { get; set; } = string.Empty;
    }
}
