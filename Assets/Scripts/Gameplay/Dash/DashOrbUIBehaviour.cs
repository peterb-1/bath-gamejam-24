using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Dash
{
    public class DashOrbUIBehaviour : MonoBehaviour
    {
        private const int DASH_INTRODUCTION_DISTRICT = 4;
        
        [SerializeField] 
        private Image image;

        [SerializeField]
        private AnimationCurve fadeCurve;

        [SerializeField] 
        private float fadeDuration;

        private void Awake()
        {
            SetColour(0f);

            if (SceneLoader.Instance.CurrentSceneConfig.LevelConfig.DistrictNumber < DASH_INTRODUCTION_DISTRICT)
            {
                image.enabled = false;
            }
        }

        public void Show()
        {
            RunFadeAsync(true).Forget();
        }
        
        public void Hide()
        {
            RunFadeAsync(false).Forget();
        }

        private async UniTask RunFadeAsync(bool isForwards)
        {
            var timeElapsed = 0f;

            while (timeElapsed < fadeDuration)
            {
                var lerp = fadeCurve.Evaluate(timeElapsed / fadeDuration);

                if (!isForwards)
                {
                    lerp = 1f - lerp;
                }

                SetColour(lerp);
            
                await UniTask.Yield();

                timeElapsed += Time.deltaTime;
            }

            SetColour(isForwards ? 1f : 0f);
        }

        private void SetColour(float lerp)
        {
            image.color = Color.Lerp(new Color(1f, 1f, 1f, 0f), Color.white, fadeCurve.Evaluate(lerp));
        }
    }
}