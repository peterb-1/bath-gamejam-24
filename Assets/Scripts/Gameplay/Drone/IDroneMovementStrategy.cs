using UnityEngine;

namespace Gameplay.Drone
{
    public interface IDroneMovementStrategy
    {
        public void Update();
        public Vector3 GetPosition();
        public Vector3 GetVelocity();

#if UNITY_EDITOR
        public void DrawGizmos();
#endif
    }
}