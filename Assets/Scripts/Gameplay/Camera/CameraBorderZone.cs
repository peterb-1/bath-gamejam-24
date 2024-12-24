using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Utils;

namespace Gameplay.Camera
{
    public class CameraBorderZone : MonoBehaviour
    {
        [SerializeField] 
        private PolygonCollider2D polygonCollider;

        [SerializeField] 
        private float dampingPower;

        private async void Awake()
        {
            await UniTask.WaitUntil(CameraAccessService.IsReady);

            CameraAccessService.Instance.CameraFollow.RegisterBorderZone(this);
        }

        public Vector2 SoftClampPosition(UnityEngine.Camera cam, Vector2 targetPosition)
        {
            var corners = GetCameraCorners(cam);

            foreach (var corner in corners)
            {
                if (polygonCollider.OverlapPoint(corner)) continue;
                
                var closestPoint = polygonCollider.ClosestPoint(corner);
                var distance = Vector2.Distance(corner, closestPoint);

                if (distance > 0f)
                {
                    var dampedDistance = Mathf.Pow(distance, dampingPower);
                    var adjustedCorner = Vector2.Lerp(closestPoint, corner, dampedDistance / distance);

                    targetPosition += adjustedCorner - corner;
                }
            }

            return targetPosition;
        }
        
        private static IEnumerable<Vector2> GetCameraCorners(UnityEngine.Camera cam)
        {
            var corners = new Vector2[4];
            
            corners[0] = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane)).xy();
            corners[1] = cam.ViewportToWorldPoint(new Vector3(1, 0, cam.nearClipPlane)).xy();
            corners[2] = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane)).xy();
            corners[3] = cam.ViewportToWorldPoint(new Vector3(0, 1, cam.nearClipPlane)).xy();

            return corners;
        }
    }
}