using System;
using UnityEngine;
using IdleStarforge.Data;

namespace IdleStarforge.Managers
{
    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance { get; private set; }

        [Header("Базовые доходы")]
        [SerializeField] private double baseClickCredits = 1d;
        [SerializeField] private double basePassiveCreditsPerSecond = 0.3d;
        [SerializeField] private double baseOrePerSecond = 0.1d;
        [SerializeField] private double baseSciencePerSecond = 0.02d;

        public double Credits { get; private set; }
        public double Ore { get; private set; }
        public double Science { get; private set; }

        public int ProgressTier { get; private set; } = 1;
        public int PrestigeLevel { get; private set; }
        public double PrestigePoints { get; private set; }

        public double ClickMultiplier { get; private set; } = 1d;
        public double PassiveMultiplier { get; private set; } = 1d;
        public double GlobalMultiplier { get; private set; } = 1d;

        public event Action OnResourcesChanged;
        public event Action OnProgressChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void AddResource(ResourceType type, double amount)
        {
            if (amount <= 0) return;

            switch (type)
            {
                case ResourceType.Credits: Credits += amount; break;
                case ResourceType.Ore: Ore += amount; break;
                case ResourceType.Science: Science += amount; break;
            }

            RecalculateProgressTier();
            OnResourcesChanged?.Invoke();
        }

        public bool SpendResource(ResourceType type, double amount)
        {
            if (amount <= 0) return true;
            double current = GetResource(type);
            if (current < amount) return false;

            switch (type)
            {
                case ResourceType.Credits: Credits -= amount; break;
                case ResourceType.Ore: Ore -= amount; break;
                case ResourceType.Science: Science -= amount; break;
            }

            OnResourcesChanged?.Invoke();
            return true;
        }

        public double GetResource(ResourceType type)
        {
            switch (type)
            {
                case ResourceType.Credits: return Credits;
                case ResourceType.Ore: return Ore;
                case ResourceType.Science: return Science;
                default: return 0;
            }
        }

        public double GetClickIncome()
        {
            return baseClickCredits * ClickMultiplier * GlobalMultiplier * GetPrestigeMultiplier();
        }

        public double GetPassiveCreditsPerSecond(double upgradeBonus)
        {
            return (basePassiveCreditsPerSecond + upgradeBonus) * PassiveMultiplier * GlobalMultiplier * GetPrestigeMultiplier();
        }

        public double GetOrePerSecond(double oreBonus)
        {
            return (baseOrePerSecond + oreBonus) * PassiveMultiplier * GlobalMultiplier * GetPrestigeMultiplier();
        }

        public double GetSciencePerSecond(double scienceBonus)
        {
            return (baseSciencePerSecond + scienceBonus) * PassiveMultiplier * GlobalMultiplier * GetPrestigeMultiplier();
        }

        public void SetMultipliers(double click, double passive, double global)
        {
            ClickMultiplier = Math.Max(1d, click);
            PassiveMultiplier = Math.Max(1d, passive);
            GlobalMultiplier = Math.Max(1d, global);
        }

        public double GetPrestigeMultiplier()
        {
            return 1d + (PrestigePoints * 0.08d);
        }

        public double CalculatePrestigeGain()
        {
            double baseValue = Math.Sqrt((Credits + (Ore * 20d) + (Science * 200d)) / 10000d);
            return Math.Floor(baseValue);
        }

        public void ApplyPrestige()
        {
            double gain = CalculatePrestigeGain();
            if (gain < 1d) return;

            PrestigePoints += gain;
            PrestigeLevel += 1;

            Credits = 0;
            Ore = 0;
            Science = 0;
            ProgressTier = 1;

            UpgradeSystem.Instance.ResetUpgradesForPrestige();
            OnResourcesChanged?.Invoke();
            OnProgressChanged?.Invoke();
        }

        public void LoadFromData(GameData data)
        {
            Credits = data.Credits;
            Ore = data.Ore;
            Science = data.Science;
            ProgressTier = Math.Max(1, data.ProgressTier);
            PrestigeLevel = data.PrestigeLevel;
            PrestigePoints = data.PrestigePoints;

            OnResourcesChanged?.Invoke();
            OnProgressChanged?.Invoke();
        }

        public void FillData(GameData data)
        {
            data.Credits = Credits;
            data.Ore = Ore;
            data.Science = Science;
            data.ProgressTier = ProgressTier;
            data.PrestigeLevel = PrestigeLevel;
            data.PrestigePoints = PrestigePoints;
        }

        private void RecalculateProgressTier()
        {
            int previous = ProgressTier;
            double score = Credits + (Ore * 25d) + (Science * 300d);

            if (score >= 5_000_000d) ProgressTier = 5;
            else if (score >= 800_000d) ProgressTier = 4;
            else if (score >= 120_000d) ProgressTier = 3;
            else if (score >= 15_000d) ProgressTier = 2;
            else ProgressTier = 1;

            if (previous != ProgressTier)
            {
                OnProgressChanged?.Invoke();
            }
        }
    }
}
