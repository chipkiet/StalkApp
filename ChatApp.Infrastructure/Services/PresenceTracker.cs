using ChatApp.Application.Interfaces.Services;

namespace ChatApp.Infrastructure.Services;

/// <summary>
/// Singleton service theo dõi trạng thái online/offline của Users thông qua SignalR connections.
/// Dùng Dictionary in-memory. Safe với concurrent access nhờ SemaphoreSlim.
/// </summary>
public class PresenceTracker : IPresenceTracker
{
    // userId -> Set<connectionId>
    private readonly Dictionary<Guid, HashSet<string>> _connections = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<bool> UserConnectedAsync(Guid userId, string connectionId)
    {
        await _lock.WaitAsync();
        try
        {
            if (!_connections.TryGetValue(userId, out var connections))
            {
                connections = new HashSet<string>();
                _connections[userId] = connections;
            }

            connections.Add(connectionId);

            // Trả về true nếu đây là kết nối đầu tiên (user vừa chuyển sang Online)
            return connections.Count == 1;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> UserDisconnectedAsync(Guid userId, string connectionId)
    {
        await _lock.WaitAsync();
        try
        {
            if (!_connections.TryGetValue(userId, out var connections))
                return false;

            connections.Remove(connectionId);

            if (connections.Count == 0)
            {
                _connections.Remove(userId);
                // Trả về true nếu đây là kết nối cuối cùng (user vừa chuyển sang Offline)
                return true;
            }

            return false;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> IsOnlineAsync(Guid userId)
    {
        await _lock.WaitAsync();
        try
        {
            return _connections.TryGetValue(userId, out var connections) && connections.Count > 0;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IEnumerable<Guid>> GetOnlineUsersAsync()
    {
        await _lock.WaitAsync();
        try
        {
            return _connections
                .Where(kvp => kvp.Value.Count > 0)
                .Select(kvp => kvp.Key)
                .ToList();
        }
        finally
        {
            _lock.Release();
        }
    }
}
