using System.Collections.Concurrent;

namespace CampusConnect.Hubs
{
    public static class UserTracker
    {
        private static ConcurrentDictionary<string, bool> _onlineUsers
            = new ConcurrentDictionary<string, bool>();

        public static void UserConnected(string userId)
        {
            _onlineUsers[userId] = true;
        }

        public static void UserDisconnected(string userId)
        {
            _onlineUsers.TryRemove(userId, out _);
        }

        public static bool IsOnline(string userId)
        {
            return _onlineUsers.ContainsKey(userId);
        }
    }
}