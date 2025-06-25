using Core;
using Cysharp.Threading.Tasks;
using Gameplay.Player;
using UnityEngine;

namespace Gameplay.Collectibles
{
    public class Collectible : MonoBehaviour
    {
        [SerializeField] 
        private SpriteRenderer spriteRenderer;
        
        [SerializeField] 
        private SpriteRenderer backgroundRenderer;

        [SerializeField] 
        private ParticleSystem collectionParticles;
        
        [SerializeField]
        private Collider2D collectionCollider;

        [SerializeField] 
        private LayerMask playerLayers;
        
        [SerializeField] 
        private AnimationCurve dissolveCurve;

        [SerializeField] 
        private float dissolveDuration;
        
        [SerializeField] 
        private float preParticleDelay;
        
        [SerializeField] 
        private float alreadyFoundAlpha;
        
        private static readonly int Threshold = Shader.PropertyToID("_Threshold");
        private static readonly int AlphaMult = Shader.PropertyToID("_AlphaMult");

        private async void Awake()
        {
            await UniTask.WaitUntil(SceneLoader.IsReady);
            
            if (SceneLoader.Instance.CurrentLevelData.HasFoundCollectible)
            {
                spriteRenderer.material.SetFloat(AlphaMult, alreadyFoundAlpha);
                backgroundRenderer.enabled = false;

                var main = collectionParticles.main;
                var colour = main.startColor.color;

                colour.a = alreadyFoundAlpha;
                main.startColor = colour;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((playerLayers.value & (1 << other.gameObject.layer)) != 0)
            {
                collectionCollider.enabled = false;
                
                PlayerAccessService.Instance.PlayerVictoryBehaviour.NotifyFoundCollectible();
                
                RunDissolveAsync().Forget();
            }
        }
        
        private async UniTask RunDissolveAsync()
        {
            var timeElapsed = 0f;
            var backgroundColour = backgroundRenderer.color;
            var startAlpha = backgroundColour.a;
            var hasPlayedParticles = false;

            while (timeElapsed < dissolveDuration)
            {
                if (timeElapsed > preParticleDelay && !hasPlayedParticles)
                {
                    collectionParticles.Play();
                    hasPlayedParticles = true;
                }
                
                var lerp = dissolveCurve.Evaluate(timeElapsed / dissolveDuration);

                backgroundColour.a = Mathf.Lerp(startAlpha, 0f, lerp);

                backgroundRenderer.color = backgroundColour;
                spriteRenderer.material.SetFloat(Threshold, lerp);
                
                await UniTask.Yield();

                timeElapsed += Time.deltaTime;
            }

            backgroundColour.a = 0f;
            
            backgroundRenderer.color = backgroundColour;
            spriteRenderer.material.SetFloat(Threshold, dissolveCurve.Evaluate(1f));
        }
    }
}