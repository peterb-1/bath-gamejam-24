using System;
using Audio;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Gameplay.Events
{
    public class AudioTriggerAction : AbstractEventAction
    {
        [SerializeField] 
        private AudioClipIdentifier audioClip;

        public override async UniTask Execute()
        {
            AudioManager.Instance.Play(audioClip);
        }
    }
}