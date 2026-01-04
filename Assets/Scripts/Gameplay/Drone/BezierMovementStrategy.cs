using NaughtyAttributes;
using UnityEngine;
using Utils;

namespace Gameplay.Drone
{
    public class BezierMovementStrategy : IDroneMovementStrategy, IFixedPathStrategy
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

        public void Initialise(DroneMovementBehaviour drone)
        {
            strategyOnFinish.Initialise(drone);
        }

        public void Update()
        {
            if (curveProgress < 1f)
            {
                curveProgress += Time.deltaTime / duration;
            }
            else
            {
                strategyOnFinish.Update();
            }
        }

        public Vector3 GetPosition()
        {
            return curveProgress < 1f ? GetPositionAfterTime(0f) : strategyOnFinish.GetPosition();
        }
        
        public Vector3 GetVelocity()
        {
            return curveProgress < 1f ? GetVelocityAfterTime(0f) : strategyOnFinish.GetVelocity();
        }
        
        public Vector3 GetPositionAfterTime(float deltaTime)
        {
            var predictedProgress = curveProgress + deltaTime / duration;
            
            if (predictedProgress < 1f)
            {
                var smoothedProgress = Mathf.SmoothStep(0f, 1f, predictedProgress);
                return bezierCurve.GetPoint(smoothedProgress);
            }

            if (strategyOnFinish is IFixedPathStrategy fixedPathStrategy)
            {
                var excessTime = (predictedProgress - 1f) * duration;
                return fixedPathStrategy.GetPositionAfterTime(excessTime);
            }
            
            return bezierCurve.GetPoint(1f);
        }

        public Vector3 GetVelocityAfterTime(float deltaTime)
        {
            var predictedProgress = curveProgress + deltaTime / duration;
    
            if (predictedProgress < 1f)
            {
                var smoothedProgress = Mathf.SmoothStep(0f, 1f, predictedProgress);
                var tangent = bezierCurve.GetTangent(smoothedProgress, normalise: false);
                var progressDerivative = 6f * predictedProgress - 6f * predictedProgress * predictedProgress;
                var progressRate = progressDerivative / duration;
        
                return tangent * progressRate;
            }

            if (strategyOnFinish is IFixedPathStrategy fixedPathStrategy)
            {
                var excessTime = (predictedProgress - 1f) * duration;
                return fixedPathStrategy.GetVelocityAfterTime(excessTime);
            }
    
            return strategyOnFinish.GetVelocity();
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

        public void DrawGizmos()
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

            var previousPosition = Vector3.negativeInfinity;

            foreach (var position in bezierCurve.GenerateCurvePoints(segmentCount).ToArray())
            {
                if (previousPosition != Vector3.negativeInfinity)
                {
                    Gizmos.DrawLine(previousPosition, position);
                }

                previousPosition = position;
            }
            
            strategyOnFinish?.DrawGizmos();
        }
#endif
    }
}