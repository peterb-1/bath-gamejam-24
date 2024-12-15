using Cysharp.Threading.Tasks;
using Gameplay.Player;
using NaughtyAttributes;
using UnityEngine;
using Utils;

namespace Gameplay.Zipline
{
    public class Zipline : MonoBehaviour
    {
        [SerializeField] 
        private LineRenderer lineRenderer;

        [SerializeField] 
        private HingeJoint2D hook;
        
        [SerializeField] 
        private BezierCurve bezierCurve;

        [SerializeField] 
        private int curveSegmentCount;

        [SerializeField] 
        private float activationDistanceThreshold;

        [SerializeField] 
        private float traversalSpeed;

        [SerializeField] 
        private float endThreshold;

        private PlayerMovementBehaviour playerMovementBehaviour;
        private Transform playerTransform;
        
        private float curveProgress;
        private bool isMovingForwards;

        private async void Awake()
        {
            await UniTask.WaitUntil(PlayerAccessService.IsReady);

            playerMovementBehaviour = PlayerAccessService.Instance.PlayerMovementBehaviour;
            playerTransform = PlayerAccessService.Instance.PlayerTransform;
        }

        private void Update()
        {
            if (hook.connectedBody == null)
            {
                TryHookPlayer();
            }
            else
            {
                MovePlayerAlongZipline();
            }
        }

        private void TryHookPlayer()
        {
            var playerDistance = bezierCurve.GetDistanceToCurve(playerTransform.position, out var closestPoint, out curveProgress);
            if (playerDistance > activationDistanceThreshold) return;
                
            hook.transform.position = closestPoint;
                
            var preHookVelocity = playerMovementBehaviour.Velocity;

            if (!playerMovementBehaviour.TryHookPlayer(hook)) return;
            
            if (curveProgress < endThreshold)
            {
                isMovingForwards = true;
            }
            else if (curveProgress > 1f - endThreshold)
            {
                isMovingForwards = false;
            }
            else
            {
                var tangent = bezierCurve.GetTangent(curveProgress).xy();
                isMovingForwards = Vector2.Dot(tangent, preHookVelocity) > 0f;
            }
        }

        private void MovePlayerAlongZipline()
        {
            curveProgress += Time.deltaTime * traversalSpeed * (isMovingForwards ? 1f : -1f);

            if (curveProgress is > 0f and < 1f)
            {
                hook.transform.position = bezierCurve.GetPoint(curveProgress);
            }
            else
            {
                playerMovementBehaviour.UnhookPlayer();
            }
        }

#if UNITY_EDITOR
        [Button("Create Curve")]
        private void PopulateCurve()
        {
            lineRenderer.positionCount = curveSegmentCount + 1;
            lineRenderer.SetPositions(bezierCurve.GenerateCurvePoints(curveSegmentCount).ToArray());
        }

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
        }
#endif
    }
}