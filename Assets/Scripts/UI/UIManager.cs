using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using IdleStarforge.Managers;
using IdleStarforge.Systems;

namespace IdleStarforge.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Основной UI")]
        [SerializeField] private TMP_Text creditsText;
        [SerializeField] private TMP_Text oreText;
        [SerializeField] private TMP_Text scienceText;
        [SerializeField] private TMP_Text tierText;
        [SerializeField] private TMP_Text prestigeText;
        [SerializeField] private TMP_Text offlineText;
        [SerializeField] private Slider progressSlider;

        [Header("Кнопки")]
        [SerializeField] private Button clickButton;
        [SerializeField] private Button adButton;
        [SerializeField] private Button prestigeButton;

        [Header("Системы")]
        [SerializeField] private IdleIncomeSystem idleIncomeSystem;
        [SerializeField] private AdManager adManager;
        [SerializeField] private OfflineIncome offlineIncome;

        public void Configure(
            TMP_Text credits,
            TMP_Text ore,
            TMP_Text science,
            TMP_Text tier,
            TMP_Text prestige,
            TMP_Text offline,
            Slider slider,
            Button click,
            Button ad,
            Button prestigeButtonRef,
            IdleIncomeSystem idle,
            AdManager ads,
            OfflineIncome offlineSystem)
        {
            creditsText = credits;
            oreText = ore;
            scienceText = science;
            tierText = tier;
            prestigeText = prestige;
            offlineText = offline;
            progressSlider = slider;
            clickButton = click;
            adButton = ad;
            prestigeButton = prestigeButtonRef;
            idleIncomeSystem = idle;
            adManager = ads;
            offlineIncome = offlineSystem;
        }

        private void Start()
        {
            if (clickButton == null || adButton == null || prestigeButton == null || idleIncomeSystem == null || adManager == null || offlineIncome == null)
            {
                Debug.LogError("UIManager не сконфигурирован. Проверьте SceneBootstrapper или ссылки в инспекторе.");
                enabled = false;
                return;
            }

            clickButton.onClick.AddListener(idleIncomeSystem.OnClickIncome);
            adButton.onClick.AddListener(adManager.ShowRewardedAd);
            prestigeButton.onClick.AddListener(EconomyManager.Instance.ApplyPrestige);

            EconomyManager.Instance.OnResourcesChanged += Refresh;
            EconomyManager.Instance.OnProgressChanged += Refresh;
            UpgradeSystem.Instance.OnUpgradesChanged += Refresh;

            Refresh();
        }

        private void OnDestroy()
        {
            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.OnResourcesChanged -= Refresh;
                EconomyManager.Instance.OnProgressChanged -= Refresh;
            }

            if (UpgradeSystem.Instance != null)
            {
                UpgradeSystem.Instance.OnUpgradesChanged -= Refresh;
            }
        }

        public void ShowOfflineIncome()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Оффлайн доход:");
            sb.AppendLine($"+{Format(offlineIncome.LastOfflineCredits)} кредитов");
            sb.AppendLine($"+{Format(offlineIncome.LastOfflineOre)} руды");
            sb.AppendLine($"+{Format(offlineIncome.LastOfflineScience)} науки");
            offlineText.text = sb.ToString();
        }

        private void Refresh()
        {
            creditsText.text = $"Кредиты: {Format(EconomyManager.Instance.Credits)}";
            oreText.text = $"Руда: {Format(EconomyManager.Instance.Ore)}";
            scienceText.text = $"Наука: {Format(EconomyManager.Instance.Science)}";
            tierText.text = $"Этап: {EconomyManager.Instance.ProgressTier}/5";
            prestigeText.text = $"Престиж: {EconomyManager.Instance.PrestigeLevel} (x{EconomyManager.Instance.GetPrestigeMultiplier():F2})";
            progressSlider.value = (EconomyManager.Instance.ProgressTier - 1) / 4f;
        }

        private string Format(double value)
        {
            if (value >= 1_000_000_000) return $"{value / 1_000_000_000d:F2}B";
            if (value >= 1_000_000) return $"{value / 1_000_000d:F2}M";
            if (value >= 1_000) return $"{value / 1_000d:F2}K";
            return value.ToString("F1");
        }
    }
}
