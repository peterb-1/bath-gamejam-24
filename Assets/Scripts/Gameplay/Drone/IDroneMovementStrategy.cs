using UnityEngine;

namespace Gameplay.Drone
{
    public interface IDroneMovementStrategy
    {
        public Vector3 GetUpdatedPosition();
    }
}