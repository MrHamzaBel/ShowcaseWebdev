using System.ComponentModel.DataAnnotations;

namespace ShowcaseWebsite.Models;

/// <summary>
/// Chatbericht dat opgeslagen wordt in de database.
/// </summary>
public class Message
{
    public int Id { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Content { get; set; } = string.Empty;

    [Required]
    public string UserName { get; set; } = string.Empty;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
