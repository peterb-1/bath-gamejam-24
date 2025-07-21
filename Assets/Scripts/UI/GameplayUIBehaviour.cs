using System;
using Core;
using Cysharp.Threading.Tasks;
using Gameplay.Dash;
using Gameplay.Ghosts;
using Gameplay.Input;
using Gameplay.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class GameplayUIBehaviour : MonoBehaviour
    {
        [SerializeField] 
        private PageGroup gameplayPageGroup;

        [SerializeField] 
        private Image buttonPromptImage;
        
        [SerializeField] 
        private Image backgroundButtonPromptImage;
        
        [SerializeField] 
        private Sprite keyboardMouseButtonPrompts;
        
        [SerializeField] 
        private Sprite gamepadButtonPrompts;
        
        [SerializeField] 
        private Sprite mouseButtonPrompts;

        [SerializeField] 
        private TMP_Text timerText;

        [SerializeField] 
        private TMP_Text timerBonusText;

        [SerializeField] 
        private Animator timerTextAnimator;
        
        [SerializeField] 
        private Animator dashOrbAnimator;

        [SerializeField]
        private DashOrbUIBehaviour[] dashOrbs;

        private PlayerVictoryBehaviour playerVictoryBehaviour;
        private TimerBehaviour timerBehaviour;
        
        private static readonly int Pulse = Animator.StringToHash("Pulse");
        private static readonly int Shake = Animator.StringToHash("Shake");

        private async void Awake()
        {
            InputManager.OnControlSchemeChanged += HandleControlSchemeChanged;
            DashTrackerService.OnDashGained += HandleDashGained;
            DashTrackerService.OnDashUsed += HandleDashUsed;
            DashTrackerService.OnDashFailed += HandleDashFailed;
            GhostRunner.OnSpectateVictorySequenceStart += HandleSpectateVictorySequenceStart;
            
            await UniTask.WaitUntil(PlayerAccessService.IsReady);

            playerVictoryBehaviour = PlayerAccessService.Instance.PlayerVictoryBehaviour;
            playerVictoryBehaviour.OnVictorySequenceStart += HandleVictorySequenceStart;
            
            timerBehaviour = PlayerAccessService.Instance.TimerBehaviour;
            timerBehaviour.OnTimeBonusApplied += HandleTimeBonusApplied;

            HandleControlSchemeChanged(InputManager.CurrentControlScheme);
            
            if (SceneLoader.Instance.SceneLoadContext != null && 
                SceneLoader.Instance.SceneLoadContext.TryGetCustomData(GhostRunner.SPECTATE_KEY, out bool isSpectating) && 
                isSpectating)
            {
                buttonPromptImage.enabled = false;
                backgroundButtonPromptImage.enabled = false;
            }
        }

        private void HandleTimeBonusApplied(float timeBonus)
        {
            timerBonusText.SetText($"- {TimerBehaviour.GetFormattedTime(timeBonus.ToMilliseconds())}");
            
            timerTextAnimator.SetTrigger(Pulse);
        }

        private void HandleVictorySequenceStart(Vector2 _1, float _2)
        {
            gameplayPageGroup.HideGroup();
        }

        private void HandleSpectateVictorySequenceStart()
        {
            gameplayPageGroup.HideGroup();
        }

        private void HandleControlSchemeChanged(ControlScheme controlScheme)
        {
            buttonPromptImage.sprite = controlScheme switch
            {
                ControlScheme.Keyboard => keyboardMouseButtonPrompts,
                ControlScheme.Gamepad => gamepadButtonPrompts,
                ControlScheme.Mouse => mouseButtonPrompts,
                _ => throw new ArgumentOutOfRangeException(nameof(controlScheme), controlScheme, null)
            };
            
            backgroundButtonPromptImage.sprite = controlScheme switch
            {
                ControlScheme.Keyboard => keyboardMouseButtonPrompts,
                ControlScheme.Gamepad => gamepadButtonPrompts,
                ControlScheme.Mouse => mouseButtonPrompts,
                _ => throw new ArgumentOutOfRangeException(nameof(controlScheme), controlScheme, null)
            };
        }
        
        private void HandleDashGained(int orbs, DashOrb _)
        {
            dashOrbs[orbs - 1].Show();
        }
        
        private void HandleDashUsed(int orbs)
        {
            dashOrbs[orbs].Hide();
        }
        
        private void HandleDashFailed()
        {
            dashOrbAnimator.SetTrigger(Shake);
        }

        private void Update()
        {
            timerText.text = timerBehaviour.GetFormattedTimeElapsed();
        }
        
        private void OnDestroy()
        {
            InputManager.OnControlSchemeChanged -= HandleControlSchemeChanged;
            DashTrackerService.OnDashGained -= HandleDashGained;
            DashTrackerService.OnDashUsed -= HandleDashUsed;
            DashTrackerService.OnDashFailed -= HandleDashFailed;
            GhostRunner.OnSpectateVictorySequenceStart -= HandleSpectateVictorySequenceStart;
            
            playerVictoryBehaviour.OnVictorySequenceStart -= HandleVictorySequenceStart;
            timerBehaviour.OnTimeBonusApplied -= HandleTimeBonusApplied;
        }
    }
}
