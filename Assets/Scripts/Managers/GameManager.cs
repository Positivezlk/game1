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

        public void Configure(
            SaveSystem save,
            OfflineIncome offline,
            UIManager ui,
            RandomBonusSystem random,
            AchievementSystem achievements)
        {
            saveSystem = save;
            offlineIncome = offline;
            uiManager = ui;
            randomBonusSystem = random;
            achievementSystem = achievements;
        }

        private void Start()
        {
            if (saveSystem == null || offlineIncome == null || uiManager == null || randomBonusSystem == null || achievementSystem == null)
            {
                Debug.LogError("GameManager не сконфигурирован. Проверьте SceneBootstrapper или ссылки в инспекторе.");
                enabled = false;
                return;
            }

            GameData data = saveSystem.LoadGame();
            EconomyManager.Instance.LoadFromData(data);
            UpgradeSystem.Instance.ImportSaveData(data.Upgrades);

            achievementSystem.LoadAchievements(data.Achievements);
            offlineIncome.ApplyOfflineIncome(data);
            uiManager.ShowOfflineIncome();
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
