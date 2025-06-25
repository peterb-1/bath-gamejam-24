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
        
        [field: SerializeField]
        public bool IsHidden { get; private set; }

        [SerializeField, HideIf(nameof(IsHidden)), AllowNesting] 
        private int missionNumber;

        [field: SerializeField, HideIf(nameof(IsHidden)), AllowNesting]
        public bool IsUnlockedByDefault { get; private set; }
        
        [field: SerializeField, HideIf(nameof(IsHidden)), AllowNesting]
        public bool HasCollectible { get; private set; }

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

        private string MissionCode => IsHidden ? "X" : missionNumber.ToString();

        public TimeRanking GetTimeRanking(float time)
        {
            if (time <= rainbowTime) return TimeRanking.Rainbow;
            if (time <= threeStarTime) return TimeRanking.ThreeStar;
            if (time <= twoStarTime) return TimeRanking.TwoStar;
            if (time <= oneStarTime) return TimeRanking.OneStar;

            return TimeRanking.Unranked;
        }

        public int GetStars(float time)
        {
            return Mathf.Min((int) GetTimeRanking(time), 3);
        }
        
        public string GetLevelCode()
        {
            return $"{GetRomanNumeral(districtNumber)}-{MissionCode}";
        }
        
        public string GetLevelText()
        {
            return $"{GetDistrictName(districtNumber)}  —  Mission {MissionCode}";
        }

        public string GetSteamName()
        {
            return $"{districtNumber}-{MissionCode}";
        }
        
        public string GetSteamGhostFileName()
        {
            return $"{districtNumber}_{MissionCode}_ghost";
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