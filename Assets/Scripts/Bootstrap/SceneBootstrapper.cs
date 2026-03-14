using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using IdleStarforge.Managers;
using IdleStarforge.Systems;
using IdleStarforge.UI;

namespace IdleStarforge.Bootstrap
{
    public static class SceneBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Build()
        {
            if (Object.FindObjectOfType<GameManager>() != null)
            {
                return;
            }

            EnsureEventSystem();
            EnsureMainCamera();

            var systemsRoot = new GameObject("Systems");
            var economyManager = systemsRoot.AddComponent<EconomyManager>();
            var upgradeSystem = systemsRoot.AddComponent<UpgradeSystem>();
            var saveSystem = systemsRoot.AddComponent<SaveSystem>();
            var adManager = systemsRoot.AddComponent<AdManager>();
            var idleIncome = systemsRoot.AddComponent<IdleIncomeSystem>();
            var offlineIncome = systemsRoot.AddComponent<OfflineIncome>();
            var achievementSystem = systemsRoot.AddComponent<AchievementSystem>();
            var randomBonusSystem = systemsRoot.AddComponent<RandomBonusSystem>();
            var gameManager = systemsRoot.AddComponent<GameManager>();

            _ = economyManager;
            _ = upgradeSystem;

            var canvas = CreateCanvas();
            var uiManager = canvas.gameObject.AddComponent<UIManager>();

            TMP_Text credits = CreateLabel(canvas.transform, "CreditsText", new Vector2(20, -20), "Кредиты: 0", 34);
            TMP_Text ore = CreateLabel(canvas.transform, "OreText", new Vector2(20, -60), "Руда: 0", 30);
            TMP_Text science = CreateLabel(canvas.transform, "ScienceText", new Vector2(20, -95), "Наука: 0", 30);
            TMP_Text tier = CreateLabel(canvas.transform, "TierText", new Vector2(20, -130), "Этап: 1/5", 28);
            TMP_Text prestige = CreateLabel(canvas.transform, "PrestigeText", new Vector2(20, -165), "Престиж: 0", 28);
            TMP_Text offline = CreateLabel(canvas.transform, "OfflineText", new Vector2(20, -205), "Оффлайн доход: 0", 24);

            var progress = CreateSlider(canvas.transform, "ProgressSlider", new Vector2(20, -255), new Vector2(450, 24));
            var clickButton = CreateButton(canvas.transform, "ClickButton", new Vector2(20, -300), new Vector2(250, 70), "Добыть");
            var adButton = CreateButton(canvas.transform, "AdButton", new Vector2(290, -300), new Vector2(250, 70), "Реклама +120 сек");
            var prestigeButton = CreateButton(canvas.transform, "PrestigeButton", new Vector2(560, -300), new Vector2(250, 70), "Престиж");

            CreateShopPlaceholder(canvas.transform);

            uiManager.Configure(
                credits,
                ore,
                science,
                tier,
                prestige,
                offline,
                progress,
                clickButton,
                adButton,
                prestigeButton,
                idleIncome,
                adManager,
                offlineIncome);

            gameManager.Configure(saveSystem, offlineIncome, uiManager, randomBonusSystem, achievementSystem);
        }

        private static void EnsureMainCamera()
        {
            if (Camera.main != null) return;

            var camera = new GameObject("Main Camera");
            camera.tag = "MainCamera";
            var cam = camera.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.08f, 0.12f);
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null) return;

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private static Canvas CreateCanvas()
        {
            var canvasObject = new GameObject("MainCanvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static TMP_Text CreateLabel(Transform parent, string name, Vector2 anchoredPos, string text, int fontSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = new Vector2(700, 32);

            var label = go.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.color = Color.white;
            return label;
        }

        private static Slider CreateSlider(Transform parent, string name, Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;

            var background = new GameObject("Background").AddComponent<Image>();
            background.transform.SetParent(go.transform, false);
            var bgRect = background.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            background.color = new Color(0.15f, 0.15f, 0.15f);

            var fillArea = new GameObject("Fill Area").AddComponent<RectTransform>();
            fillArea.transform.SetParent(go.transform, false);
            fillArea.anchorMin = Vector2.zero;
            fillArea.anchorMax = Vector2.one;
            fillArea.offsetMin = new Vector2(5, 5);
            fillArea.offsetMax = new Vector2(-5, -5);

            var fill = new GameObject("Fill").AddComponent<Image>();
            fill.transform.SetParent(fillArea, false);
            var fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            fill.color = new Color(0.3f, 0.9f, 0.5f);

            var slider = go.AddComponent<Slider>();
            slider.fillRect = fillRect;
            slider.targetGraphic = fill;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            return slider;
        }

        private static Button CreateButton(Transform parent, string name, Vector2 anchoredPos, Vector2 size, string text)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;

            var image = go.AddComponent<Image>();
            image.color = new Color(0.2f, 0.3f, 0.45f);
            var button = go.AddComponent<Button>();

            var label = new GameObject("Label").AddComponent<TextMeshProUGUI>();
            label.transform.SetParent(go.transform, false);
            label.alignment = TextAlignmentOptions.Center;
            label.text = text;
            label.fontSize = 28;
            label.color = Color.white;
            var labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            return button;
        }

        private static void CreateShopPlaceholder(Transform parent)
        {
            var panel = new GameObject("ShopPanel");
            panel.transform.SetParent(parent, false);
            var rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(-20, -20);
            rect.sizeDelta = new Vector2(430, 620);

            var image = panel.AddComponent<Image>();
            image.color = new Color(0.08f, 0.1f, 0.18f, 0.85f);

            var title = CreateLabel(panel.transform, "ShopTitle", new Vector2(16, -16), "Магазин апгрейдов", 32);
            title.color = new Color(0.95f, 0.9f, 0.55f);

            CreateLabel(panel.transform, "ShopHint", new Vector2(16, -60), "Подключите карточки апгрейдов к UpgradeSystem.Definitions", 22);

            var scrollRoot = new GameObject("ShopScroll");
            scrollRoot.transform.SetParent(panel.transform, false);
            var scrollRect = scrollRoot.AddComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0, 0);
            scrollRect.anchorMax = new Vector2(1, 1);
            scrollRect.offsetMin = new Vector2(12, 12);
            scrollRect.offsetMax = new Vector2(-12, -90);

            var scrollImage = scrollRoot.AddComponent<Image>();
            scrollImage.color = new Color(0f, 0f, 0f, 0.22f);
            scrollRoot.AddComponent<Mask>().showMaskGraphic = false;

            var content = new GameObject("Content");
            content.transform.SetParent(scrollRoot.transform, false);
            var contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 900);

            for (int i = 0; i < 10; i++)
            {
                var item = CreateLabel(content.transform, $"UpgradeStub{i + 1}", new Vector2(16, -16 - (i * 78)), $"Апгрейд {i + 1}: +доход", 24);
                item.color = new Color(0.85f, 0.9f, 1f);
            }

            var sv = scrollRoot.AddComponent<ScrollRect>();
            sv.viewport = scrollRect;
            sv.content = contentRect;
            sv.horizontal = false;
            sv.vertical = true;
        }
    }
}
