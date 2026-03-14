using System.Runtime.InteropServices;
using UnityEngine;
using IdleStarforge.Data;

namespace IdleStarforge.Managers
{
    public class AdManager : MonoBehaviour
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void YG_ShowRewarded(string callbackObject);
#endif

        [SerializeField] private double rewardedMultiplierSeconds = 120d;
        [SerializeField] private float pauseScale = 0f;

        private bool waitingReward;

        public void ShowRewardedAd()
        {
            if (waitingReward) return;

            waitingReward = true;
            Time.timeScale = pauseScale;

#if UNITY_WEBGL && !UNITY_EDITOR
            YG_ShowRewarded(gameObject.name);
#else
            OnRewardedSuccess();
#endif
        }

        public void OnRewardedSuccess()
        {
            double creditsPerSecond = EconomyManager.Instance.GetPassiveCreditsPerSecond(UpgradeSystem.Instance.GetTotalBonus(ResourceType.Credits));
            double reward = creditsPerSecond * rewardedMultiplierSeconds;
            EconomyManager.Instance.AddResource(ResourceType.Credits, reward);

            waitingReward = false;
            Time.timeScale = 1f;
        }

        public void OnRewardedFailed()
        {
            waitingReward = false;
            Time.timeScale = 1f;
        }
    }
}
