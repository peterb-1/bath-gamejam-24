using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Gameplay.Dash
{
    public class DashOrb : MonoBehaviour
    {
        [SerializeField] 
        private SpriteRenderer spriteRenderer;

        [SerializeField] 
        private Collider2D collectionCollider;
        
        [SerializeField] 
        private LayerMask playerLayers;
        
        [SerializeField] 
        private AnimationCurve dissolveCurve;

        [SerializeField] 
        private float dissolveDuration;
        
        private static readonly int Threshold = Shader.PropertyToID("_Threshold");
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((playerLayers.value & (1 << other.gameObject.layer)) != 0)
            {
                if (DashTrackerService.Instance.TryCollect(this))
                {
                    collectionCollider.enabled = false;
                    
                    RunDissolveAsync().Forget();
                }
            }
        }
        
        private async UniTask RunDissolveAsync()
        {
            var timeElapsed = 0f;

            while (timeElapsed < dissolveDuration)
            {
                var lerp = dissolveCurve.Evaluate(timeElapsed / dissolveDuration);

                spriteRenderer.material.SetFloat(Threshold, lerp);
                
                await UniTask.Yield();

                timeElapsed += Time.deltaTime;
            }
            
            spriteRenderer.material.SetFloat(Threshold, dissolveCurve.Evaluate(1f));
        }
    }
}