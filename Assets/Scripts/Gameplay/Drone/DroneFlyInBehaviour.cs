using NaughtyAttributes;
using UnityEngine;
using Utils;

namespace Gameplay.Drone
{
    public class DroneFlyInBehaviour : MonoBehaviour
    {
        [Header("Settings")] [SerializeField] private BezierCurve bezierCurve;

        [SerializeField] private int curveSegmentCount;

        [SerializeField] private float flyInTime;

        [SerializeField] private Transform patrolPoint1;

        [SerializeField] private float cycleTime;

        [SerializeField] private bool smoothEnds;

        [Header("References")] [SerializeField]
        private DroneHitboxBehaviour droneHitboxBehaviour;

        private Vector3 patrolPoint2;

        private float currentCycleTime;
        private float curveProgress;
        private bool isActive;
        private bool isAlive = true;

        private void Awake()
        {
            droneHitboxBehaviour.OnDroneKilled += HandleDroneKilled;

            isActive = droneHitboxBehaviour.GetStartState();

            patrolPoint2 = bezierCurve.GetPoint(1f);
        }

        private void HandleDroneKilled(DroneHitboxBehaviour _)
        {
            isAlive = false;
        }

        public void Activate()
        {
            isActive = true;
            droneHitboxBehaviour.ActivateHitbox();
        }

        private void Update()
        {
            if (!isAlive || !isActive) return;

            if (curveProgress < 1f)
            {
                curveProgress += Time.deltaTime / flyInTime;
                var smoothedProgress = Mathf.SmoothStep(0f, 1f, curveProgress);
                transform.position = bezierCurve.GetPoint(smoothedProgress);
                return;
            }

            currentCycleTime += Time.deltaTime;
            currentCycleTime %= cycleTime;

            var cycleProgress = currentCycleTime / cycleTime;
            var lerp = 1f - 2f * Mathf.Abs(cycleProgress - 0.5f);

            if (smoothEnds)
            {
                lerp = Mathf.SmoothStep(0f, 1f, lerp);
            }

            transform.position = lerp * patrolPoint1.position + (1f - lerp) * patrolPoint2;
        }

        private void OnDestroy()
        {
            droneHitboxBehaviour.OnDroneKilled -= HandleDroneKilled;
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

            foreach (var position in bezierCurve.GenerateCurvePoints(curveSegmentCount).ToArray())
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
