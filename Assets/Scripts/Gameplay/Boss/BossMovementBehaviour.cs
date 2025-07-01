using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.Player;
using NaughtyAttributes;
using UnityEngine;
using Utils;

namespace Gameplay.Boss
{
    public class BossMovementBehaviour : MonoBehaviour
    {
        [System.Serializable]
        private struct WaitPoint
        {
            public float time;
            public float curveProgress;
            public Transform playerThreshold;
        }
        
        [SerializeField] 
        private BezierCurve bezierCurve;
        
        [SerializeField] 
        private int curveSegmentCount;
        
        [SerializeField] 
        private List<WaitPoint> waitPoints;
        
        [SerializeField] 
        private bool smoothEnds;
       
        [SerializeField]
        private float playerProgressOffset;

        [SerializeField]
        private float dampingFactor;
        
        [SerializeField]
        private float bossRecoil;
        
        [SerializeField]
        private Rigidbody2D rigidBody;
        
        private PlayerMovementBehaviour playerMovementBehaviour;
        
        private float progress;
        private float prevProgress;
        private float prevProgressDelta;
        private int nextPointIndex;
        private bool isAlive = true;

        private async void Awake()
        {
            await UniTask.WaitUntil(PlayerAccessService.IsReady);
            
            playerMovementBehaviour = PlayerAccessService.Instance.PlayerMovementBehaviour;
            
            progress = 0f;
            prevProgress = 0f;
            prevProgressDelta = 0f;
            nextPointIndex = 1;
        }

        public void IncrementProgress()
        {
            nextPointIndex++;
            progress = 0f;
            prevProgress = 0f;
            prevProgressDelta = bossRecoil;
        }
        
        private void Update()
        {
            if (!isAlive) return;
            
            progress += Time.deltaTime / waitPoints[nextPointIndex].time;
            // naturally move along path over time
            // proportion of distance between the 2 player thresholds covered so far
            var playerProgress = (playerMovementBehaviour.transform.position.x -
                                     waitPoints[nextPointIndex - 1].playerThreshold.position.x) /
                                    (waitPoints[nextPointIndex].playerThreshold.position.x -
                                     waitPoints[nextPointIndex - 1].playerThreshold.position.x);
            playerProgress += playerProgressOffset;

            if (progress < playerProgress)
            {
                progress = playerProgress;
            }
            if (progress < prevProgress)
            {
                progress = prevProgress;
            }

            progress = (1 - dampingFactor) * progress + dampingFactor * (prevProgress + prevProgressDelta);
            
            progress = Mathf.Min(progress, 1);
            
            prevProgressDelta = progress - prevProgress;
            prevProgress = progress;
            
            var smoothedProgress = smoothEnds ? Mathf.SmoothStep(0f, 1f, progress) : progress;
            
            // scale progress to between the 2 current endpoints
            var t = waitPoints[nextPointIndex - 1].curveProgress + 
                    (smoothedProgress * (waitPoints[nextPointIndex].curveProgress - waitPoints[nextPointIndex - 1].curveProgress));
            transform.position = bezierCurve.GetPoint(t);
        }

        public int GetMaxHealth()
        {
            return waitPoints.Count - 1;
        }

        public float GetProgress()
        {
            return progress;
        }
        

#if UNITY_EDITOR

        [Button("Add Control Point")]
        private void AddControlPoint()
        {
            var newPoint = new GameObject("ControlPoint");
            newPoint.transform.parent = transform;
            
            bezierCurve.AddPoint(newPoint.transform);
        }

        [Button("Clear Points")]
        private void ClearPoints()
        {
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);

                if (child.name == "ControlPoint")
                {
                    DestroyImmediate(transform.GetChild(i).gameObject);
                }
            }
            
            bezierCurve.Clear();
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;

            Transform previousPoint = null;

            foreach (var point in bezierCurve.GetPoints())
            {
                Gizmos.DrawSphere(point.position, 0.15f);

                if (previousPoint != null)
                {
                    Gizmos.DrawLine(previousPoint.position, point.position);
                }
                
                previousPoint = point;
            }
            
            Gizmos.color = Color.magenta;
            
            Vector3 previousPosition = Vector3.negativeInfinity;

            foreach (var position in bezierCurve.GenerateCurvePoints(curveSegmentCount).ToArray())
            {
                if (previousPosition != Vector3.negativeInfinity)
                {
                    Gizmos.DrawLine(previousPosition, position);
                }
                
                previousPosition = position;
            }

            foreach (var waitPoint in waitPoints)
            {
                Gizmos.DrawSphere(bezierCurve.GetPoint(waitPoint.curveProgress), 0.15f);
            }
            
            Gizmos.color = Color.yellow;

            foreach (var waitPoint in waitPoints)
            {
                Vector2 bottom = new Vector2(waitPoint.playerThreshold.position.x, -50f);
                Vector2 top = new Vector2(waitPoint.playerThreshold.position.x, 50f);
                Gizmos.DrawLine(bottom, top);
            }
        }
#endif
    }
}
