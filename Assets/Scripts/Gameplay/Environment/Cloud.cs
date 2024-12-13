using NaughtyAttributes;
using UnityEngine;

namespace Gameplay.Environment
{
    public class Cloud : MonoBehaviour
    {
        [SerializeField, ReadOnly]
        private float horizontalSpeed;
        
        [SerializeField, ReadOnly]
        private float leftEnd;
        
        [SerializeField, ReadOnly]
        private float rightEnd;

        public void Configure(float speed, float left, float right)
        {
            horizontalSpeed = speed;
            leftEnd = left;
            rightEnd = right;
        }

        private void Update()
        {
            var position = transform.localPosition;
            
            position += Vector3.right * (horizontalSpeed * Time.deltaTime);

            if (horizontalSpeed > 0 && position.x > rightEnd)
            {
                position.x -= rightEnd - leftEnd;
            }
            else if (horizontalSpeed < 0 && position.x < leftEnd)
            {
                position.x += rightEnd - leftEnd;
            }

            transform.localPosition = position;
        }
    }
}
