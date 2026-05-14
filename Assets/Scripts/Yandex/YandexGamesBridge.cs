using System;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

namespace NeonDash.Yandex
{
    public enum FullscreenAdReason
    {
        GameOver,
        LevelComplete
    }

    [Serializable]
    public sealed class SaveData
    {
        public int bestScore;
        public int coins;
        public int selectedSkin;
        public int completedMissions;
        public int totalRuns;
    }

    public sealed class YandexGamesBridge : MonoBehaviour
    {
        private const string SaveKey = "neon_dash_save_v1";
        private static YandexGamesBridge instance;
        private bool readySent;
        private bool sdkInitialized;
        private string language = "en";
        private Action rewardedCallback;
        private Action fullscreenClosedCallback;

        public static YandexGamesBridge Instance => instance;
        public bool IsFullscreenAdActive { get; private set; }
        public string Language => language;
        public event Action<bool> PauseByAdChanged;

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void YGBridgeInit(string gameObjectName);
        [DllImport("__Internal")] private static extern void YGBridgeReady();
        [DllImport("__Internal")] private static extern void YGBridgeGameplayStart();
        [DllImport("__Internal")] private static extern void YGBridgeGameplayStop();
        [DllImport("__Internal")] private static extern void YGBridgeShowInterstitial(string reason);
        [DllImport("__Internal")] private static extern void YGBridgeShowRewarded();
        [DllImport("__Internal")] private static extern string YGBridgeGetLanguage();
        [DllImport("__Internal")] private static extern string YGBridgeLoad();
        [DllImport("__Internal")] private static extern void YGBridgeSave(string json);
#endif

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSdk();
        }

        public void InitializeSdk()
        {
            if (sdkInitialized)
                return;

#if UNITY_WEBGL && !UNITY_EDITOR
            YGBridgeInit(gameObject.name);
            var sdkLanguage = YGBridgeGetLanguage();
            if (!string.IsNullOrEmpty(sdkLanguage))
                language = NormalizeLanguage(sdkLanguage);
#else
            language = NormalizeLanguage(Application.systemLanguage == SystemLanguage.Russian ? "ru" : "en");
#endif
            var pluginLanguage = TryGetPluginLanguage();
            if (!string.IsNullOrEmpty(pluginLanguage))
                language = NormalizeLanguage(pluginLanguage);

            RegisterPluginAdEvents();
            sdkInitialized = true;
            Debug.Log($"Yandex SDK initialized, language={language}");
        }

        public void MarkGameReady()
        {
            if (readySent)
                return;

            readySent = true;
            if (TryCallPlugin("GameReadyAPI"))
                return;

#if UNITY_WEBGL && !UNITY_EDITOR
            YGBridgeReady();
#else
            Debug.Log("LoadingAPI.ready() simulated in Editor");
#endif
        }

        public void GameplayStart()
        {
            if (TryCallPlugin("GameplayStart"))
                return;
#if UNITY_WEBGL && !UNITY_EDITOR
            YGBridgeGameplayStart();
#endif
        }

        public void GameplayStop()
        {
            if (TryCallPlugin("GameplayStop"))
                return;
#if UNITY_WEBGL && !UNITY_EDITOR
            YGBridgeGameplayStop();
#endif
        }

