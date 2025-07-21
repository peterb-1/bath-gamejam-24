using System;
using NaughtyAttributes;
using UnityEngine;
using Utils;

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
        public LevelType LevelType { get; private set; }

        [SerializeField, HideIf(nameof(ShouldHideFields)), AllowNesting] 
        private int missionNumber;

        [field: SerializeField, HideIf(nameof(ShouldHideFields)), AllowNesting]
        public bool IsUnlockedByDefault { get; private set; }
        
        [field: SerializeField, HideIf(nameof(ShouldHideFields)), AllowNesting]
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

        public int OneStarMilliseconds => oneStarTime.ToMilliseconds();
        public int TwoStarMilliseconds => twoStarTime.ToMilliseconds();
        public int ThreeStarMilliseconds => threeStarTime.ToMilliseconds();
        public int RainbowMilliseconds => rainbowTime.ToMilliseconds();

        private bool ShouldHideFields => LevelType is LevelType.Hidden or LevelType.Boss;

        public TimeRanking GetTimeRanking(int milliseconds)
        {
            if (milliseconds <= RainbowMilliseconds) return TimeRanking.Rainbow;
            if (milliseconds <= ThreeStarMilliseconds) return TimeRanking.ThreeStar;
            if (milliseconds <= TwoStarMilliseconds) return TimeRanking.TwoStar;
            if (milliseconds <= OneStarMilliseconds) return TimeRanking.OneStar;

            return TimeRanking.Unranked;
        }

        public int GetStars(int milliseconds)
        {
            return Mathf.Min((int) GetTimeRanking(milliseconds), 3);
        }
        
        public string GetLevelCode()
        {
            return $"{GetRomanNumeral(districtNumber)}-{GetMissionCode()}";
        }
        
        public string GetLevelText()
        {
            return $"{GetDistrictName(districtNumber)}  —  Mission {GetMissionCode()}";
        }

        public string GetSteamName()
        {
            return $"{districtNumber}-{GetMissionCode()}";
        }
        
        public string GetSteamGhostFileName()
        {
            return $"{districtNumber}_{GetMissionCode()}_ghost";
        }
        
        private string GetMissionCode()
        {
            return LevelType switch
            {
                LevelType.Standard => missionNumber.ToString(),
                LevelType.Hidden => "X",
                LevelType.Boss => "B",
                _ => throw new ArgumentOutOfRangeException()
            };
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