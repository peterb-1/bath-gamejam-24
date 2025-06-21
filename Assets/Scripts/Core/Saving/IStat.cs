using System;

namespace Core.Saving
{
    public interface IStat
    {
        void Add(object value);
        object GetValue();
        void SetThreshold(object threshold, Action thresholdAction);
    }
}