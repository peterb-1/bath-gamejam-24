using System;
using Cysharp.Threading.Tasks;
using Gameplay.Dash;
using Gameplay.Input;
using Gameplay.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class GameplayUIBehaviour : MonoBehaviour
    {
        [SerializeField] 
        private PageGroup gameplayPageGroup;

        [SerializeField] 
        private Image buttonPromptImage;
        
        [SerializeField] 
        private Sprite keyboardMouseButtonPrompts;
        
        [SerializeField] 
        private Sprite gamepadButtonPrompts;
        
        [SerializeField] 
        private TimerBehaviour timerBehaviour;

        [SerializeField] 
        private TMP_Text timerText;

        [SerializeField] 
        private TMP_Text timerBonusText;

        [SerializeField] 
        private Animator timerTextAnimator;

        [SerializeField]
        private DashOrbUIBehaviour[] dashOrbs;

        private PlayerVictoryBehaviour playerVictoryBehaviour;
        
        private static readonly int Pulse = Animator.StringToHash("Pulse");

        private async void Awake()
        {
            InputManager.OnControlSchemeChanged += HandleControlSchemeChanged;
            DashTrackerService.OnDashGained += HandleDashGained;
            DashTrackerService.OnDashUsed += HandleDashUsed;
            timerBehaviour.OnTimeBonusApplied += HandleTimeBonusApplied;
            
            await UniTask.WaitUntil(PlayerAccessService.IsReady);

            playerVictoryBehaviour = PlayerAccessService.Instance.PlayerVictoryBehaviour;
            playerVictoryBehaviour.OnVictorySequenceStart += HandleVictorySequenceStart;
            
            HandleControlSchemeChanged(InputManager.CurrentNonMouseControlScheme);
        }

        private void HandleTimeBonusApplied(float timeBonus)
        {
            timerBonusText.SetText($"- {TimerBehaviour.GetFormattedTime(timeBonus)}");
            
            timerTextAnimator.SetTrigger(Pulse);
        }

        private void HandleVictorySequenceStart(Vector2 _1, float _2)
        {
            gameplayPageGroup.HideGroup();
        }

        private void HandleControlSchemeChanged(ControlScheme controlScheme)
        {
            buttonPromptImage.sprite = controlScheme switch
            {
                ControlScheme.Keyboard => keyboardMouseButtonPrompts,
                ControlScheme.Gamepad => gamepadButtonPrompts,
                ControlScheme.Mouse => buttonPromptImage.sprite,
                _ => throw new ArgumentOutOfRangeException(nameof(controlScheme), controlScheme, null)
            };
        }
        
        private void HandleDashGained(int orbs)
        {
            dashOrbs[orbs - 1].Show();
        }
        
        private void HandleDashUsed(int orbs)
        {
            dashOrbs[orbs].Hide();
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
            timerBehaviour.OnTimeBonusApplied -= HandleTimeBonusApplied;
        }
    }
}
