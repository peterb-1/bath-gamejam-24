using UnityEngine;

namespace Gameplay.Victory
{
    public class VictoryZone : MonoBehaviour
    {
        [SerializeField] 
        private Transform blackHoleTransform;

        [SerializeField] 
        private float rotationPerSecond;

        private void Update()
        {
            blackHoleTransform.Rotate(blackHoleTransform.forward, Time.deltaTime * rotationPerSecond);
        }
    }
}
