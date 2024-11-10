using System;
using Cysharp.Threading.Tasks;
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
        private TMP_Text timerText;

        private float time;

        private PlayerVictoryBehaviour playerVictoryBehaviour;

        private async void Awake()
        {
            InputManager.OnControlSchemeChanged += HandleControlSchemeChanged;
            
            await UniTask.WaitUntil(PlayerAccessService.IsReady);

            playerVictoryBehaviour = PlayerAccessService.Instance.PlayerVictoryBehaviour;
            playerVictoryBehaviour.OnVictorySequenceStart += HandleVictorySequenceStart;
            
            HandleControlSchemeChanged(InputManager.CurrentControlScheme);
        }

        private void Start()
        {
            time = 0;
        }

        private void HandleVictorySequenceStart(Vector2 _1, float _2)
        {
            gameplayPageGroup.HideGroup();
        }

        private void HandleControlSchemeChanged(ControlScheme controlScheme)
        {
            buttonPromptImage.sprite = controlScheme switch
            {
                ControlScheme.KeyboardMouse => keyboardMouseButtonPrompts,
                ControlScheme.Gamepad => gamepadButtonPrompts,
                _ => throw new ArgumentOutOfRangeException(nameof(controlScheme), controlScheme, null)
            };
        }

        private void OnDestroy()
        {
            InputManager.OnControlSchemeChanged -= HandleControlSchemeChanged;
        }

        private void FormatTime()
        {
            var seconds = (int)(time % 60);
            var centiSeconds = (int)((time - (int)time)*100);
            var minutes = (int)(time / 60);
            timerText.text = $"{minutes:00}:{seconds:00}:{centiSeconds:00}";
        }

        private void Update()
        {
            time += Time.deltaTime;
            FormatTime();
        }
    }
}
