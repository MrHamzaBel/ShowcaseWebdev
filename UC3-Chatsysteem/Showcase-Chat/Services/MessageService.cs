using Microsoft.EntityFrameworkCore;
using Showcase_Chat.Data;
using Showcase_Chat.Models;

namespace Showcase_Chat.Services
{
    public class MessageService : IMessageService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<MessageService> _logger;

        public MessageService(AppDbContext db, ILogger<MessageService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<IEnumerable<Message>> GetAllMessagesAsync()
        {
            // Haal laatste 100 berichten op met senderinfo, nieuwste eerst
            return await _db.Messages
                .Include(m => m.Sender)
                .OrderByDescending(m => m.CreatedAt)
                .Take(100)
                .ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetMessagesByUserAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("userId mag niet leeg zijn", nameof(userId));

            return await _db.Messages
                .Include(m => m.Sender)
                .Where(m => m.SenderId == userId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<Message> SendMessageAsync(string senderId, string content)
        {
            // Valideer server-side: content mag nooit leeg of te lang zijn
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Bericht mag niet leeg zijn", nameof(content));

            if (content.Length > 1000)
                throw new ArgumentException("Bericht mag maximaal 1000 tekens bevatten", nameof(content));

            var message = new Message
            {
                Content = content.Trim(),
                SenderId = senderId,
                CreatedAt = DateTime.UtcNow
            };

            _db.Messages.Add(message);
            await _db.SaveChangesAsync();

            // Laad de sender na opslaan voor SignalR response
            await _db.Entry(message).Reference(m => m.Sender).LoadAsync();

            _logger.LogInformation("Bericht opgeslagen van gebruiker {SenderId}", senderId);
            return message;
        }
    }
}
