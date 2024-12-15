using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace Utils
{
    [Serializable]
    public class BezierCurve
    {
        [field: SerializeField] 
        private List<Transform> controlPoints;

        [SerializeField, ReadOnly, AllowNesting] 
        private List<Vector3> segmentPoints;

        public void AddPoint(Transform transform)
        {
            controlPoints.Add(transform);
        }

        public void Clear()
        {
            controlPoints = new List<Transform>();
            segmentPoints = new List<Vector3>();
        }
        
        public Vector3 GetPoint(float t)
        {
            if (controlPoints.Count < 2)
            {
                GameLogger.LogError("Bezier curve must have at least two control points!");
                return default;
            }
            
            var points = new List<Vector3>();
            foreach (var point in controlPoints)
            {
                points.Add(point.position);
            }

            while (points.Count > 1)
            {
                var nextLevel = new List<Vector3>();
                for (var i = 0; i < points.Count - 1; i++)
                {
                    nextLevel.Add(Vector3.Lerp(points[i], points[i + 1], t));
                }
                points = nextLevel;
            }
            
            return points[0];
        }

        public List<Transform> GetPoints()
        {
            return controlPoints;
        }
        
        public List<Vector3> GenerateCurvePoints(int segmentCount)
        {
            if (segmentCount < 1)
            {
                GameLogger.LogError($"Cannot generate curve points with segment count {segmentCount}!");
            }
            
            segmentPoints = new List<Vector3>(segmentCount + 1);
            
            for (var i = 0; i <= segmentCount; i++)
            {
                var t = i / (float)segmentCount;
                segmentPoints.Add(GetPoint(t));
            }
            
            return segmentPoints;
        }

        public float GetDistanceToCurve(Vector3 position, out Vector3 closestPoint, out float t)
        {
            var minSqrDistance = float.MaxValue;
            
            t = default;
            closestPoint = default;

            for (var i = 0; i < segmentPoints.Count; i++)
            {
                var point = segmentPoints[i];
                var sqrDistance = (point - position).sqrMagnitude;

                if (sqrDistance < minSqrDistance)
                {
                    t = i / ((float) segmentPoints.Count - 1);
                    minSqrDistance = sqrDistance;
                    closestPoint = point;
                }
            }

            return Mathf.Sqrt(minSqrDistance);
        }
        
        public Vector3 GetTangent(float t)
        {
            if (controlPoints.Count < 2)
            {
                GameLogger.LogError("Bezier curve must have at least two control points!");
                return default;
            }

            var points = new List<Vector3>();
            foreach (var point in controlPoints)
            {
                points.Add(point.position);
            }

            // generate first derivative points
            var derivativePoints = new List<Vector3>();
            for (var i = 0; i < points.Count - 1; i++)
            {
                derivativePoints.Add(points[i + 1] - points[i]);
            }

            // compute the tangent at t
            while (derivativePoints.Count > 1)
            {
                var nextLevel = new List<Vector3>();
                for (var i = 0; i < derivativePoints.Count - 1; i++)
                {
                    nextLevel.Add(Vector3.Lerp(derivativePoints[i], derivativePoints[i + 1], t));
                }
                derivativePoints = nextLevel;
            }

            return derivativePoints[0].normalized;
        }
    }
}