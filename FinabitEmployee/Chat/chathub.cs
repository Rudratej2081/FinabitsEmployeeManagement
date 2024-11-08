using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Threading.Tasks;

public class ChatHub : Hub
{
    // Use ConcurrentDictionary for thread-safe access to connected users
    private static readonly ConcurrentDictionary<string, string> ConnectedUsers = new ConcurrentDictionary<string, string>();

    // When a user connects, map their connection ID to their username or email
    public override Task OnConnectedAsync()
    {
        var userEmail = Context.User.Identity.Name;

        // Add the user connection if not already connected
        ConnectedUsers.TryAdd(userEmail, Context.ConnectionId);

        // Optionally, notify all users about the new connection
        return Clients.All.SendAsync("UserConnected", userEmail);
    }

    // When a user disconnects, remove their mapping
    public override Task OnDisconnectedAsync(Exception exception)
    {
        var userEmail = Context.User.Identity.Name;

        // Remove the user connection
        ConnectedUsers.TryRemove(userEmail, out _);

        // Optionally, notify all users about the disconnection
        return Clients.All.SendAsync("UserDisconnected", userEmail);
    }

    // Send private message to a specific user by their email
    public async Task SendPrivateMessage(string toUserEmail, string message)
    {
        if (ConnectedUsers.TryGetValue(toUserEmail, out var connectionId))
        {
            // Send message to the target user
            await Clients.Client(connectionId).SendAsync("ReceiveMessage", Context.User.Identity.Name, message);
        }
        else
        {
            // Handle the case when the user is not connected
            await Clients.Caller.SendAsync("ReceiveMessage", "System", $"{toUserEmail} is not connected. The message will be sent when they come online.");

            // Optional: Implement logic to save the message to a database for later retrieval
            // SaveMessageToDatabase(Context.User.Identity.Name, toUserEmail, message);
        }
    }

    // Optional: Broadcast a message to all connected users
    public async Task SendGroupMessage(string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", "Broadcast", message);
    }

   
     private void SaveMessageToDatabase(string senderEmail, string receiverEmail, string message)
    {
        // Implement database save logic here
    }
}
