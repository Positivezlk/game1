using System;
using UnityEngine;
using IdleStarforge.Data;
using IdleStarforge.Managers;

namespace IdleStarforge.Systems
{
    public class OfflineIncome : MonoBehaviour
    {
        [SerializeField] private int maxOfflineSeconds = 8 * 60 * 60;
        [SerializeField] private double offlineEfficiency = 0.65d;

        public double LastOfflineCredits { get; private set; }
        public double LastOfflineOre { get; private set; }
        public double LastOfflineScience { get; private set; }

        public void ApplyOfflineIncome(GameData data)
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            int deltaSeconds = (int)Mathf.Clamp(now - data.LastUnixTime, 0, maxOfflineSeconds);
            if (deltaSeconds <= 5) return;

            double creditsBonus = UpgradeSystem.Instance.GetTotalBonus(ResourceType.Credits);
            double oreBonus = UpgradeSystem.Instance.GetTotalBonus(ResourceType.Ore);
            double scienceBonus = UpgradeSystem.Instance.GetTotalBonus(ResourceType.Science);
            double global = UpgradeSystem.Instance.GetGlobalUpgradeMultiplier();

            EconomyManager.Instance.SetMultipliers(1d, 1d, global);

            LastOfflineCredits = EconomyManager.Instance.GetPassiveCreditsPerSecond(creditsBonus) * deltaSeconds * offlineEfficiency;
            LastOfflineOre = EconomyManager.Instance.GetOrePerSecond(oreBonus) * deltaSeconds * offlineEfficiency;
            LastOfflineScience = EconomyManager.Instance.GetSciencePerSecond(scienceBonus) * deltaSeconds * offlineEfficiency;

            EconomyManager.Instance.AddResource(ResourceType.Credits, LastOfflineCredits);
            EconomyManager.Instance.AddResource(ResourceType.Ore, LastOfflineOre);
            EconomyManager.Instance.AddResource(ResourceType.Science, LastOfflineScience);
        }
    }
}
