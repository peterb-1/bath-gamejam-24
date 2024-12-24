using System;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Gameplay.Environment
{
    public class Tile : MonoBehaviour
    {
        [SerializeField] 
        private SpriteRenderer spriteRenderer;

        [SerializeField] 
        private AnimationCurve flashCurve;

        [SerializeField] 
        private float flashDuration;

        [SerializeField] 
        private DistrictTileSprites[] districtSprites;

        [SerializeField, ReadOnly]
        private Color baseColour;
        
        private bool isFlashing;

        public async UniTask FlashAsync(Color flashColour)
        {
            if (isFlashing) return;
            
            isFlashing = true;

            var timeElapsed = 0f;

            while (timeElapsed < flashDuration && isFlashing)
            {
                var lerp = flashCurve.Evaluate(timeElapsed / flashDuration);

                spriteRenderer.color = (1f - lerp) * baseColour + lerp * flashColour;

                await UniTask.DelayFrame(1, cancellationToken: destroyCancellationToken);
                
                timeElapsed += Time.deltaTime;
            }

            spriteRenderer.color = baseColour;
            isFlashing = false;
        }

        public void CancelFlash()
        {
            isFlashing = false;
        }

        public void Toggle(bool isActive)
        {
            baseColour = new Color(baseColour.r, baseColour.g, baseColour.b, isActive ? 1f : 0f);

            if (spriteRenderer != null)
            {
                spriteRenderer.color = baseColour;
            }
        }
        
        public void SetSize(Vector2 size, Vector2 random)
        {
            var variation = new Vector2(Random.Range(-random.x, random.x), Random.Range(-random.y, random.y)); 
            transform.localScale = size + variation;
        }
        
        public void SetPosition(Vector2 position)
        {
            transform.position = position;
        }

        public void SetColour(Color colour)
        {
            baseColour = colour;
            spriteRenderer.color = colour;
        }

        public void SetOrder(string layer, int order)
        {
            spriteRenderer.sortingLayerName = layer;
            spriteRenderer.sortingOrder = order;
        }

        public void SetSprite(int district)
        {
            spriteRenderer.sprite = districtSprites[district - 1].Sprites.RandomChoice();
        }
    }

    [Serializable]
    public class DistrictTileSprites
    {
        [field: SerializeField] 
        public Sprite[] Sprites { get; private set; }
    }
}
