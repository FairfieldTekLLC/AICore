using Microsoft.AspNetCore.SignalR;

namespace AICore.Hubs
{
    public class ChatHub : Hub
    {
        //public async Task SendMessage(string user, string message)
        //{
        //    await Clients.All.SendAsync("ReceiveMessage", user, message);
        //}

        public async Task SendMessage(string sender, string receiver, string message)
        {
            // Log message sending
            Console.WriteLine($"[SendMessage] {sender} -> {receiver}: {message}");

            // Send the message only to the recipient
            await Clients.Group(receiver).SendAsync("ReceiveMessage", sender, message);
        }

        public override async Task OnConnectedAsync()
        {
            var username = Context.GetHttpContext()?.Request.Query["username"];
            if (!string.IsNullOrEmpty(username))
            {
                // Add user to a group
                Console.WriteLine($"[OnConnected] User connected: {username}");
                await Groups.AddToGroupAsync(Context.ConnectionId, username);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var username = Context.GetHttpContext()?.Request.Query["username"];
            if (!string.IsNullOrEmpty(username))
            {
                // Remove user from a group
                Console.WriteLine($"[OnDisconnected] User disconnected: {username}");
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, username);
            }
            await base.OnDisconnectedAsync(exception);
        }
        
    }
}
