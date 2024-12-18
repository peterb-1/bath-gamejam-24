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
        
        [SerializeField] 
        private float oneStarTime;
        
        [SerializeField] 
        private float twoStarTime;
        
        [SerializeField] 
        private float threeStarTime;
        
        [SerializeField] 
        private float rainbowTime;

        public int DistrictNumber => districtNumber;
        public int MissionNumber => missionNumber;

        public float OneStarTime => oneStarTime;
        public float TwoStarTime => twoStarTime;
        public float ThreeStarTime => threeStarTime;
        public float RainbowTime => rainbowTime;

        public TimeRanking GetTimeRanking(float time)
        {
            if (time <= rainbowTime) return TimeRanking.Rainbow;
            if (time <= threeStarTime) return TimeRanking.ThreeStar;
            if (time <= twoStarTime) return TimeRanking.TwoStar;
            if (time <= oneStarTime) return TimeRanking.OneStar;

            return TimeRanking.Unranked;
        }
        
        public string GetLevelCode()
        {
            return $"{GetRomanNumeral(districtNumber)}-{missionNumber}";
        }
        
        public string GetLevelText()
        {
            return $"{GetDistrictName(districtNumber)}  —  Mission {missionNumber}";
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
        
        public static string GetDistrictName(int i)
        {
            return i switch
            {
                1 => "The Outskirts",
                2 => "Scrapyard North",
                3 => "Transit Hub",
                4 => "Power Station",
                5 => "Chroma Springs",
                6 => "UNNAMED",
                7 => "Security Perimeter",
                8 => "UNNAMED",
                9 => "UNNAMED",
                10 => "Production Core",
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