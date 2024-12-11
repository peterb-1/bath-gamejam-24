using System;
using UnityEngine;

namespace Gameplay.Core
{
    [Serializable]
    public class LevelConfig
    {
        [SerializeField] 
        private int districtNumber;
        
        [SerializeField] 
        private int missionNumber;

        public string GetLevelNumber()
        {
            return $"{GetRomanNumeral(districtNumber)}-{missionNumber}";
        }
        
        public string GetLevelText()
        {
            return $"District {GetRomanNumeral(districtNumber)}  —  Mission {missionNumber}";
        }

        private string GetRomanNumeral(int i)
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
    }
}