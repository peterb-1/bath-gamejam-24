using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Core.Saving
{
    [Serializable]
    public class StatsData
    {
        [SerializeField] 
        private float distanceCovered;

        [SerializeField] 
        private int jumpsMade;

        [SerializeField] 
        private int dashesMade;

        [SerializeField] 
        private int colourChanges;

        [SerializeField] 
        private int dronesKilled;

        [SerializeField] 
        private int cloudDeaths;

        [SerializeField] 
        private int droneDeaths;

        [SerializeField] 
        private int laserDeaths;

        [SerializeField] 
        private int buildingDeaths;

        [SerializeField]
        private float ziplineTime;

        private Dictionary<StatType, IStat> stats = new();
        
        public void Initialise()
        {
            stats[StatType.DistanceCovered] = new FloatStat(distanceCovered, v => distanceCovered = v);
            stats[StatType.JumpsMade] = new IntStat(jumpsMade, v => jumpsMade = v);
            stats[StatType.DashesMade] = new IntStat(dashesMade, v => dashesMade = v);
            stats[StatType.ColourChanges] = new IntStat(colourChanges, v => colourChanges = v);
            stats[StatType.DronesKilled] = new IntStat(dronesKilled, v => dronesKilled = v);
            stats[StatType.CloudDeaths] = new IntStat(cloudDeaths, v => cloudDeaths = v);
            stats[StatType.DroneDeaths] = new IntStat(droneDeaths, v => droneDeaths = v);
            stats[StatType.LaserDeaths] = new IntStat(laserDeaths, v => laserDeaths = v);
            stats[StatType.BuildingDeaths] = new IntStat(buildingDeaths, v => buildingDeaths = v);
            stats[StatType.ZiplineTime] = new FloatStat(ziplineTime, v => ziplineTime = v);
        }

        public void RegisterStatInterest(StatType statType, object value, Action onThresholdReached)
        {
            if (!stats.TryGetValue(statType, out var stat))
            {
                GameLogger.LogError($"StatType {statType} not registered.");
                return;
            }

            stat.SetThreshold(value, onThresholdReached);
        }
        
        public void AddToStat(StatType statType, object value)
        {
            if (stats.TryGetValue(statType, out var stat))
            {
                stat.Add(value);
            }
            else
            {
                GameLogger.LogError($"Stat {statType} not found.");
            }
        }
    }
}