        public void ShowInterstitial(FullscreenAdReason reason, Action onClosed)
        {
            fullscreenClosedCallback = onClosed;
            SetAdPause(true);

            if (TryCallPlugin("InterstitialAdvShow"))
            {
                CancelInvoke(nameof(OnInterstitialClosedFromJs));
                Invoke(nameof(OnInterstitialClosedFromJs), 15f);
                return;
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            YGBridgeShowInterstitial(reason.ToString());
#else
            Invoke(nameof(OnInterstitialClosedFromJs), 0.4f);
#endif
        }

        public void ShowRewarded(Action onReward)
        {
            rewardedCallback = onReward;
            SetAdPause(true);

            if (TryCallPlugin("RewardedAdvShow", "coins_25", new Action(OnRewardedGrantedFromJs)) || TryCallPlugin("RewardedAdvShow", "coins_25"))
                return;

#if UNITY_WEBGL && !UNITY_EDITOR
            YGBridgeShowRewarded();
#else
            Invoke(nameof(OnRewardedGrantedFromJs), 0.4f);
#endif
        }

        public SaveData LoadSave()
        {
            var json = string.Empty;
            if (TryLoadFromPlugin(out var pluginJson))
                json = pluginJson;
#if UNITY_WEBGL && !UNITY_EDITOR
            if (string.IsNullOrEmpty(json))
                json = YGBridgeLoad();
#endif
            if (string.IsNullOrEmpty(json))
                json = PlayerPrefs.GetString(SaveKey, string.Empty);

            if (string.IsNullOrEmpty(json))
                return new SaveData();

            try
            {
                return JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Save parse failed: {exception.Message}");
                return new SaveData();
            }
        }

        public void Save(SaveData data)
        {
            var json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
            TrySaveToPlugin(data, json);
#if UNITY_WEBGL && !UNITY_EDITOR
            YGBridgeSave(json);
#endif
        }

        public void OnInterstitialClosedFromJs()
        {
            CancelInvoke(nameof(OnInterstitialClosedFromJs));
            SetAdPause(false);
            fullscreenClosedCallback?.Invoke();
            fullscreenClosedCallback = null;
        }

        public void OnRewardedGrantedFromJs()
        {
            rewardedCallback?.Invoke();
            rewardedCallback = null;
            SetAdPause(false);
            fullscreenClosedCallback?.Invoke();
            fullscreenClosedCallback = null;
        }

        public void OnRewardedClosedWithoutRewardFromJs()
        {
            rewardedCallback = null;
            SetAdPause(false);
            fullscreenClosedCallback?.Invoke();
            fullscreenClosedCallback = null;
        }

        private void SetAdPause(bool paused)
        {
            IsFullscreenAdActive = paused;
            AudioListener.pause = paused;
            if (paused)
                GameplayStop();
            else
                GameplayStart();
            PauseByAdChanged?.Invoke(paused);
        }

        private void RegisterPluginAdEvents()
        {
            var type = FindYG2Type();
            if (type == null)
                return;

            TrySubscribe(type, "onOpenAnyAdv", new Action(OnPluginAdOpened));
            TrySubscribe(type, "onCloseAnyAdv", new Action(OnPluginAdClosed));
        }

        private void OnPluginAdOpened() => SetAdPause(true);

        private void OnPluginAdClosed() => OnInterstitialClosedFromJs();

        private static void TrySubscribe(Type type, string eventName, Delegate handler)
        {
            var eventInfo = type.GetEvent(eventName, BindingFlags.Public | BindingFlags.Static);
            if (eventInfo == null)
                return;

            try
            {
                eventInfo.AddEventHandler(null, handler);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"PluginYG event subscription {eventName} failed: {exception.Message}");
            }
        }

        private static string NormalizeLanguage(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
                return "en";
            return source.ToLowerInvariant().StartsWith("ru", StringComparison.Ordinal) ? "ru" : "en";
        }

        private static Type FindYG2Type() => Type.GetType("YG.YG2, Assembly-CSharp") ?? Type.GetType("YG.YG2");

        private static bool TryCallPlugin(string methodName, params object[] args)
        {
            var type = FindYG2Type();
            if (type == null)
                return false;

            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                if (method.Name != methodName || method.GetParameters().Length != args.Length)
                    continue;

                try
                {
                    method.Invoke(null, args.Length == 0 ? null : args);
                    return true;
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"PluginYG call {methodName} failed: {exception.Message}");
                }
            }

            return false;
        }

        private static string TryGetPluginLanguage()
        {
            var type = FindYG2Type();
            var envir = type?.GetField("envir", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
            var languageField = envir?.GetType().GetField("language", BindingFlags.Public | BindingFlags.Instance)
                ?? envir?.GetType().GetField("lang", BindingFlags.Public | BindingFlags.Instance);
            return languageField?.GetValue(envir)?.ToString();
        }

        private static bool TryLoadFromPlugin(out string json)
        {
            json = string.Empty;
            var type = FindYG2Type();
            var saves = type?.GetField("saves", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
            var field = saves?.GetType().GetField("neonDashJson", BindingFlags.Public | BindingFlags.Instance);
            json = field?.GetValue(saves)?.ToString();
            return !string.IsNullOrEmpty(json);
        }

        private static void TrySaveToPlugin(SaveData data, string json)
        {
            var type = FindYG2Type();
            var saves = type?.GetField("saves", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
            if (saves != null)
            {
                SetField(saves, "neonDashJson", json);
                SetField(saves, "bestScore", data.bestScore);
                SetField(saves, "coins", data.coins);
                SetField(saves, "selectedSkin", data.selectedSkin);
            }

            TryCallPlugin("SaveProgress");
        }

        private static void SetField(object target, string name, object value)
        {
            var field = target.GetType().GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (field != null)
                field.SetValue(target, value);
        }
    }
}
