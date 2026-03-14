using UnityEngine;

namespace IdleStarforge.Yandex
{
    public class YandexGamesBridge : MonoBehaviour
    {
        public void OnRewardedAdCompleted(string callbackTarget)
        {
            var target = GameObject.Find(callbackTarget);
            if (target != null)
            {
                target.SendMessage("OnRewardedSuccess", SendMessageOptions.DontRequireReceiver);
            }
        }

        public void OnRewardedAdFailed(string callbackTarget)
        {
            var target = GameObject.Find(callbackTarget);
            if (target != null)
            {
                target.SendMessage("OnRewardedFailed", SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}
