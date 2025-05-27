using System;
using Audio;
using Core;
using Cysharp.Threading.Tasks;
using Gameplay.Input;
using Hardware;
using TMPro;
using UnityEngine;
using Utils;

namespace UI
{
    public class TitleScreenUIBehaviour : MonoBehaviour
    {
        [SerializeField] 
        private Animator titleScreenAnimator;

        [SerializeField] 
        private SpriteRenderer fogSpriteRenderer;
        
        [SerializeField] 
        private SpriteRenderer logoSpriteRenderer;
        
        [SerializeField] 
        private TMP_Text versionText;

        [SerializeField] 
        private PageGroup titlePageGroup;
        
        [SerializeField]
        private GameObjectStateSetter controllerStateSetter;

        [SerializeField] 
        private RumbleConfig titleDisplayRumbleConfig;
        
        [SerializeField] 
        private RumbleConfig sceneSwitchRumbleConfig;

        [SerializeField] 
        private AnimationCurve logoDisplayCurve;
    
        [SerializeField] 
        private float preLogoDuration;
        
        [SerializeField] 
        private float logoDuration;
        
        [SerializeField] 
        private float postLogoDuration;

        [SerializeField] 
        private float sceneSwitchWaitDuration;
        
        [SerializeField] 
        private float activeStarIntensity;
        
        private static readonly int ShowTitle = Animator.StringToHash("ShowTitle");
        private static readonly int SwitchScene = Animator.StringToHash("SwitchScene");
        private static readonly int StarIntensity = Shader.PropertyToID("_StarIntensity");
        private static readonly int Threshold = Shader.PropertyToID("_Threshold");

        private async void Awake()
        {
            logoSpriteRenderer.material.SetFloat(Threshold, 1f);
            fogSpriteRenderer.material.SetFloat(StarIntensity, 0f);

            versionText.text = $"v{Application.version}";
            
            InputManager.OnControlSchemeChanged += HandleControlSchemeChanged;
            
            HandleControlSchemeChanged(InputManager.CurrentNonMouseControlScheme);
            
            await UniTask.WaitUntil(AudioManager.IsReady);

            RunLogoSequenceAsync().Forget();
        }
        
        private void HandleControlSchemeChanged(ControlScheme controlScheme)
        {
            switch (controlScheme)
            {
                case ControlScheme.Gamepad:
                    controllerStateSetter.SetState();
                    break;
                case ControlScheme.Keyboard:
                    controllerStateSetter.SetInverseState();
                    break;
            }
        }

        private async UniTask RunLogoSequenceAsync()
        {
            // make sure music doesn't play before the first frame, otherwise the delay will take longer than expected
            await UniTask.Yield();
            
            AudioManager.Instance.Play(AudioClipIdentifier.SplashScreen);

            await UniTask.Delay(TimeSpan.FromSeconds(preLogoDuration));

            await ShowLogoAsync();
            
            await UniTask.Delay(TimeSpan.FromSeconds(postLogoDuration));
            
            titlePageGroup.ShowGroupImmediate();
            titleScreenAnimator.SetTrigger(ShowTitle);
            fogSpriteRenderer.material.SetFloat(StarIntensity, activeStarIntensity);
            
            RumbleManager.Instance.Rumble(titleDisplayRumbleConfig);
            AudioManager.Instance.PlayMusic(MusicIdentifier.MainMenu);

            InputManager.OnStartPerformed += HandleStartPerformed;
        }

        private async UniTask ShowLogoAsync()
        {
            var timeElapsed = 0f;

            while (timeElapsed < logoDuration)
            {
                var lerp = logoDisplayCurve.Evaluate(timeElapsed / logoDuration);

                logoSpriteRenderer.material.SetFloat(Threshold, lerp);
                
                await UniTask.Yield();

                timeElapsed += Time.unscaledDeltaTime;
            }
        }

        private void HandleStartPerformed()
        {
            InputManager.OnStartPerformed -= HandleStartPerformed;
            InputManager.OnControlSchemeChanged -= HandleControlSchemeChanged;
            
            MoveToLevelSelectAsync().Forget();
        }

        private async UniTask MoveToLevelSelectAsync()
        {
            AudioManager.Instance.Play(AudioClipIdentifier.ButtonHover);
            AudioManager.Instance.Play(AudioClipIdentifier.RainbowResult);
            RumbleManager.Instance.Rumble(sceneSwitchRumbleConfig);
            
            titleScreenAnimator.SetTrigger(SwitchScene);
            
            await UniTask.Delay(TimeSpan.FromSeconds(sceneSwitchWaitDuration));
            
            SceneLoader.Instance.LoadLevelSelect();
        }

        private void OnDestroy()
        {
            InputManager.OnStartPerformed -= HandleStartPerformed;
            InputManager.OnControlSchemeChanged -= HandleControlSchemeChanged;
        }
    }
}