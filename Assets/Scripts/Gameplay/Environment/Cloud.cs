using NaughtyAttributes;
using UnityEngine;

namespace Gameplay.Environment
{
    public class Cloud : MonoBehaviour
    {
        [SerializeField, ReadOnly]
        private float horizontalSpeed;
        
        [SerializeField, ReadOnly]
        private float despawnPosition;
        
        [SerializeField, ReadOnly]
        private float respawnPosition;

        public void Configure(float speed, float despawn, float respawn)
        {
            horizontalSpeed = speed;
            despawnPosition = despawn;
            respawnPosition = respawn;
        }

        private void Update()
        {
            var position = transform.localPosition;
            
            position += Vector3.right * (horizontalSpeed * Time.deltaTime);

            if ((horizontalSpeed > 0 && position.x > despawnPosition) ||
                (horizontalSpeed < 0 && position.x < despawnPosition))
            {
                position.x = respawnPosition;
            }

            transform.localPosition = position;
        }
    }
}
