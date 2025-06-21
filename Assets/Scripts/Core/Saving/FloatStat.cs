using System;
using Utils;

namespace Core.Saving
{
    public class FloatStat : IStat
    {
        private float value;
        private float threshold = float.MaxValue;

        private Action onThresholdReached;
        private readonly Action<float> valueSetAction;

        public FloatStat(float initialValue, Action<float> setter)
        {
            value = initialValue;
            valueSetAction = setter;
        }

        public void Add(object val)
        {
            if (val is float f)
            {
                value += f;
                valueSetAction?.Invoke(value);
            }
            else
            {
                GameLogger.LogError($"Trying to add non-float value {val} to float stat!");
                return;
            }

            if (value >= threshold)
            {
                onThresholdReached?.Invoke();
                threshold = float.MaxValue;
            }
        }

        public object GetValue() => value;

        public void SetThreshold(object thresh, Action thresholdAction)
        {
            if (thresh is float f)
            {
                threshold = f;
                onThresholdReached = thresholdAction;
            }
            else
            {
                GameLogger.LogError($"Trying to set non-float threshold {thresh} on float stat!");
            }
        }
    }
}