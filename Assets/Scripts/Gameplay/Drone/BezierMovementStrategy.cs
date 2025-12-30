using NaughtyAttributes;
using UnityEngine;
using Utils;

namespace Gameplay.Drone
{
    public class BezierMovementStrategy : IDroneMovementStrategy
    {
        [SerializeField] 
        private BezierCurve bezierCurve;

        [SerializeField]
        private int segmentCount;

        [SerializeField] 
        private float duration;

        [SerializeReference] 
        private IDroneMovementStrategy strategyOnFinish;

        private float curveProgress;

        public Vector3 GetUpdatedPosition()
        {
            if (curveProgress < 1f)
            {
                curveProgress += Time.deltaTime / duration;
                var smoothedProgress = Mathf.SmoothStep(0f, 1f, curveProgress);
                return bezierCurve.GetPoint(smoothedProgress);
            }
            
            return strategyOnFinish.GetUpdatedPosition();
        }
        
#if UNITY_EDITOR
        [Button("Add Control Point")]
        public void AddControlPoint(Transform droneTransform)
        {
            var newPoint = new GameObject("ControlPoint")
            {
                transform =
                {
                    parent = droneTransform.parent
                }
            };

            bezierCurve.AddPoint(newPoint.transform);
        }

        [Button("Clear Points")]
        public void ClearPoints(Transform droneTransform)
        {
            for (var i = droneTransform.childCount - 1; i >= 0; i--)
            {
                var child = droneTransform.GetChild(i);

                if (child.name == "ControlPoint")
                {
                    Object.DestroyImmediate(droneTransform.GetChild(i).gameObject);
                }
            }

            bezierCurve.Clear();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;

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

            Gizmos.color = Color.red;

            Vector3 previousPosition = Vector3.negativeInfinity;

            foreach (var position in bezierCurve.GenerateCurvePoints(segmentCount).ToArray())
            {
                if (previousPosition != Vector3.negativeInfinity)
                {
                    Gizmos.DrawLine(previousPosition, position);
                }

                previousPosition = position;
            }
        }
#endif
    }
}