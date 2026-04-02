using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ShowcaseWebsite.Services;

namespace ShowcaseWebsite.Hubs;

/// <summary>
/// SignalR Hub voor realtime chatnachrichten.
/// Alleen ingelogde gebruikers mogen berichten versturen (ASVS V4.1).
/// </summary>
[Authorize]
public class ChatHub : Hub
{
    private readonly IMessageService _messages;

    public ChatHub(IMessageService messages) => _messages = messages;

    /// <summary>
    /// Ontvang een bericht van een client, sla het op en broadcast naar alle verbonden clients.
    /// </summary>
    public async Task SendMessage(string content)
    {
        // Saniteer inhoud (lege berichten afwijzen)
        if (string.IsNullOrWhiteSpace(content) || content.Length > 1000)
            return;

        string userName = Context.User?.Identity?.Name ?? "Onbekend";

        // Sla op in database
        var msg = await _messages.SaveAsync(userName, content);

        // Broadcast naar alle clients
        await Clients.All.SendAsync("ReceiveMessage", new
        {
            userName = msg.UserName,
            content = msg.Content,
            sentAt = msg.SentAt.ToString("HH:mm")
        });
    }
}
