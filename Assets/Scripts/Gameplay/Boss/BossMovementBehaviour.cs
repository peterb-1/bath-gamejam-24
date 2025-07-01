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
        [SerializeField] 
        private BezierCurve bezierCurve;
        
        [SerializeField] 
        private int curveSegmentCount;
        
        [SerializeField] 
        private List<BossWaitPoint> waitPoints;
        
        [SerializeField]
        private float damageProgressThreshold;
        
        [SerializeField]
        private AnimationCurve movementCurve;
        
        [SerializeField]
        private AnimationCurve recoilCurve;

        private PlayerMovementBehaviour playerMovementBehaviour;
        
        private float progress;
        private int nextPointIndex;
        private bool isAlive = true;

        private async void Awake()
        {
            await UniTask.WaitUntil(PlayerAccessService.IsReady);
            
            playerMovementBehaviour = PlayerAccessService.Instance.PlayerMovementBehaviour;
            
            progress = 0f;
            nextPointIndex = 0;
            transform.position = bezierCurve.GetPoint(0f);
        }

        public void IncrementProgress()
        {
            nextPointIndex++;
            progress = 0f;
        }
        
        private void Update()
        {
            if (!isAlive) return;

            if (!waitPoints[nextPointIndex].IsDamagePoint)
            {
                if (playerMovementBehaviour.transform.position.x >
                    waitPoints[nextPointIndex].PlayerThreshold.position.x)
                {
                    IncrementProgress();
                }
            }

            if (nextPointIndex == 0) {return;}
            
            if (progress < 1f)
            {
                progress += Time.deltaTime / waitPoints[nextPointIndex].Time;
            }
            
            progress = Mathf.Min(progress, 1);
            
            var smoothedProgress = (waitPoints[nextPointIndex - 1].IsDamagePoint ?
                recoilCurve : movementCurve).Evaluate(progress);
            
            // scale progress to between the 2 current endpoints
            var t = waitPoints[nextPointIndex - 1].CurveProgress + 
                    (smoothedProgress * (waitPoints[nextPointIndex].CurveProgress - waitPoints[nextPointIndex - 1].CurveProgress));
            transform.position = bezierCurve.GetPoint(t);
        }

        public int GetMaxHealth()
        {
            var health = 0;
            foreach (var waitPoint in waitPoints)
            {
                if (waitPoint.IsDamagePoint)
                {
                    health++;
                }
            }

            return health;
        }

        public bool IsDamageable()
        {
            return waitPoints[nextPointIndex].IsDamagePoint && progress >= damageProgressThreshold;
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
                Gizmos.DrawSphere(bezierCurve.GetPoint(waitPoint.CurveProgress), 0.15f);
            }
            
            Gizmos.color = Color.yellow;

            foreach (var waitPoint in waitPoints)
            {
                if (!waitPoint.IsDamagePoint)
                {
                    Vector2 bottom = new Vector2(waitPoint.PlayerThreshold.position.x, -50f);
                    Vector2 top = new Vector2(waitPoint.PlayerThreshold.position.x, 50f);
                    Gizmos.DrawLine(bottom, top);
                }
            }
        }
#endif
    }
}
