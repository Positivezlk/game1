using UnityEngine;
using IdleStarforge.Data;
using IdleStarforge.Managers;

namespace IdleStarforge.Systems
{
    public class RandomBonusSystem : MonoBehaviour
    {
        [SerializeField] private Vector2 intervalRange = new Vector2(25f, 55f);
        [SerializeField] private double rewardSeconds = 45d;

        private float timer;
        private float nextSpawn;

        private void Awake()
        {
            ScheduleNext();
        }

        public void StartBonuses()
        {
            timer = 0f;
            ScheduleNext();
        }

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer < nextSpawn) return;

            timer = 0f;
            TriggerBonus();
            ScheduleNext();
        }

        private void TriggerBonus()
        {
            double creditsPerSecond = EconomyManager.Instance.GetPassiveCreditsPerSecond(UpgradeSystem.Instance.GetTotalBonus(ResourceType.Credits));
            double reward = creditsPerSecond * rewardSeconds;
            EconomyManager.Instance.AddResource(ResourceType.Credits, reward);
            Debug.Log($"Случайный бонус: +{reward:F1} кредитов");
        }

        private void ScheduleNext()
        {
            nextSpawn = Random.Range(intervalRange.x, intervalRange.y);
        }
    }
}
