using UnityEngine;
using IdleStarforge.Data;
using IdleStarforge.Managers;

namespace IdleStarforge.Systems
{
    public class IdleIncomeSystem : MonoBehaviour
    {
        [SerializeField] private float tickRate = 0.2f;
        private float timer;

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer < tickRate) return;

            timer = 0f;
            ApplyTickIncome(tickRate);
        }

        public void ApplyTickIncome(float delta)
        {
            double creditsBonus = UpgradeSystem.Instance.GetTotalBonus(ResourceType.Credits);
            double oreBonus = UpgradeSystem.Instance.GetTotalBonus(ResourceType.Ore);
            double scienceBonus = UpgradeSystem.Instance.GetTotalBonus(ResourceType.Science);

            double global = UpgradeSystem.Instance.GetGlobalUpgradeMultiplier();
            EconomyManager.Instance.SetMultipliers(1d, 1d, global);

            double credits = EconomyManager.Instance.GetPassiveCreditsPerSecond(creditsBonus) * delta;
            double ore = EconomyManager.Instance.GetOrePerSecond(oreBonus) * delta;
            double science = EconomyManager.Instance.GetSciencePerSecond(scienceBonus) * delta;

            EconomyManager.Instance.AddResource(ResourceType.Credits, credits);
            EconomyManager.Instance.AddResource(ResourceType.Ore, ore);
            EconomyManager.Instance.AddResource(ResourceType.Science, science);
        }

        public void OnClickIncome()
        {
            EconomyManager.Instance.AddResource(ResourceType.Credits, EconomyManager.Instance.GetClickIncome());
        }
    }
}
