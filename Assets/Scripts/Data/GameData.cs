using System;
using System.Collections.Generic;

namespace IdleStarforge.Data
{
    [Serializable]
    public class UpgradeSaveData
    {
        public string Id;
        public int Level;
    }

    [Serializable]
    public class GameData
    {
        public double Credits;
        public double Ore;
        public double Science;
        public int ProgressTier;
        public int PrestigeLevel;
        public double PrestigePoints;
        public long LastUnixTime;
        public List<UpgradeSaveData> Upgrades = new List<UpgradeSaveData>();
        public List<string> Achievements = new List<string>();
    }

    public enum ResourceType
    {
        Credits,
        Ore,
        Science
    }
}
