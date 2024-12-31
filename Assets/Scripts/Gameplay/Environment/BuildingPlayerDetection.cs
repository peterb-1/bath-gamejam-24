using UnityEngine;

namespace Gameplay.Environment
{
    public class BuildingPlayerDetection : MonoBehaviour
    {
        [SerializeField] 
        private Building building;
        
        private void OnTriggerEnter2D(Collider2D _)
        {
            building.NotifyPlayerEntered();
        }
        
        private void OnTriggerExit2D(Collider2D _)
        {
            building.NotifyPlayerExited();
        }
    }
}