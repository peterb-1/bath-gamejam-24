using System;
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

        private PlayerMovementBehaviour playerMovementBehaviour;
        
        private float progress;
        private float progressTimer;
        private bool progressTimerActive;
        private int nextPointIndex;
        private bool isAlive = true;

        private float currentCycleTime;

        public event Action<BossMovementBehaviour> OnBossProgress;

        private async void Awake()
        {
            await UniTask.WaitUntil(PlayerAccessService.IsReady);
            
            playerMovementBehaviour = PlayerAccessService.Instance.PlayerMovementBehaviour;
            
            progress = 0f;
            progressTimer = -progressTimeBuffer;
            nextPointIndex = 0;
        }

        public void IncrementProgress()
        {
            progress = 0f;
            progressTimer = -progressTimeBuffer;
            progressTimerActive = false;
            OnBossProgress?.Invoke(this);
            nextPointIndex++;    
        }
        
        private void Update()
        {
            if (!isAlive) return;

            if (waitPoints[nextPointIndex].ProgressType == BossWaitPoint.BossProgressType.Timed)
            {
                if (progressTimer >= waitPoints[nextPointIndex].TimeToProgress)
                {
                    IncrementProgress();
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
 
            transform.position = waitPoints[nextPointIndex - 1].Curve.GetPoint(progress);
            transform.position += cycleOffset;
        }

        public int GetMaxHealth()
        {
            var health = 0;
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
            return waitPoints[nextPointIndex].ProgressType == BossWaitPoint.BossProgressType.Damage
                   && progress >= damageProgressThreshold;
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
