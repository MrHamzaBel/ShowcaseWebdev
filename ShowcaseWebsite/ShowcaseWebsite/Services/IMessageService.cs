using ShowcaseWebsite.Models;

namespace ShowcaseWebsite.Services;

/// <summary>
/// Abstractie voor CRUD op chat-berichten.
/// Maakt unit-testen via Moq mogelijk.
/// </summary>
public interface IMessageService
{
    Task<List<Message>> GetRecentAsync(int count = 50);
    Task<Message> SaveAsync(string userName, string content);
    Task<List<Message>> GetByUserAsync(string userName);
}
