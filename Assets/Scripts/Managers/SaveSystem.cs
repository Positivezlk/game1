using System;
using UnityEngine;
using IdleStarforge.Data;
using IdleStarforge.Systems;

namespace IdleStarforge.Managers
{
    public class SaveSystem : MonoBehaviour
    {
        public static SaveSystem Instance { get; private set; }

        private const string SaveKey = "idle_starforge_save";

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

        public void SaveGame()
        {
            var data = new GameData();
            EconomyManager.Instance.FillData(data);
            data.Upgrades = UpgradeSystem.Instance.ExportSaveData();
            data.LastUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (AchievementSystem.Instance != null)
            {
                data.Achievements = AchievementSystem.Instance.ExportAchievements();
            }

            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }

        public GameData LoadGame()
        {
            if (!PlayerPrefs.HasKey(SaveKey))
            {
                return CreateDefault();
            }

            string json = PlayerPrefs.GetString(SaveKey);
            if (string.IsNullOrWhiteSpace(json))
            {
                return CreateDefault();
            }

            var data = JsonUtility.FromJson<GameData>(json);
            if (data == null)
            {
                return CreateDefault();
            }

            return data;
        }

        private GameData CreateDefault()
        {
            return new GameData
            {
                Credits = 0,
                Ore = 0,
                Science = 0,
                ProgressTier = 1,
                PrestigeLevel = 0,
                PrestigePoints = 0,
                LastUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
        }
    }
}
