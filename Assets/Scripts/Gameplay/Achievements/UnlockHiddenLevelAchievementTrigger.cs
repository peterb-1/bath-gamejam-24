using UI;

namespace Gameplay.Achievements
{
    public class UnlockHiddenLevelAchievementTrigger : AbstractAchievementTrigger
    {
        private void Awake()
        {
            SatelliteBehaviour.OnSatelliteComplete += HandleSatelliteComplete;
        }

        private void HandleSatelliteComplete()
        {
            TriggerAchievement();
        }
        
        private void OnDestroy()
        {
            SatelliteBehaviour.OnSatelliteComplete -= HandleSatelliteComplete;
        }
    }
}