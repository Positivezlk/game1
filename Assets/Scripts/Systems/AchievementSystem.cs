using System;
using System.Collections.Generic;
using UnityEngine;
using IdleStarforge.Managers;

namespace IdleStarforge.Systems
{
    public class AchievementSystem : MonoBehaviour
    {
        public static AchievementSystem Instance { get; private set; }

        private readonly HashSet<string> unlocked = new HashSet<string>();

        public event Action<string> OnAchievementUnlocked;

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

        private void Update()
        {
            Check("first_click", EconomyManager.Instance.Credits >= 10, "Первые деньги");
            Check("ore_found", EconomyManager.Instance.Ore >= 100, "Рудокоп");
            Check("science_found", EconomyManager.Instance.Science >= 50, "Молодой ученый");
            Check("tier_3", EconomyManager.Instance.ProgressTier >= 3, "Индустриальный прорыв");
            Check("prestige_1", EconomyManager.Instance.PrestigeLevel >= 1, "Новая эпоха");
        }

        public void LoadAchievements(List<string> ids)
        {
            unlocked.Clear();
            if (ids == null) return;
            foreach (var id in ids)
            {
                unlocked.Add(id);
            }
        }

        public List<string> ExportAchievements()
        {
            return new List<string>(unlocked);
        }

        private void Check(string id, bool condition, string message)
        {
            if (!condition || unlocked.Contains(id)) return;
            unlocked.Add(id);
            Debug.Log($"Достижение: {message}");
            OnAchievementUnlocked?.Invoke(message);
        }
    }
}
