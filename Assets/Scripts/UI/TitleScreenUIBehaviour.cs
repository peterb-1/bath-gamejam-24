using System;
using Audio;
using Core;
using Cysharp.Threading.Tasks;
using Gameplay.Input;
using UnityEngine;

namespace UI
{
    public class TitleScreenUIBehaviour : MonoBehaviour
    {
        [SerializeField] 
        private float logoDuration;
        
        private async void Awake()
        {
            await UniTask.WaitUntil(AudioManager.IsReady);

            RunLogoSequenceAsync().Forget();
        }

        private async UniTask RunLogoSequenceAsync()
        {
            // make sure music doesn't play before the first frame, otherwise the delay will take longer than expected
            await UniTask.Yield();
            
            AudioManager.Instance.Play(AudioClipIdentifier.SplashScreen);

            await UniTask.Delay(TimeSpan.FromSeconds(logoDuration));
            
            AudioManager.Instance.PlayMusic(MusicIdentifier.MainMenu);

            InputManager.OnStartPerformed += HandleStartPerformed;
        }

        private void HandleStartPerformed()
        {
            InputManager.OnStartPerformed -= HandleStartPerformed;
            
            MoveToLevelSelectAsync().Forget();
        }

        private async UniTask MoveToLevelSelectAsync()
        {
            SceneLoader.Instance.LoadLevelSelect();
        }

        private void OnDestroy()
        {
            InputManager.OnStartPerformed -= HandleStartPerformed;
        }
    }
}