using Steamworks;

namespace Utils
{
    public static class SteamworksExtensions
    {
        public static string GetUsername(this CSteamID id)
        {
            return SteamFriends.GetFriendPersonaName(id);
        }
    }
}