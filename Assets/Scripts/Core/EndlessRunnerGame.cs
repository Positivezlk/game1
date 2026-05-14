using System;
using System.Collections.Generic;
using NeonDash.Yandex;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NeonDash.Core
{
    public sealed class EndlessRunnerGame : MonoBehaviour
    {
        private enum GameState { Boot, Menu, Playing, Paused, GameOver, Settings, Help }
        private enum MissionType { Distance, Coins, Obstacles }

        private sealed class Mission
        {
            public MissionType type;
            public int target;
            public string Ru;
            public string En;
            public bool completed;
        }

        private readonly List<GameObject> spawned = new();
        private readonly List<Mission> missions = new();
        private readonly Color[] skinColors =
        {
            new(0.0f, 0.85f, 1f),
            new(1f, 0.35f, 0.75f),
            new(1f, 0.82f, 0.1f)
        };

        private YandexGamesBridge bridge;
        private SaveData save;
        private Camera mainCamera;
        private GameObject player;
        private Rigidbody2D playerBody;
        private RectTransform hud;
        private RectTransform menu;
        private RectTransform pausePanel;
        private RectTransform gameOverPanel;
        private RectTransform settingsPanel;
        private RectTransform helpPanel;
        private Text scoreText;
        private Text coinsText;
        private Text bestText;
        private Text missionText;
        private Text titleText;
        private Text finalText;
        private Button rewardButton;
        private Button skinButton;
        private GameState state = GameState.Boot;
        private float speed = 6f;
        private float distance;
        private float spawnTimer;
        private float levelTimer;
        private float difficultyTimer;
        private int runCoins;
        private int passedObstacles;
        private int jumpsLeft;
        private bool grounded;
        private bool muted;
        private string lang = "en";

        private bool IsRu => lang == "ru";
        private string T(string ru, string en) => IsRu ? ru : en;

        private void Start()
        {
            Application.targetFrameRate = 60;
            bridge = new GameObject("YandexGamesBridge").AddComponent<YandexGamesBridge>();
            bridge.PauseByAdChanged += OnAdPauseChanged;
            save = bridge.LoadSave();
            lang = bridge.Language;

            BuildWorld();
            BuildUi();
            CreateMissions();
            ApplySkin();
            ShowMenu();
            bridge.MarkGameReady();
        }

        private void Update()
        {
            if (state == GameState.Playing)
                TickGameplay();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (state == GameState.Playing)
                    PauseGame();
                else if (state == GameState.Paused || state == GameState.Settings || state == GameState.Help)
                    ShowMenu();
            }
        }

        private void TickGameplay()
        {
            if (bridge.IsFullscreenAdActive)
                return;

            var dt = Time.deltaTime;
            distance += speed * dt;
            levelTimer += dt;
            difficultyTimer += dt;
            spawnTimer -= dt;
            speed = Mathf.Min(18f, speed + dt * 0.035f);

            if (difficultyTimer >= 45f)
            {
                difficultyTimer = 0f;
                CompleteMission(MissionType.Distance, Mathf.RoundToInt(distance));
            }

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetMouseButtonDown(0) || TouchStarted())
                Jump();

            if (spawnTimer <= 0f)
                SpawnPattern();

            MoveSpawned(dt);
            UpdateHud();

            if (levelTimer >= 180f)
            {
                levelTimer = 0f;
                bridge.ShowInterstitial(FullscreenAdReason.LevelComplete, () => { if (state == GameState.Playing) bridge.GameplayStart(); });
            }
        }

        private static bool TouchStarted()
        {
            for (var i = 0; i < Input.touchCount; i++)
                if (Input.GetTouch(i).phase == TouchPhase.Began)
                    return true;
            return false;
        }

        private void BuildWorld()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
                mainCamera = new GameObject("MainCamera").AddComponent<Camera>();
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 5f;
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = new Color(0.04f, 0.05f, 0.1f);

            var lightObject = new GameObject("DirectionalLight");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 0.8f;

            CreateBlock("Ground", new Vector2(0f, -3.6f), new Vector2(22f, 0.55f), new Color(0.12f, 0.16f, 0.22f));
            player = CreateBlock("Player", new Vector2(-5f, -2.65f), new Vector2(0.75f, 0.75f), skinColors[0]);
            player.layer = LayerMask.NameToLayer("Player");
            playerBody = player.AddComponent<Rigidbody2D>();
            playerBody.freezeRotation = true;
            playerBody.gravityScale = 3f;
            var playerCollider = player.AddComponent<BoxCollider2D>();
            playerCollider.size = Vector2.one;
        }

        private GameObject CreateBlock(string name, Vector2 pos, Vector2 scale, Color color)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = scale;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            renderer.color = color;
            return go;
        }

        private void BuildUi()
        {
            var eventSystem = FindObjectOfType<EventSystem>() ?? new GameObject("EventSystem").AddComponent<EventSystem>();
            if (eventSystem.GetComponent<StandaloneInputModule>() == null)
                eventSystem.gameObject.AddComponent<StandaloneInputModule>();

            var canvasObject = new GameObject("Canvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1280, 720);
            canvasObject.GetComponent<CanvasScaler>().matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            hud = Panel(canvasObject.transform, "HUD", new Color(0, 0, 0, 0));
            scoreText = Label(hud, "Score", Anchor.TopLeft, new Vector2(24, -24), 28, TextAnchor.UpperLeft);
            coinsText = Label(hud, "Coins", Anchor.TopRight, new Vector2(-24, -24), 28, TextAnchor.UpperRight);
            missionText = Label(hud, "Mission", Anchor.Bottom, new Vector2(0, 26), 24, TextAnchor.LowerCenter);

            menu = Panel(canvasObject.transform, "MainMenu", new Color(0.02f, 0.03f, 0.08f, 0.92f));
            titleText = Label(menu, "Title", Anchor.Top, new Vector2(0, -90), 58, TextAnchor.MiddleCenter);
            bestText = Label(menu, "Best", Anchor.Top, new Vector2(0, -158), 28, TextAnchor.MiddleCenter);
            Button(menu, "Play", Anchor.Middle, new Vector2(0, 80), T("Играть", "Play"), StartRun);
            Button(menu, "Help", Anchor.Middle, new Vector2(0, 10), T("Управление", "Controls"), ShowHelp);
            Button(menu, "Settings", Anchor.Middle, new Vector2(0, -60), T("Настройки", "Settings"), ShowSettings);
            skinButton = Button(menu, "Skin", Anchor.Middle, new Vector2(0, -130), string.Empty, BuyOrSelectSkin);

            pausePanel = StandardPanel(canvasObject.transform, "PausePanel", T("Пауза", "Paused"));
            Button(pausePanel, "Resume", Anchor.Middle, new Vector2(0, 70), T("Продолжить", "Resume"), ResumeGame);
            Button(pausePanel, "Menu", Anchor.Middle, new Vector2(0, 0), T("В меню", "Menu"), ShowMenu);

            gameOverPanel = StandardPanel(canvasObject.transform, "GameOverPanel", T("Игра окончена", "Game Over"));
            finalText = Label(gameOverPanel, "Final", Anchor.Middle, new Vector2(0, 80), 28, TextAnchor.MiddleCenter);
            Button(gameOverPanel, "Restart", Anchor.Middle, new Vector2(0, 0), T("Заново", "Restart"), StartRun);
            rewardButton = Button(gameOverPanel, "Reward", Anchor.Middle, new Vector2(0, -70), T("Смотреть видео: +25 монет", "Watch video: +25 coins"), WatchRewarded);
            Button(gameOverPanel, "Menu", Anchor.Middle, new Vector2(0, -140), T("В меню", "Menu"), ShowMenu);

            settingsPanel = StandardPanel(canvasObject.transform, "SettingsPanel", T("Настройки", "Settings"));
            Button(settingsPanel, "Language", Anchor.Middle, new Vector2(0, 80), T("Language: RU", "Язык: EN"), ToggleLanguage);
            Button(settingsPanel, "Mute", Anchor.Middle, new Vector2(0, 10), T("Звук вкл/выкл", "Sound on/off"), ToggleMute);
            Button(settingsPanel, "Back", Anchor.Middle, new Vector2(0, -80), T("Назад", "Back"), ShowMenu);

            helpPanel = StandardPanel(canvasObject.transform, "HelpPanel", T("Управление", "Controls"));
            Label(helpPanel, "HelpText", Anchor.Middle, new Vector2(0, 20), 28, TextAnchor.MiddleCenter).text = T(
                "Desktop: Space/↑/мышь — прыжок, Esc — пауза\nMobile: касание — прыжок\nСобирайте монеты, выполняйте миссии и избегайте препятствий.",
                "Desktop: Space/↑/mouse — jump, Esc — pause\nMobile: tap — jump\nCollect coins, finish missions, avoid obstacles.");
            Button(helpPanel, "Back", Anchor.Bottom, new Vector2(0, 90), T("Назад", "Back"), ShowMenu);
        }

        private RectTransform StandardPanel(Transform parent, string name, string title)
        {
            var panel = Panel(parent, name, new Color(0.02f, 0.03f, 0.08f, 0.94f));
            Label(panel, name + "Title", Anchor.Top, new Vector2(0, -100), 52, TextAnchor.MiddleCenter).text = title;
            return panel;
        }

        private enum Anchor { TopLeft, TopRight, Top, Middle, Bottom }

        private RectTransform Panel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var image = go.AddComponent<Image>();
            image.color = color;
            return rect;
        }

        private Text Label(Transform parent, string name, Anchor anchor, Vector2 pos, int size, TextAnchor alignment)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            Place(rect, anchor, pos, new Vector2(900, 80));
            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = size;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 16;
            text.resizeTextMaxSize = size;
            text.color = Color.white;
            text.alignment = alignment;
            return text;
        }

        private Button Button(Transform parent, string name, Anchor anchor, Vector2 pos, string caption, UnityEngine.Events.UnityAction click)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            Place(rect, anchor, pos, new Vector2(420, 58));
            var image = go.AddComponent<Image>();
            image.color = new Color(0f, 0.55f, 0.9f, 0.95f);
            var button = go.AddComponent<Button>();
            button.onClick.AddListener(click);
            var text = Label(go.transform, name + "Text", Anchor.Middle, Vector2.zero, 24, TextAnchor.MiddleCenter);
            text.text = caption;
            text.raycastTarget = false;
            return button;
        }

        private static void Place(RectTransform rect, Anchor anchor, Vector2 pos, Vector2 size)
        {
            rect.sizeDelta = size;
            switch (anchor)
            {
                case Anchor.TopLeft: rect.anchorMin = rect.anchorMax = new Vector2(0, 1); rect.pivot = new Vector2(0, 1); break;
                case Anchor.TopRight: rect.anchorMin = rect.anchorMax = new Vector2(1, 1); rect.pivot = new Vector2(1, 1); break;
                case Anchor.Top: rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1); rect.pivot = new Vector2(0.5f, 1); break;
                case Anchor.Bottom: rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0); rect.pivot = new Vector2(0.5f, 0); break;
                default: rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f); rect.pivot = new Vector2(0.5f, 0.5f); break;
            }
            rect.anchoredPosition = pos;
        }

        private void StartRun()
        {
            ClearSpawned();
            state = GameState.Playing;
            speed = 6f;
            distance = 0;
            runCoins = 0;
            passedObstacles = 0;
            levelTimer = 0;
            difficultyTimer = 0;
            spawnTimer = 0.5f;
            player.transform.position = new Vector2(-5f, -2.65f);
            playerBody.velocity = Vector2.zero;
            jumpsLeft = 2;
            grounded = true;
            SetPanel(hud, true);
            SetPanel(menu, false);
            SetPanel(pausePanel, false);
            SetPanel(gameOverPanel, false);
            SetPanel(settingsPanel, false);
            SetPanel(helpPanel, false);
            bridge.GameplayStart();
        }

        private void PauseGame()
        {
            state = GameState.Paused;
            Time.timeScale = 0f;
            SetPanel(pausePanel, true);
            bridge.GameplayStop();
        }

        private void ResumeGame()
        {
            Time.timeScale = 1f;
            state = GameState.Playing;
            SetPanel(pausePanel, false);
            bridge.GameplayStart();
        }

        private void ShowMenu()
        {
            Time.timeScale = 1f;
            state = GameState.Menu;
            bridge.GameplayStop();
            titleText.text = "NEON DASH";
            bestText.text = T($"Рекорд: {save.bestScore}   Монеты: {save.coins}", $"Best: {save.bestScore}   Coins: {save.coins}");
            skinButton.GetComponentInChildren<Text>().text = T($"Скин: {save.selectedSkin + 1}/3", $"Skin: {save.selectedSkin + 1}/3");
            SetPanel(hud, false);
            SetPanel(menu, true);
            SetPanel(pausePanel, false);
            SetPanel(gameOverPanel, false);
            SetPanel(settingsPanel, false);
            SetPanel(helpPanel, false);
        }

        private void ShowSettings()
        {
            state = GameState.Settings;
            SetPanel(menu, false);
            SetPanel(settingsPanel, true);
        }

        private void ShowHelp()
        {
            state = GameState.Help;
            SetPanel(menu, false);
            SetPanel(helpPanel, true);
        }

        private static void SetPanel(RectTransform panel, bool active) => panel.gameObject.SetActive(active);

        private void Jump()
        {
            if (jumpsLeft <= 0)
                return;
            playerBody.velocity = new Vector2(0, 0);
            playerBody.AddForce(Vector2.up * 9.5f, ForceMode2D.Impulse);
            jumpsLeft--;
            grounded = false;
        }

        private void FixedUpdate()
        {
            if (state != GameState.Playing)
                return;

            if (player.transform.position.y <= -2.65f)
            {
                var p = player.transform.position;
                p.y = -2.65f;
                player.transform.position = p;
                grounded = true;
                jumpsLeft = 2;
            }
            else if (grounded)
            {
                grounded = false;
            }
        }

        private void SpawnPattern()
        {
            spawnTimer = UnityEngine.Random.Range(1.0f, Mathf.Max(1.15f, 2.0f - speed * 0.045f));
            var roll = UnityEngine.Random.value;
            if (roll < 0.45f)
                SpawnObstacle("Crate", new Vector2(8.8f, -2.75f), new Vector2(0.7f, 0.9f), new Color(1f, 0.25f, 0.22f));
            else if (roll < 0.75f)
                SpawnObstacle("TallGate", new Vector2(8.8f, -2.45f), new Vector2(0.55f, 1.5f), new Color(0.9f, 0.2f, 1f));
            else
                SpawnObstacle("Drone", new Vector2(8.8f, -1.25f), new Vector2(0.85f, 0.45f), new Color(1f, 0.58f, 0.12f));

            if (UnityEngine.Random.value < 0.72f)
            {
                for (var i = 0; i < 3; i++)
                    SpawnCoin(new Vector2(9.4f + i * 0.65f, UnityEngine.Random.Range(-1.8f, 0.6f)));
            }
        }

        private void SpawnObstacle(string name, Vector2 pos, Vector2 scale, Color color)
        {
            var obstacle = CreateBlock(name, pos, scale, color);
            obstacle.AddComponent<BoxCollider2D>().isTrigger = true;
            spawned.Add(obstacle);
        }

        private void SpawnCoin(Vector2 pos)
        {
            var coin = CreateBlock("Coin", pos, new Vector2(0.35f, 0.35f), new Color(1f, 0.9f, 0.1f));
            coin.AddComponent<CircleCollider2D>().isTrigger = true;
            spawned.Add(coin);
        }

        private void MoveSpawned(float dt)
        {
            for (var i = spawned.Count - 1; i >= 0; i--)
            {
                var item = spawned[i];
                if (item == null)
                {
                    spawned.RemoveAt(i);
                    continue;
                }

                item.transform.position += Vector3.left * speed * dt;
                var dist = Vector2.Distance(player.transform.position, item.transform.position);
                if (dist < 0.75f && item.name.StartsWith("Coin", StringComparison.Ordinal))
                {
                    runCoins++;
                    save.coins++;
                    CompleteMission(MissionType.Coins, runCoins);
                    bridge.Save(save);
                    Destroy(item);
                    spawned.RemoveAt(i);
                    continue;
                }

                if (dist < 0.72f && !item.name.StartsWith("Coin", StringComparison.Ordinal))
                {
                    GameOver();
                    return;
                }

                if (item.transform.position.x < -7.5f)
                {
                    if (!item.name.StartsWith("Coin", StringComparison.Ordinal))
                    {
                        passedObstacles++;
                        CompleteMission(MissionType.Obstacles, passedObstacles);
                    }
                    Destroy(item);
                    spawned.RemoveAt(i);
                }
            }
        }

        private void GameOver()
        {
            state = GameState.GameOver;
            bridge.GameplayStop();
            var score = Mathf.RoundToInt(distance + runCoins * 5);
            save.bestScore = Mathf.Max(save.bestScore, score);
            save.totalRuns++;
            bridge.Save(save);
            finalText.text = T($"Счёт: {score}\nМонеты за забег: {runCoins}", $"Score: {score}\nRun coins: {runCoins}");
            SetPanel(hud, false);
            SetPanel(gameOverPanel, true);
            bridge.ShowInterstitial(FullscreenAdReason.GameOver, () => { });
        }

        private void UpdateHud()
        {
            var score = Mathf.RoundToInt(distance + runCoins * 5);
            scoreText.text = T($"Счёт: {score}", $"Score: {score}");
            coinsText.text = T($"Монеты: {save.coins}", $"Coins: {save.coins}");
            var mission = missions.Find(m => !m.completed) ?? missions[^1];
            missionText.text = mission.completed ? T("Все миссии выполнены!", "All missions complete!") : (IsRu ? mission.Ru : mission.En);
        }

        private void CreateMissions()
        {
            missions.Clear();
            missions.Add(new Mission { type = MissionType.Distance, target = 500, Ru = "Миссия: пробегите 500 м", En = "Mission: run 500 m" });
            missions.Add(new Mission { type = MissionType.Coins, target = 25, Ru = "Миссия: соберите 25 монет", En = "Mission: collect 25 coins" });
            missions.Add(new Mission { type = MissionType.Obstacles, target = 40, Ru = "Миссия: обойдите 40 препятствий", En = "Mission: pass 40 obstacles" });
            for (var i = 0; i < save.completedMissions && i < missions.Count; i++)
                missions[i].completed = true;
        }

        private void CompleteMission(MissionType type, int value)
        {
            var mission = missions.Find(m => !m.completed && m.type == type && value >= m.target);
            if (mission == null)
                return;
            mission.completed = true;
            save.completedMissions = Mathf.Max(save.completedMissions, missions.FindIndex(m => m == mission) + 1);
            save.coins += 50;
            bridge.Save(save);
        }

        private void WatchRewarded()
        {
            rewardButton.interactable = false;
            bridge.ShowRewarded(() =>
            {
                save.coins += 25;
                bridge.Save(save);
                rewardButton.interactable = true;
                finalText.text += T("\nНаграда получена: +25 монет", "\nReward granted: +25 coins");
            });
        }

        private void BuyOrSelectSkin()
        {
            var next = (save.selectedSkin + 1) % skinColors.Length;
            if (next > 0 && save.coins < 100)
                return;
            if (next > 0)
                save.coins -= 100;
            save.selectedSkin = next;
            ApplySkin();
            bridge.Save(save);
            ShowMenu();
        }

        private void ApplySkin()
        {
            if (player != null)
                player.GetComponent<SpriteRenderer>().color = skinColors[Mathf.Clamp(save.selectedSkin, 0, skinColors.Length - 1)];
        }

        private void ToggleLanguage()
        {
            lang = IsRu ? "en" : "ru";
            BuildUiRefresh();
        }

        private void ToggleMute()
        {
            muted = !muted;
            AudioListener.volume = muted ? 0f : 1f;
        }

        private void BuildUiRefresh()
        {
            Destroy(hud.parent.gameObject);
            BuildUi();
            ShowSettings();
        }

        private void OnAdPauseChanged(bool paused)
        {
            Time.timeScale = paused ? 0f : 1f;
        }

        private void ClearSpawned()
        {
            foreach (var item in spawned)
                if (item != null)
                    Destroy(item);
            spawned.Clear();
        }
    }
}
