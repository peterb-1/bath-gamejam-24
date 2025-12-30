using UnityEngine;

namespace Gameplay.Drone
{
    public interface IDroneMovementStrategy
    {
        public Vector3 GetUpdatedPosition();

#if UNITY_EDITOR
        public void DrawGizmos();
#endif
    }
}