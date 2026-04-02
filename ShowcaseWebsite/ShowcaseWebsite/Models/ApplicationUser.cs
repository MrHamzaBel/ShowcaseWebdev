using Microsoft.AspNetCore.Identity;

namespace ShowcaseWebsite.Models;

/// <summary>
/// Uitbreid IdentityUser met eventuele extra profielvelden.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
}
