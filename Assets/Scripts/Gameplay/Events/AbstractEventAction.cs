using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Gameplay.Events
{
    public abstract class AbstractEventAction : MonoBehaviour
    {
        [field: SerializeField]
        public bool ShouldAwaitExecution { get; private set; }

        [SerializeField] 
        private bool isActiveOnAwake = true;
        
        public bool IsActive { get; private set; }

        public abstract UniTask Execute();

        private void Awake()
        {
            IsActive = isActiveOnAwake;
        }

        public void SetActive(bool isActive)
        {
            IsActive = isActive;
        }
    }
}