using System;
using Utils;

namespace Core.Saving
{
    public class IntStat : IStat
    {
        private int value;
        private int threshold = int.MaxValue;
        
        private Action onThresholdReached;
        private readonly Action<int> valueSetAction;
        
        public IntStat(int initialValue, Action<int> setter)
        {
            value = initialValue;
            valueSetAction = setter;
        }

        public void Add(object val)
        {
            if (val is int i)
            {
                value += i;
                valueSetAction?.Invoke(value);
            }
            else
            {
                GameLogger.LogError($"Trying to add non-int value {val} to int stat!");
                return;
            }

            if (value >= threshold)
            {
                onThresholdReached?.Invoke();
                threshold = int.MaxValue;
            }
        }

        public object GetValue() => value;

        public void SetThreshold(object thresh, Action thresholdAction)
        {
            if (thresh is int i)
            {
                threshold = i;
                onThresholdReached = thresholdAction;
            }
            else
            {
                GameLogger.LogError($"Trying to set non-int threshold {thresh} on int stat!");
            }
        }
    }
}