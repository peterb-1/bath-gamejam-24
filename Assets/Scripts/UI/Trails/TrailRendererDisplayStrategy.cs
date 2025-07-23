using UnityEngine;
using Utils;

namespace UI.Trails
{
    public class TrailRendererDisplayStrategy : ITrailDisplayStrategy
    {
        private readonly TrailRenderer activePreviewTrail;
        private readonly float trailScrollSpeed;
        
        private Vector3[] trailPositionsBuffer = new Vector3[64];

        public TrailRendererDisplayStrategy(TrailRenderer trailRenderer, float scrollSpeed, float lifetime)
        {
            trailRenderer.Clear();
            trailRenderer.emitting = true;
            trailRenderer.sortingLayerName = "UI";
            trailRenderer.sortingOrder = 1;
            trailRenderer.colorGradient = trailRenderer.colorGradient.WithTint(new Color(0.8f, 0.8f, 0.8f));
            trailRenderer.time = lifetime;

            activePreviewTrail = trailRenderer;
            trailScrollSpeed = scrollSpeed;
        }

        public void Update()
        {
            if (activePreviewTrail == null) return;
            
            var positionsCount = activePreviewTrail.positionCount;
            if (positionsCount == 0) return;

            if (trailPositionsBuffer.Length < positionsCount)
            {
                trailPositionsBuffer = new Vector3[positionsCount];
            }

            activePreviewTrail.GetPositions(trailPositionsBuffer);

            var scrollOffset = trailScrollSpeed * Time.deltaTime;

            for (var i = 0; i < positionsCount; i++)
            {
                trailPositionsBuffer[i] += Vector3.left * scrollOffset;
            }

            activePreviewTrail.SetPositions(trailPositionsBuffer);
        }

        public void EmitTrail()
        {
            if (activePreviewTrail != null)
            {
                activePreviewTrail.Clear();
                activePreviewTrail.emitting = true;
            }
        }

        public void StopEmitting()
        {
            if (activePreviewTrail != null)
            {
                activePreviewTrail.emitting = false;
            }
        }
    }
}