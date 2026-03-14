using UnityEngine;
using IdleStarforge.Data;
using IdleStarforge.Systems;
using IdleStarforge.UI;

namespace IdleStarforge.Managers
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private SaveSystem saveSystem;
        [SerializeField] private OfflineIncome offlineIncome;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private RandomBonusSystem randomBonusSystem;
        [SerializeField] private AchievementSystem achievementSystem;

        private float autoSaveTimer;

        private void Start()
        {
            GameData data = saveSystem.LoadGame();
            EconomyManager.Instance.LoadFromData(data);
            UpgradeSystem.Instance.ImportSaveData(data.Upgrades);

            offlineIncome.ApplyOfflineIncome(data);
            uiManager.ShowOfflineIncome();

            achievementSystem.LoadAchievements(data.Achievements);
            randomBonusSystem.StartBonuses();
        }

        private void Update()
        {
            autoSaveTimer += Time.unscaledDeltaTime;
            if (autoSaveTimer >= 15f)
            {
                autoSaveTimer = 0f;
                SaveNow();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveNow();
            }
        }

        private void OnApplicationQuit()
        {
            SaveNow();
        }

        private void SaveNow()
        {
            saveSystem.SaveGame();
        }
    }
}
