using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Showcase_Chat.Services;

namespace Showcase_Chat.Hubs
{
    // Alleen ingelogde gebruikers mogen verbinding maken met de hub
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IMessageService _messageService;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(IMessageService messageService, ILogger<ChatHub> logger)
        {
            _messageService = messageService;
            _logger = logger;
        }

        // Wordt aangeroepen vanuit de Vue-component: hub.invoke("SendMessage", content)
        public async Task SendMessage(string content)
        {
            // BELANGRIJK: SenderId altijd van de server halen, nooit van de client!
            // Dit voorkomt dat een gebruiker namens iemand anders kan posten.
            var senderId = Context.UserIdentifier;

            if (string.IsNullOrEmpty(senderId))
            {
                _logger.LogWarning("SendMessage aangeroepen zonder geldig UserIdentifier");
                return;
            }

            try
            {
                var message = await _messageService.SendMessageAsync(senderId, content);

                // Stuur bericht naar ALLE verbonden clients
                await Clients.All.SendAsync(
                    "ReceiveMessage",
                    message.Sender.DisplayName,
                    message.Content,
                    message.CreatedAt.ToString("HH:mm")
                );
            }
            catch (ArgumentException ex)
            {
                // Stuur foutmelding alleen terug naar de verzender
                await Clients.Caller.SendAsync("ReceiveError", ex.Message);
            }
        }
    }
}
