using Microsoft.AspNetCore.SignalR;

namespace Web_API.Hubs
{
    public class NotificationHub : Hub
    {
        // Method for clients to receive notifications
        public async Task SendNotification(string userId, string message)
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", message);
        }

    }
}
