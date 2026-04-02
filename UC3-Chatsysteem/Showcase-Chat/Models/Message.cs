using System.ComponentModel.DataAnnotations;

namespace Showcase_Chat.Models
{
    public class Message
    {
        public int Id { get; set; }

        // Maximaal 1000 tekens; EF Core gebruikt dit als column constraint
        [Required]
        [StringLength(1000)]
        public string Content { get; set; } = string.Empty;

        // Altijd UTC opslaan
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign key naar ApplicationUser
        [Required]
        public string SenderId { get; set; } = string.Empty;
        public ApplicationUser Sender { get; set; } = null!;
    }
}
