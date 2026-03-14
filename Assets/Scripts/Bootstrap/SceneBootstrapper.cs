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

            Text credits = CreateLabel(canvas.transform, "CreditsText", new Vector2(20, -20), "Кредиты: 0", 28);
            Text ore = CreateLabel(canvas.transform, "OreText", new Vector2(20, -58), "Руда: 0", 24);
            Text science = CreateLabel(canvas.transform, "ScienceText", new Vector2(20, -90), "Наука: 0", 24);
            Text tier = CreateLabel(canvas.transform, "TierText", new Vector2(20, -122), "Этап: 1/5", 22);
            Text prestige = CreateLabel(canvas.transform, "PrestigeText", new Vector2(20, -152), "Престиж: 0", 22);
            Text offline = CreateLabel(canvas.transform, "OfflineText", new Vector2(20, -185), "Оффлайн доход: 0", 20);

            var progress = CreateSlider(canvas.transform, "ProgressSlider", new Vector2(20, -240), new Vector2(450, 24));
            var clickButton = CreateButton(canvas.transform, "ClickButton", new Vector2(20, -280), new Vector2(250, 70), "Добыть");
            var adButton = CreateButton(canvas.transform, "AdButton", new Vector2(290, -280), new Vector2(250, 70), "Реклама +120 сек");
            var prestigeButton = CreateButton(canvas.transform, "PrestigeButton", new Vector2(560, -280), new Vector2(250, 70), "Престиж");

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

        private static Font GetDefaultFont()
        {
            return Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        private static void EnsureMainCamera()
        {
            if (Camera.main != null) return;

            var camera = new GameObject("Main Camera");
            camera.tag = "MainCamera";
            var cam = camera.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.08f, 0.12f);
            cam.orthographic = true;
            cam.orthographicSize = 5f;
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

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static Text CreateLabel(Transform parent, string name, Vector2 anchoredPos, string text, int fontSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = new Vector2(760, 34);

            var label = go.AddComponent<Text>();
            label.text = text;
            label.font = GetDefaultFont();
            label.fontSize = fontSize;
            label.color = Color.white;
            label.alignment = TextAnchor.MiddleLeft;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
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

            var backgroundObj = new GameObject("Background");
            backgroundObj.transform.SetParent(go.transform, false);
            var background = backgroundObj.AddComponent<Image>();
            var bgRect = background.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            background.color = new Color(0.15f, 0.15f, 0.15f);

            var fillAreaObj = new GameObject("Fill Area");
            fillAreaObj.transform.SetParent(go.transform, false);
            var fillArea = fillAreaObj.AddComponent<RectTransform>();
            fillArea.anchorMin = Vector2.zero;
            fillArea.anchorMax = Vector2.one;
            fillArea.offsetMin = new Vector2(5, 5);
            fillArea.offsetMax = new Vector2(-5, -5);

            var fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(fillArea, false);
            var fill = fillObj.AddComponent<Image>();
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

            var labelObject = new GameObject("Label");
            labelObject.transform.SetParent(go.transform, false);
            var labelRect = labelObject.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var label = labelObject.AddComponent<Text>();
            label.text = text;
            label.font = GetDefaultFont();
            label.fontSize = 26;
            label.color = Color.white;
            label.alignment = TextAnchor.MiddleCenter;

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
            image.color = new Color(0.08f, 0.1f, 0.18f, 0.9f);

            var title = CreateLabel(panel.transform, "ShopTitle", new Vector2(16, -16), "Магазин апгрейдов", 30);
            title.color = new Color(0.95f, 0.9f, 0.55f);

            CreateLabel(panel.transform, "ShopHint", new Vector2(16, -56), "Подключите карточки апгрейдов к UpgradeSystem.Definitions", 18);

            var scrollRoot = new GameObject("ShopScroll");
            scrollRoot.transform.SetParent(panel.transform, false);
            var scrollRectTransform = scrollRoot.AddComponent<RectTransform>();
            scrollRectTransform.anchorMin = new Vector2(0, 0);
            scrollRectTransform.anchorMax = new Vector2(1, 1);
            scrollRectTransform.offsetMin = new Vector2(12, 12);
            scrollRectTransform.offsetMax = new Vector2(-12, -90);

            var scrollImage = scrollRoot.AddComponent<Image>();
            scrollImage.color = new Color(0f, 0f, 0f, 0.22f);
            var mask = scrollRoot.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            var content = new GameObject("Content");
            content.transform.SetParent(scrollRoot.transform, false);
            var contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.sizeDelta = new Vector2(0, 900);

            for (int i = 0; i < 12; i++)
            {
                var item = CreateLabel(content.transform, $"UpgradeStub{i + 1}", new Vector2(12, -12 - (i * 70)), $"Апгрейд {i + 1}: +доход", 20);
                item.color = new Color(0.85f, 0.9f, 1f);
            }

            var scroll = scrollRoot.AddComponent<ScrollRect>();
            scroll.viewport = scrollRectTransform;
            scroll.content = contentRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.scrollSensitivity = 25f;
        }
    }
}
