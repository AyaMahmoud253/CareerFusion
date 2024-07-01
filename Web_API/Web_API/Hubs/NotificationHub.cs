using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace Web_API.Hubs
{
    public class NotificationHub : Hub
    {
        // Store user connection information
        public static ConcurrentDictionary<string, string> UserConnections = new ConcurrentDictionary<string, string>();

        // Method for clients to receive notifications
        public async Task SendNotification(string userId, string message)
        {
            if (UserConnections.TryGetValue(userId, out string connectionId))
            {
                await Clients.Client(connectionId).SendAsync("ReceiveNotification", message);
            }
        }

        // Method called when a client connects
        public override async Task OnConnectedAsync()
        {
            string userId = Context.UserIdentifier;
            string connectionId = Context.ConnectionId;

            if (!string.IsNullOrEmpty(userId))
            {
                UserConnections[userId] = connectionId;
            }

            await base.OnConnectedAsync();
        }

        // Method called when a client disconnects
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string userId = Context.UserIdentifier;

            if (!string.IsNullOrEmpty(userId))
            {
                UserConnections.TryRemove(userId, out _);
            }

            await base.OnDisconnectedAsync(exception);
        }

    }
}
