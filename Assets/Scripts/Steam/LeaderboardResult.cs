using Steamworks;

namespace Steam
{
    public class LeaderboardResult
    {
        public LeaderboardEntry_t Entry { get; private set; }
        public int[] Details { get; private set; }

        public LeaderboardResult(LeaderboardEntry_t entry, int[] details)
        {
            Entry = entry;
            Details = details;
        }
    }
}