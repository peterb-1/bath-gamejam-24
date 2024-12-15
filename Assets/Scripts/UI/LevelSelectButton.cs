using Core;
using Core.Saving;
using Cysharp.Threading.Tasks;
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

        public SceneConfig SceneConfig => sceneConfig;

        private static readonly int Selected = Animator.StringToHash("Selected");

        protected override void Awake()
        {
            base.Awake();
            
            if (!Application.isPlaying) return;
            
            onClick.AddListener(HandleButtonClicked);

            if (sceneConfig.IsLevelScene)
            {
                levelNumberText.text = sceneConfig.LevelConfig.GetLevelNumber();
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

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);

            if (interactable)
            {
                borderAnimator.SetBool(Selected, true);
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
            
            if (interactable)
            {
                borderAnimator.SetBool(Selected, true);
            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            borderAnimator.SetBool(Selected, false);
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