using System;
using Cysharp.Threading.Tasks;
using Gameplay.Core;
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
        private float traversalSpeed;
        
        [SerializeField] 
        private float gradientSpeedMultiplier;
        
        [SerializeField] 
        private float activationDistanceThreshold;

        [SerializeField] 
        private float endThreshold;

        [SerializeField] 
        private float swingSensitivity;
        
        [SerializeField] 
        private float swingStrength;
        
        [SerializeField]
        private float accelSensitivity;
        
        [SerializeField]
        private float accelWeight;

        [SerializeField] 
        private int stabilisationFrames;

        private PlayerMovementBehaviour playerMovementBehaviour;
        private Transform playerTransform;
        
        private float gradientSpeed;
        private float curveProgress;
        private float previousHorizontalVelocity;
        
        private int stabilisationFramesRemaining;
        
        private bool isMovingForwards;

        private async void Awake()
        {
            await UniTask.WaitUntil(PlayerAccessService.IsReady);

            playerMovementBehaviour = PlayerAccessService.Instance.PlayerMovementBehaviour;
            playerTransform = PlayerAccessService.Instance.PlayerTransform;

            gradientSpeed = Mathf.Sqrt(traversalSpeed) * gradientSpeedMultiplier;
        }

        private void Update()
        {
            if (PauseManager.Instance.IsPaused) return;
            
            if (hook.connectedBody == null)
            {
                TryHookPlayer();
            }
            else
            {
                MovePlayerAlongZipline();
            }
            
            UpdateGradient();
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
                isMovingForwards = tangent.x * preHookVelocity.x >= 0f;
            }
            
            stabilisationFramesRemaining = stabilisationFrames;
            previousHorizontalVelocity = playerMovementBehaviour.Velocity.x;
        }

        private void MovePlayerAlongZipline()
        {
            var oldPosition = hook.transform.position;
            var oldProgress = curveProgress;
            var desiredProgress = Time.deltaTime * traversalSpeed * (isMovingForwards ? 1f : -1f);

            curveProgress += desiredProgress;
            hook.transform.position = bezierCurve.GetPoint(curveProgress);

            if (curveProgress is > 0f and < 1f)
            {
                RotateHook();
            }
            else
            {
                var actualProgress = Mathf.Clamp(curveProgress, 0f, 1f) - oldProgress;
                var inverseProgressProportion = desiredProgress / actualProgress;
                var unhookVelocity = (hook.transform.position - oldPosition).xy() * inverseProgressProportion / Time.deltaTime;
                
                playerMovementBehaviour.UnhookPlayer(unhookVelocity);
                previousHorizontalVelocity = 0f;
            }
        }
        
        private void RotateHook()
        {
            // seems like dividing acceleration by time leads to a jitter for some reason
            var horizontalVelocity = playerMovementBehaviour.Velocity.x;
            var horizontalAccel = horizontalVelocity - previousHorizontalVelocity; 

            switch (stabilisationFramesRemaining)
            {
                case > 0:
                {
                    var predictedFrameProgressGain = Time.deltaTime * traversalSpeed * (isMovingForwards ? 1f : -1f);

                    var predictedCurveProgress = curveProgress + predictedFrameProgressGain;
                    var predictedPosition = bezierCurve.GetPoint(predictedCurveProgress);
                    var predictedVelocity = (predictedPosition - hook.transform.position).x / Time.deltaTime;
                
                    var nextPredictedCurveProgress = predictedCurveProgress + predictedFrameProgressGain;
                    var nextPredictedPosition = bezierCurve.GetPoint(nextPredictedCurveProgress);
                    var nextPredictedVelocity = (nextPredictedPosition - predictedPosition).x / Time.deltaTime;

                    horizontalVelocity = predictedVelocity;
                    horizontalAccel = nextPredictedVelocity - predictedVelocity;

                    stabilisationFramesRemaining--;
                    break;
                }
                case 0:
                {
                    // nobody will ever know why this works
                    horizontalAccel = -horizontalAccel;
                
                    stabilisationFramesRemaining--;
                    break;
                }
            }
            
            var currentAngle = hook.transform.eulerAngles.z;
            var accelContribution = accelWeight * Mathf.Atan(accelSensitivity * horizontalAccel);
            var targetAngle = -Mathf.Rad2Deg * Mathf.Atan((horizontalVelocity + accelContribution) * swingSensitivity) * swingStrength;
            
            hook.transform.Rotate(Vector3.forward, targetAngle - currentAngle);
            
            previousHorizontalVelocity = horizontalVelocity;
        }
        
        private void UpdateGradient()
        {
            var colourKeys = lineRenderer.colorGradient.colorKeys;
            var newColourKeys = new GradientColorKey[colourKeys.Length];
            
            for (var i = 0; i < colourKeys.Length; i++)
            {
                newColourKeys[i] = colourKeys[i];
            }

            for (var i = 1; i < newColourKeys.Length - 1; i++)
            {
                var colourKey = newColourKeys[i];
                
                colourKey.time += gradientSpeed * Time.deltaTime;
                colourKey.time %= 1f;
                    
                newColourKeys[i] = colourKey;
            }
            
            Array.Sort(newColourKeys, (a, b) => a.time.CompareTo(b.time));

            var secondKey = newColourKeys[1];
            var penultimateKey = newColourKeys[^2];
            var secondKeyContribution = 1f - penultimateKey.time;
            var penultimateKeyContribution = secondKey.time;

            if (secondKeyContribution + penultimateKeyContribution > 0f)
            {
                var unnormalisedColour = secondKey.color * secondKeyContribution + penultimateKey.color * penultimateKeyContribution;
                var endColour = unnormalisedColour / (secondKeyContribution + penultimateKeyContribution);

                newColourKeys[0].color = endColour;
                newColourKeys[^1].color = endColour;
            }

            var newGradient = lineRenderer.colorGradient;
            newGradient.colorKeys = newColourKeys;
            lineRenderer.colorGradient = newGradient;
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