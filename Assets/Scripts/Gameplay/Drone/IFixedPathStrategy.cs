using UnityEngine;

namespace Gameplay.Drone
{
    public interface IFixedPathStrategy
    {
        Vector3 GetPositionAfterTime(float deltaTime);
        Vector3 GetVelocityAfterTime(float deltaTime);
    }
}
