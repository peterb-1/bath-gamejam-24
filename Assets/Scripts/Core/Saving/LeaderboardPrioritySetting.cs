using UnityEngine;

namespace Core.Saving
{
    [CreateAssetMenu(menuName = "Settings/LeaderboardPrioritySetting")]
    public class LeaderboardPrioritySetting : AbstractSetting<LeaderboardPriority> {}
    
    public enum LeaderboardPriority
    {
        Newest,
        Oldest
    }
}