using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace Gameplay.Boss
{
    public class BossMovementBehaviour : MonoBehaviour
    {
        [SerializeField] 
        private List<BossWaitPoint> waitPoints;
        
        [SerializeField]
        private float damageProgressThreshold;

        [SerializeField]
        private float cycleDuration;
        
        [SerializeField]
        private float cycleAmplitude;

        [SerializeField]
        private float progressTimeBuffer;

        [SerializeField]
        private GameObject movementCurves;
        
        private float progress;
        private float progressTimer;
        private bool progressTimerActive;
        private int nextPointIndex;
        private int numPoints;
        private int eventsQueued;
        private bool isAlive = true;

        private float currentCycleTime;

        public event Action<BossMovementBehaviour> OnBossProgress;

        private void Awake()
        {
            progressTimer = -progressTimeBuffer;
            numPoints = waitPoints.Count;
        }

        public void IncrementProgress()
        {
            progress = 0f;
            progressTimer = -progressTimeBuffer;
            progressTimerActive = false;
            
            OnBossProgress?.Invoke(this);
            
            nextPointIndex++;    
        }

        // Queue the event instead of immediately triggering
        // as a failsafe for if the player activates events out of order
        public void QueueIncrementProgress()
        {
            eventsQueued++;
        }
        
        private void Update()
        {
            if (!isAlive) return;

            if (nextPointIndex < numPoints)
            {
                // Increment on timed point
                if (waitPoints[nextPointIndex].ProgressType == BossWaitPoint.BossProgressType.Timed)
                {
                    if (progressTimer >= waitPoints[nextPointIndex].TimeToProgress)
                    {
                        IncrementProgress();
                    }
                }
            
                // Increment if event is queued
                if (waitPoints[nextPointIndex].ProgressType == BossWaitPoint.BossProgressType.Event
                    && (progress >= 1f || nextPointIndex == 0))
                {
                    if (eventsQueued > 0)
                    {
                        IncrementProgress();
                        eventsQueued--;
                    }
                }
            }
            
            if (progressTimerActive)
            {
                progressTimer += Time.deltaTime;
            }
            
            currentCycleTime += Time.deltaTime;

            var cycleOffset = cycleAmplitude * Mathf.Sin(currentCycleTime * Mathf.PI * 2 / cycleDuration) * Vector3.up;
            
            if (nextPointIndex == 0)
            {
                transform.position = waitPoints[0].Curve.GetPoint(0f) + cycleOffset;
                return;
            }
            
            if (progress < 1f)
            {
                progress += Time.deltaTime / waitPoints[nextPointIndex - 1].Time;
            }
            else
            {
                progressTimerActive = true;
            }
            
            progress = Mathf.Min(progress, 1f);

            var position = waitPoints[nextPointIndex - 1].Curve.GetPoint(progress);
            
            transform.position = position + cycleOffset;
        }

        public int GetMaxHealth()
        {
            // start counting from 1 since the final hit doesn't have a movement curve
            var health = 1;
            
            foreach (var waitPoint in waitPoints)
            {
                if (waitPoint.ProgressType == BossWaitPoint.BossProgressType.Damage)
                {
                    health++;
                }
            }

            return health;
        }

        public int GetNextPointIndex()
        {
            return nextPointIndex;
        }

        public bool IsDamageable()
        {
            return nextPointIndex == numPoints ||
                   (waitPoints[nextPointIndex].ProgressType is BossWaitPoint.BossProgressType.Damage && 
                    progress >= damageProgressThreshold);
        }
        

#if UNITY_EDITOR
        
        [Button("Add Curve")]
        private void AddCurve()
        {
            var newCurve = new GameObject("MovementCurve");
            newCurve.AddComponent<BossMovementCurve>();
            newCurve.transform.parent = movementCurves.transform;

            var waitPoint = new BossWaitPoint(newCurve.GetComponent<BossMovementCurve>());
            waitPoints.Add(waitPoint);
        }

        [Button("Clear Curves")]
        private void ClearCurves()
        {
            for (var i = movementCurves.transform.childCount - 1; i >= 0; i--)
            {
                var child = movementCurves.transform.GetChild(i);

                if (child.name == "MovementCurve")
                {
                    DestroyImmediate(movementCurves.transform.GetChild(i).gameObject);
                }
            }
            
            waitPoints.Clear();
        }
#endif
    }
}
