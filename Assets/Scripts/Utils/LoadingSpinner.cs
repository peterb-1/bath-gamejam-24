using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Utils
{
    public class LoadingSpinner : MonoBehaviour
    {
        [SerializeField] 
        private Image spinnerImage;
    
        [SerializeField] 
        private float cycleTime;
    
        [SerializeField] 
        private AnimationCurve easingFunction;
    
        private bool isSpinning;
        private float currentCycleTime;

        private void Awake()
        {
            spinnerImage.fillClockwise = true;
            spinnerImage.fillMethod = Image.FillMethod.Radial360;
        }

        public void StartSpinner()
        {
            isSpinning = true;
            currentCycleTime = 0.25f;

            SpinAsync().Forget();
        }

        public void StopSpinner()
        {
            isSpinning = false;
        }

        public async UniTask StopSpinnerAfter(float seconds)
        {
            await UniTask.WaitForSeconds(seconds);
        
            isSpinning = false;
        }
    
        private async UniTask SpinAsync()
        {
            while (isSpinning)
            {
                currentCycleTime += Time.deltaTime;
                currentCycleTime %= cycleTime;

                var cycleProgress = currentCycleTime / cycleTime;

                if (cycleProgress < .5f)
                {
                    var forwardProgress = easingFunction.Evaluate(cycleProgress * 2);

                    spinnerImage.fillClockwise = true;
                    spinnerImage.fillAmount = forwardProgress;
                    spinnerImage.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, -360 * forwardProgress));
                }
                else
                {
                    var backwardProgress = easingFunction.Evaluate((1 - cycleProgress) * 2);

                    spinnerImage.fillClockwise = false;
                    spinnerImage.fillAmount = backwardProgress;
                    spinnerImage.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 360 * backwardProgress));
                }

                await UniTask.Yield();
            }
        }

        private void OnDestroy()
        {
            StopSpinner();
        }
    }
}
