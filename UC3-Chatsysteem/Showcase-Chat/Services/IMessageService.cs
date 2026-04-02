using Showcase_Chat.Models;

namespace Showcase_Chat.Services
{
    public interface IMessageService
    {
        // Retourneert de laatste berichten voor alle users (max. 100)
        Task<IEnumerable<Message>> GetAllMessagesAsync();

        // Retourneert berichten gefilterd op een specifieke gebruiker (Admin-only)
        Task<IEnumerable<Message>> GetMessagesByUserAsync(string userId);

        // Slaat een nieuw bericht op en retourneert het opgeslagen object
        Task<Message> SendMessageAsync(string senderId, string content);
    }
}
