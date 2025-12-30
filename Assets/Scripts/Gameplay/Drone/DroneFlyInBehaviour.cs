using NaughtyAttributes;
using UnityEngine;
using Utils;

namespace Gameplay.Drone
{
    public class DroneFlyInBehaviour : MonoBehaviour
    {
        [Header("Curve settings")] 
        [SerializeField] 
        private BezierCurve bezierCurve;

        [SerializeField]
        private int curveSegmentCount;

        [SerializeField]
        private float flyInTime;

#if UNITY_EDITOR
        [Button("Add Control Point")]
        private void AddControlPoint()
        {
            var newPoint = new GameObject("ControlPoint");
            
            newPoint.transform.parent = transform.parent;

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
