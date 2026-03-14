using System;
using System.Collections.Generic;
using UnityEngine;
using IdleStarforge.Data;
using IdleStarforge.Economy;

namespace IdleStarforge.Managers
{
    public class UpgradeSystem : MonoBehaviour
    {
        public static UpgradeSystem Instance { get; private set; }

        private readonly List<UpgradeDefinition> definitions = new List<UpgradeDefinition>();
        private readonly Dictionary<string, int> levels = new Dictionary<string, int>();

        public event Action OnUpgradesChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            CreateDefaultUpgrades();
        }

        public IReadOnlyList<UpgradeDefinition> Definitions => definitions;

        public int GetLevel(string id)
        {
            return levels.TryGetValue(id, out int value) ? value : 0;
        }

        public double GetUpgradeCost(UpgradeDefinition definition)
        {
            int level = GetLevel(definition.Id);
            return definition.BaseCost * Math.Pow(definition.CostGrowth, level);
        }

        public bool CanBuy(UpgradeDefinition definition)
        {
            if (EconomyManager.Instance.ProgressTier < definition.TierRequired) return false;
            int level = GetLevel(definition.Id);
            if (level >= definition.MaxLevel) return false;

            return EconomyManager.Instance.GetResource(definition.CostResource) >= GetUpgradeCost(definition);
        }

        public bool BuyUpgrade(UpgradeDefinition definition)
        {
            if (!CanBuy(definition)) return false;

            double cost = GetUpgradeCost(definition);
            if (!EconomyManager.Instance.SpendResource(definition.CostResource, cost)) return false;

            levels[definition.Id] = GetLevel(definition.Id) + 1;
            OnUpgradesChanged?.Invoke();
            return true;
        }

        public double GetTotalBonus(ResourceType target)
        {
            double total = 0d;
            foreach (var definition in definitions)
            {
                if (definition.TargetResource != target) continue;
                int level = GetLevel(definition.Id);
                if (level == 0) continue;
                total += definition.BaseIncomeBonus * (Math.Pow(definition.IncomeGrowth, level) - 1d) / (definition.IncomeGrowth - 1d);
            }

            return total;
        }

        public double GetGlobalUpgradeMultiplier()
        {
            double multiplier = 1d;
            foreach (var definition in definitions)
            {
                if (definition.Id.StartsWith("global_"))
                {
                    multiplier *= 1d + (GetLevel(definition.Id) * 0.05d);
                }
            }

            return multiplier;
        }

        public List<UpgradeSaveData> ExportSaveData()
        {
            var list = new List<UpgradeSaveData>();
            foreach (var pair in levels)
            {
                list.Add(new UpgradeSaveData
                {
                    Id = pair.Key,
                    Level = pair.Value
                });
            }

            return list;
        }

        public void ImportSaveData(List<UpgradeSaveData> save)
        {
            levels.Clear();
            if (save == null)
            {
                OnUpgradesChanged?.Invoke();
                return;
            }

            foreach (var entry in save)
            {
                levels[entry.Id] = entry.Level;
            }

            OnUpgradesChanged?.Invoke();
        }

        public void ResetUpgradesForPrestige()
        {
            levels.Clear();
            OnUpgradesChanged?.Invoke();
        }

        private void CreateDefaultUpgrades()
        {
            definitions.Clear();

            AddPack("click", "Клик-доход", ResourceType.Credits, ResourceType.Credits, 10d, 1.18d, 0.3d, 1.06d, 8, 1);
            AddPack("ore", "Добыча руды", ResourceType.Credits, ResourceType.Ore, 35d, 1.2d, 0.08d, 1.08d, 8, 1);
            AddPack("science", "Лаборатория", ResourceType.Ore, ResourceType.Science, 18d, 1.22d, 0.015d, 1.09d, 8, 2);
            AddPack("credit", "Автоматизация кредита", ResourceType.Credits, ResourceType.Credits, 80d, 1.25d, 0.45d, 1.07d, 8, 2);
            AddPack("global", "Глобальный бустер", ResourceType.Science, ResourceType.Credits, 6d, 1.3d, 0.6d, 1.05d, 8, 3, true);
        }

        private void AddPack(
            string prefix,
            string title,
            ResourceType cost,
            ResourceType target,
            double baseCost,
            double growth,
            double bonus,
            double incomeGrowth,
            int count,
            int minTier,
            bool isGlobal = false)
        {
            for (int i = 1; i <= count; i++)
            {
                definitions.Add(new UpgradeDefinition
                {
                    Id = (isGlobal ? "global_" : string.Empty) + $"{prefix}_{i}",
                    NameRu = $"{title} {i}",
                    DescriptionRu = $"Увеличивает доход ({target})",
                    CostResource = cost,
                    TargetResource = target,
                    BaseCost = baseCost * Math.Pow(1.9d, i - 1),
                    CostGrowth = growth,
                    BaseIncomeBonus = bonus * Math.Pow(1.28d, i - 1),
                    IncomeGrowth = incomeGrowth,
                    MaxLevel = 10,
                    TierRequired = Math.Min(5, minTier + (i / 3))
                });
            }
        }
    }
}
