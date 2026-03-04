using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace KsIceCream.Hubs
{
    [Authorize]
    public class ActivityHub : Hub
    {
        // Static dictionary to track connection IDs per user
        // userId -> List of connection IDs
        private static readonly ConcurrentDictionary<string, ConcurrentBag<string>> UserConnections 
            = new();

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst("userId")?.Value ?? "unknown";
            var username = Context.User?.FindFirst("username")?.Value ?? "Unknown";
            var connectionId = Context.ConnectionId;

            // Add tction to the user's connections
            UserConnections.AddOrUpdate(userId,
                new ConcurrentBag<string> { connectionId },
                (key, bag) =>
                {
                    bag.Add(connectionId);
                    return bag;
                });

            // Notify the user that they've connected (only their connections)
            await SendToUserConnections(userId, "UserConnected", new { username, connectionId });

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst("userId")?.Value ?? "unknown";
            var username = Context.User?.FindFirst("username")?.Value ?? "Unknown";
            var connectionId = Context.ConnectionId;

            // Remove this connection from the user's connections
            if (UserConnections.TryGetValue(userId, out var bag))
            {
                var updatedBag = new ConcurrentBag<string>(bag.Where(c => c != connectionId));
                if (updatedBag.Count == 0)
                {
                    UserConnections.TryRemove(userId, out _);
                }
                else
                {
                    UserConnections[userId] = updatedBag;
                }
            }

            // Notify the user that they've disconnected (only their remaining connections)
            await SendToUserConnections(userId, "UserDisconnected", new { username, connectionId });

            await base.OnDisconnectedAsync(exception);
        }

        public async Task BroadcastActivityToCurrentUser(string action, string itemName)
        {
            var userId = Context.User?.FindFirst("userId")?.Value ?? "unknown";
            var username = Context.User?.FindFirst("username")?.Value ?? "Unknown";

            await SendToUserConnections(userId, "ReceiveActivity", new { username, action, itemName });
        }

        /// <summary>
        /// Sends a message only to the specified user's active connections
        /// </summary>
        private async Task SendToUserConnections(string userId, string methodName, object data)
        {
            if (UserConnections.TryGetValue(userId, out var connectionIds))
            {
                var validConnectionIds = connectionIds.Where(id => !string.IsNullOrEmpty(id)).ToList();
                if (validConnectionIds.Count > 0)
                {
                    await Clients.Clients(validConnectionIds).SendAsync(methodName, data);
                }
            }
        }

        /// <summary>
        /// Get all active users and ction counts (for debugging/monitoring)
        /// </summary>
        public Task<Dictionary<string, int>> GetActiveUsers()
        {
            var activeUsers = UserConnections
                .Where(kvp => kvp.Value.Count > 0)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count);
            
            return Task.FromResult(activeUsers);
        }
    }
}
