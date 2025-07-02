using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Gameplay.Events
{
    public abstract class AbstractEventAction : MonoBehaviour
    {
        [field: SerializeField]
        public bool ShouldAwaitExecution { get; private set; }

        public abstract UniTask Execute();
    }
}