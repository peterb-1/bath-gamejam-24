using Audio;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Gameplay.Events
{
    public class PlayAudioAction : AbstractEventAction
    {
        [SerializeField] 
        private AudioClipIdentifier audioClip;

        public override UniTask Execute()
        {
            AudioManager.Instance.Play(audioClip);
            
            return UniTask.CompletedTask;
        }
    }
}