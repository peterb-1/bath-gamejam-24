using Core;
using Core.Saving;
using Cysharp.Threading.Tasks;
using Gameplay.Input;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils;

namespace UI
{
    public class LevelSelectButton : ExtendedButton
    {
        [SerializeField] 
        private TMP_Text levelNumberText;

        [SerializeField] 
        private SceneConfig sceneConfig;

        [SerializeField] 
        private Animator borderAnimator;

        [SerializeField] 
        private LineRenderer lineRenderer;

        [SerializeField] 
        private Transform leftConnectionAnchor;
        
        [SerializeField] 
        private Transform rightConnectionAnchor;

        public SceneConfig SceneConfig => sceneConfig;
        public Transform LeftConnectionAnchor => leftConnectionAnchor;
        public Transform RightConnectionAnchor => rightConnectionAnchor;

        private Transform previousConnectionAnchor;
        private bool wasSelectedThisFrame;
        private bool enableLinkUpdates;

        private static readonly int Selected = Animator.StringToHash("Selected");

        protected override void Awake()
        {
            base.Awake();
            
            if (!Application.isPlaying) return;
            
            onClick.AddListener(HandleButtonClicked);

            if (sceneConfig.IsLevelScene)
            {
                levelNumberText.text = sceneConfig.LevelConfig.GetLevelCode();
            }
            else
            {
                GameLogger.LogWarning($"Level select button {name} was assigned scene config with no level config!", this);
                levelNumberText.text = "N/A";
            }
            
            SetLockedStateAsync().Forget();
        }
        
        private async UniTask SetLockedStateAsync()
        {
            await UniTask.WaitUntil(() => SaveManager.IsReady);

            interactable = SaveManager.Instance.SaveData.CampaignData.TryGetLevelData(sceneConfig.LevelConfig, out var levelData) &&
                           levelData.IsUnlocked;

            if (!interactable)
            {
                gameObject.SetActive(false);
            }
        }
        
        public void EnableLink(Transform previousAnchor)
        {
            previousConnectionAnchor = previousAnchor;
            enableLinkUpdates = true;
        }

        private void Update()
        {
            if (!enableLinkUpdates) return;
            
            lineRenderer.SetPositions(new [] {previousConnectionAnchor.position, LeftConnectionAnchor.position});
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);

            if (interactable)
            {
                borderAnimator.SetBool(Selected, true);

                SetSelectedStateThisFrameAsync().Forget();
            }
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            borderAnimator.SetBool(Selected, false);
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            
            // if it's not mouse, ignore pointer events - we'll get selected/deselected instead
            if (InputManager.CurrentControlScheme is not ControlScheme.Mouse) return;

            if (interactable)
            {
                borderAnimator.SetBool(Selected, true);
            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            
            // if it's not mouse, ignore pointer events - we'll get selected/deselected instead
            if (InputManager.CurrentControlScheme is not ControlScheme.Mouse && wasSelectedThisFrame) return;
            
            borderAnimator.SetBool(Selected, false);
        }

        private async UniTask SetSelectedStateThisFrameAsync()
        {
            if (wasSelectedThisFrame) return;
            
            wasSelectedThisFrame = true;

            await UniTask.DelayFrame(1, PlayerLoopTiming.PostLateUpdate);
            
            wasSelectedThisFrame = false;
        }

        private void HandleButtonClicked()
        {
            SceneLoader.Instance.LoadScene(sceneConfig);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            onClick.RemoveListener(HandleButtonClicked);
        }
    }
}