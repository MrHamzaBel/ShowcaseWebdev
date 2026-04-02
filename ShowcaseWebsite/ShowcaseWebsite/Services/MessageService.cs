using Microsoft.EntityFrameworkCore;
using ShowcaseWebsite.Data;
using ShowcaseWebsite.Models;

namespace ShowcaseWebsite.Services;

/// <summary>
/// Slaat berichten op in SQLite via EF Core.
/// </summary>
public class MessageService : IMessageService
{
    private readonly AppDbContext _db;

    public MessageService(AppDbContext db) => _db = db;

    public async Task<List<Message>> GetRecentAsync(int count = 50)
        => await _db.Messages
            .OrderByDescending(m => m.SentAt)
            .Take(count)
            .OrderBy(m => m.SentAt)
            .ToListAsync();

    public async Task<Message> SaveAsync(string userName, string content)
    {
        // XSS-preventie: we vertrouwen de input niet – Razor encode automatisch bij render
        var msg = new Message
        {
            UserName = userName,
            Content = content,
            SentAt = DateTime.UtcNow
        };
        _db.Messages.Add(msg);
        await _db.SaveChangesAsync();
        return msg;
    }

    public async Task<List<Message>> GetByUserAsync(string userName)
        => await _db.Messages
            .Where(m => m.UserName == userName)
            .OrderBy(m => m.SentAt)
            .ToListAsync();
}
