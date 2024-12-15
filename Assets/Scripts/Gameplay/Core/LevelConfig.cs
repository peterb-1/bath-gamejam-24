using System;
using NaughtyAttributes;
using UnityEngine;

namespace Gameplay.Core
{
    [Serializable]
    public class LevelConfig
    {
        [field: SerializeField, ReadOnly, AllowNesting] 
        public string Guid { get; private set; }
        
        [SerializeField] 
        private int districtNumber;
        
        [SerializeField] 
        private int missionNumber;

        [field: SerializeField]
        public bool IsUnlockedByDefault { get; private set; }

        public string GetLevelNumber()
        {
            return $"{GetRomanNumeral(districtNumber)}-{missionNumber}";
        }
        
        public string GetLevelText()
        {
            return $"District {GetRomanNumeral(districtNumber)}  —  Mission {missionNumber}";
        }

        public static string GetRomanNumeral(int i)
        {
            return i switch
            {
                1 => "i",
                2 => "ii",
                3 => "iii",
                4 => "iv",
                5 => "v",
                6 => "vi",
                7 => "vii",
                8 => "viii",
                9 => "ix",
                10 => "x",
                _ => throw new ArgumentOutOfRangeException(nameof(i), i, null)
            };
        }
        
#if UNITY_EDITOR
        public void SetGuid(Guid guid)
        {
            Guid = guid.ToString();
        }
#endif
    }
}