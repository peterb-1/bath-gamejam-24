using System;
using Core;
using Core.Colour;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay.Buildings
{
    public class Tile : MonoBehaviour
    {
        [SerializeField] 
        private SpriteRenderer spriteRenderer;

        [SerializeField] 
        private AnimationCurve flashCurve;

        [SerializeField] 
        private float flashDuration;

        [SerializeField, ReadOnly]
        private Color baseColour;
        
        private bool isFlashing;

        public async UniTask FlashAsync()
        {
            if (isFlashing) return;
            
            isFlashing = true;

            var timeElapsed = 0f;

            while (timeElapsed < flashDuration)
            {
                var lerp = flashCurve.Evaluate(timeElapsed / flashDuration);

                spriteRenderer.color = (1f - lerp) * baseColour + lerp * Color.white;

                await UniTask.DelayFrame(1).AttachExternalCancellation(destroyCancellationToken);
                
                timeElapsed += Time.deltaTime;
            }

            spriteRenderer.color = baseColour;
            isFlashing = false;
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

        public void SetOrder(int order)
        {
            spriteRenderer.sortingOrder = order;
        }
    }
}
