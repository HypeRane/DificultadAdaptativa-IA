using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Construye y maneja todo el HUD por código: vida, puntaje/combo, panel de telemetría de la
// IA de dificultad (el objetivo demostrativo del proyecto), crosshair, avisos y pantalla de game over.
public class HUDController : MonoBehaviour
{
    public static HUDController Instance { get; private set; }

    private const float DamageFlashDuration = 0.35f;
    private const float StatsPollInterval = 0.25f;

    private RectTransform floatingLayer;
    private Image vignetteImage;

    // Vida
    private Image healthChipBar;
    private Image healthFrontBar;
    private Text healthValueText;
    private float displayedHealthFraction = 1f;
    private float chipHealthFraction = 1f;

    // Puntaje
    private Text scoreText;
    private Text comboText;
    private Text timeText;
    private Coroutine comboPunchRoutine;

    // Dificultad
    private Text difficultyLevelText;
    private Image difficultyLevelBar;
    private Text statsText;
    private float statsPollTimer;

    // Aviso de cambio de dificultad
    private RectTransform toastRoot;
    private CanvasGroup toastGroup;
    private Text toastText;
    private Coroutine toastRoutine;
    private const float ToastBaseY = -170f;

    // Daño / vida baja
    private Image damageFlashImage;
    private float damageFlashTimer;

    // Crosshair
    private RectTransform crosshair;

    // Aviso inicial de controles
    private CanvasGroup tipGroup;

    // Game over
    private GameObject gameOverPanel;
    private Text gameOverStatsText;

    // Arma
    private Text weaponText;

    // Jefe
    private GameObject bossBarPanel;
    private Image bossBarFill;
    private Text bossBarLabel;
    private Text bossBarHpText;
    private Enemy trackedBoss;

    // Elección de perk
    private GameObject perkPanel;
    private Action<PerkDefinition> perkCallback;

    // Menú principal
    private GameObject mainMenuPanel;
    private Text mainMenuScoresText;

    private PlayerHealth playerHealth;
    private PlayerShooting playerShooting;

    private void Awake()
    {
        Instance = this;
        BuildUI();
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerHealth = playerObj.GetComponent<PlayerHealth>();
        }

        if (playerHealth != null)
        {
            SetHealthImmediate(playerHealth.CurrentHealth, playerHealth.MaxHealth);
            playerHealth.OnHealthChanged += HandleHealthChanged;
            playerHealth.OnDamaged += HandleDamaged;
        }

        if (playerObj != null)
        {
            playerShooting = playerObj.GetComponent<PlayerShooting>();
        }

        if (playerShooting != null)
        {
            playerShooting.OnWeaponChanged += HandleWeaponChanged;
            HandleWeaponChanged(WeaponKind.Pistol, -1);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged += HandleScoreChanged;
            GameManager.Instance.OnGameOver += HandleGameOver;
        }

        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.OnDifficultyChanged += HandleDifficultyChanged;
        }

        ShowMainMenu();
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= HandleHealthChanged;
            playerHealth.OnDamaged -= HandleDamaged;
        }
        if (playerShooting != null)
        {
            playerShooting.OnWeaponChanged -= HandleWeaponChanged;
        }
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged -= HandleScoreChanged;
            GameManager.Instance.OnGameOver -= HandleGameOver;
        }
        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.OnDifficultyChanged -= HandleDifficultyChanged;
        }
    }

    private void Update()
    {
        UpdateBossBar();
        UpdateCrosshair();
        AnimateHealthBars();
        UpdateTimeText();
        PollDifficultyStats();

        if (gameOverPanel.activeSelf && Input.GetKeyDown(KeyCode.R))
        {
            GameManager.Instance?.RestartGame();
        }
    }

    // ---------- Construcción de la UI ----------

    private void BuildUI()
    {
        GameObject canvasObj = new GameObject("HUDCanvas", typeof(RectTransform));
        canvasObj.transform.SetParent(transform, false);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasObj.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();

        BuildVignette(canvasRect);
        BuildHealthPanel(canvasRect);
        BuildWeaponPanel(canvasRect);
        BuildScorePanel(canvasRect);
        BuildDifficultyPanel(canvasRect);
        BuildToast(canvasRect);
        BuildCrosshair(canvasRect);
        BuildTip(canvasRect);
        BuildDamageOverlay(canvasRect);
        BuildFloatingLayer(canvasRect);
        BuildGameOverPanel(canvasRect);
        BuildBossBar(canvasRect);
        BuildPerkPanel(canvasRect);
        BuildMainMenuPanel(canvasRect);
    }

    private static void Anchor(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
    }

    private void BuildVignette(Transform parent)
    {
        RectTransform rt = UIKit.CreateUIObject("Vignette", parent);
        Anchor(rt, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        vignetteImage = rt.gameObject.AddComponent<Image>();
        vignetteImage.sprite = ProceduralSprites.Vignette;
        vignetteImage.color = new Color(0f, 0f, 0.02f, 0.55f);
        vignetteImage.raycastTarget = false;
    }

    private void BuildHealthPanel(Transform parent)
    {
        RectTransform panel = UIKit.CreateUIObject("HealthPanel", parent);
        Anchor(panel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24, -24), new Vector2(420, 86));
        Image bg = panel.gameObject.AddComponent<Image>();
        bg.sprite = ProceduralSprites.RoundedRect();
        bg.type = Image.Type.Sliced;
        bg.color = new Color(0f, 0f, 0f, 0.45f);

        Text label = UIKit.CreateText("Label", panel, "VIDA", 16, new Color(1f, 1f, 1f, 0.7f), TextAnchor.UpperLeft);
        Anchor(label.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18, -8), new Vector2(200, 20));

        RectTransform barBg = UIKit.CreateUIObject("BarBg", panel);
        Anchor(barBg, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(18, 14), new Vector2(384, 32));
        Image barBgImg = barBg.gameObject.AddComponent<Image>();
        barBgImg.sprite = ProceduralSprites.RoundedRect();
        barBgImg.type = Image.Type.Sliced;
        barBgImg.color = new Color(0f, 0f, 0f, 0.5f);

        RectTransform chipRt = UIKit.CreateUIObject("Chip", panel);
        Anchor(chipRt, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(22, 18), new Vector2(376, 24));
        healthChipBar = chipRt.gameObject.AddComponent<Image>();
        healthChipBar.color = new Color(1f, 0.85f, 0.3f);
        healthChipBar.type = Image.Type.Filled;
        healthChipBar.fillMethod = Image.FillMethod.Horizontal;
        healthChipBar.fillAmount = 1f;

        RectTransform frontRt = UIKit.CreateUIObject("Front", panel);
        Anchor(frontRt, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(22, 18), new Vector2(376, 24));
        healthFrontBar = frontRt.gameObject.AddComponent<Image>();
        healthFrontBar.color = Color.green;
        healthFrontBar.type = Image.Type.Filled;
        healthFrontBar.fillMethod = Image.FillMethod.Horizontal;
        healthFrontBar.fillAmount = 1f;

        healthValueText = UIKit.CreateText("Value", panel, "100 / 100", 15, Color.white, TextAnchor.MiddleCenter);
        Anchor(healthValueText.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(22, 18), new Vector2(376, 24));
    }

    private void BuildWeaponPanel(Transform parent)
    {
        RectTransform panel = UIKit.CreateUIObject("WeaponPanel", parent);
        Anchor(panel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24, -122), new Vector2(340, 46));
        Image bg = panel.gameObject.AddComponent<Image>();
        bg.sprite = ProceduralSprites.RoundedRect();
        bg.type = Image.Type.Sliced;
        bg.color = new Color(0f, 0f, 0f, 0.4f);

        weaponText = UIKit.CreateText("Weapon", panel, "Pistola", 17, Color.white, TextAnchor.MiddleCenter);
        Anchor(weaponText.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
    }

    private void BuildScorePanel(Transform parent)
    {
        RectTransform panel = UIKit.CreateUIObject("ScorePanel", parent);
        Anchor(panel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -24), new Vector2(380, 110));
        Image bg = panel.gameObject.AddComponent<Image>();
        bg.sprite = ProceduralSprites.RoundedRect();
        bg.type = Image.Type.Sliced;
        bg.color = new Color(0f, 0f, 0f, 0.45f);

        scoreText = UIKit.CreateText("Score", panel, "0", 38, Color.white, TextAnchor.MiddleCenter);
        Anchor(scoreText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -12), new Vector2(360, 46));

        comboText = UIKit.CreateText("Combo", panel, "", 22, new Color(1f, 0.75f, 0.2f), TextAnchor.MiddleCenter);
        comboText.fontStyle = FontStyle.BoldAndItalic;
        Anchor(comboText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -60), new Vector2(360, 28));
        comboText.gameObject.SetActive(false);

        timeText = UIKit.CreateText("Time", panel, "00:00", 16, new Color(1f, 1f, 1f, 0.7f), TextAnchor.MiddleCenter);
        Anchor(timeText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 10), new Vector2(360, 20));
    }

    private void BuildDifficultyPanel(Transform parent)
    {
        RectTransform panel = UIKit.CreateUIObject("DifficultyPanel", parent);
        Anchor(panel, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24, -24), new Vector2(400, 230));
        Image bg = panel.gameObject.AddComponent<Image>();
        bg.sprite = ProceduralSprites.RoundedRect();
        bg.type = Image.Type.Sliced;
        bg.color = new Color(0f, 0f, 0f, 0.5f);

        Text title = UIKit.CreateText("Title", panel, "DIFICULTAD ADAPTATIVA · IA", 15, new Color(0.55f, 0.85f, 1f), TextAnchor.UpperLeft);
        Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16, -10), new Vector2(368, 20));

        difficultyLevelText = UIKit.CreateText("Level", panel, "NIVEL 1/10", 22, Color.white, TextAnchor.UpperLeft);
        Anchor(difficultyLevelText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16, -36), new Vector2(368, 28));

        RectTransform barBg = UIKit.CreateUIObject("LevelBarBg", panel);
        Anchor(barBg, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16, -70), new Vector2(368, 14));
        Image barBgImg = barBg.gameObject.AddComponent<Image>();
        barBgImg.color = new Color(1f, 1f, 1f, 0.15f);

        RectTransform barFillRt = UIKit.CreateUIObject("LevelBarFill", panel);
        Anchor(barFillRt, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16, -70), new Vector2(368, 14));
        difficultyLevelBar = barFillRt.gameObject.AddComponent<Image>();
        difficultyLevelBar.color = new Color(0.4f, 0.85f, 1f);
        difficultyLevelBar.type = Image.Type.Filled;
        difficultyLevelBar.fillMethod = Image.FillMethod.Horizontal;
        difficultyLevelBar.fillAmount = 0.1f;

        statsText = UIKit.CreateText("Stats", panel, "", 18, new Color(1f, 1f, 1f, 0.9f), TextAnchor.UpperLeft);
        statsText.fontStyle = FontStyle.Normal;
        statsText.lineSpacing = 1.3f;
        Anchor(statsText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16, -96), new Vector2(368, 130));
    }

    private void BuildToast(Transform parent)
    {
        toastRoot = UIKit.CreateUIObject("Toast", parent);
        Anchor(toastRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, ToastBaseY), new Vector2(520, 44));
        toastGroup = toastRoot.gameObject.AddComponent<CanvasGroup>();
        toastGroup.alpha = 0f;
        toastGroup.blocksRaycasts = false;

        Image toastBg = toastRoot.gameObject.AddComponent<Image>();
        toastBg.sprite = ProceduralSprites.RoundedRect();
        toastBg.type = Image.Type.Sliced;
        toastBg.color = new Color(0f, 0f, 0f, 0.55f);

        toastText = UIKit.CreateText("Text", toastRoot, "", 22, Color.white, TextAnchor.MiddleCenter);
        Anchor(toastText.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
    }

    private void BuildCrosshair(Transform parent)
    {
        crosshair = UIKit.CreateUIObject("Crosshair", parent);
        Anchor(crosshair, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(30, 30));
        Image img = crosshair.gameObject.AddComponent<Image>();
        img.sprite = ProceduralSprites.Ring;
        img.color = new Color(1f, 1f, 1f, 0.85f);
        img.raycastTarget = false;
    }

    private void BuildTip(Transform parent)
    {
        RectTransform tipRt = UIKit.CreateUIObject("Tip", parent);
        Anchor(tipRt, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 30), new Vector2(900, 30));
        tipGroup = tipRt.gameObject.AddComponent<CanvasGroup>();

        Text tip = UIKit.CreateText("Text", tipRt, "WASD: moverte    Click: disparar    1-4: cambiar de arma    R: reiniciar al morir", 18, new Color(1f, 1f, 1f, 0.85f), TextAnchor.MiddleCenter);
        Anchor(tip.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
    }

    private void BuildDamageOverlay(Transform parent)
    {
        RectTransform rt = UIKit.CreateUIObject("DamageFlash", parent);
        Anchor(rt, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        damageFlashImage = rt.gameObject.AddComponent<Image>();
        damageFlashImage.color = new Color(0.85f, 0.05f, 0.05f, 0f);
        damageFlashImage.raycastTarget = false;
    }

    private void BuildFloatingLayer(Transform parent)
    {
        floatingLayer = UIKit.CreateUIObject("FloatingLayer", parent);
        Anchor(floatingLayer, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
    }

    private void BuildGameOverPanel(Transform parent)
    {
        RectTransform root = UIKit.CreateUIObject("GameOverPanel", parent);
        Anchor(root, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        Image bg = root.gameObject.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.8f);
        gameOverPanel = root.gameObject;

        Text title = UIKit.CreateText("Title", root, "GAME OVER", 70, new Color(1f, 0.3f, 0.3f), TextAnchor.MiddleCenter);
        Anchor(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 100), new Vector2(900, 100));

        gameOverStatsText = UIKit.CreateText("Stats", root, "", 26, Color.white, TextAnchor.MiddleCenter);
        gameOverStatsText.fontStyle = FontStyle.Normal;
        gameOverStatsText.lineSpacing = 1.3f;
        Anchor(gameOverStatsText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(700, 140));

        Text restartHint = UIKit.CreateText("RestartHint", root, "Presiona R para reintentar", 24, new Color(1f, 1f, 1f, 0.9f), TextAnchor.MiddleCenter);
        Anchor(restartHint.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -110), new Vector2(700, 50));
        restartHint.gameObject.AddComponent<BlinkText>();

        gameOverPanel.SetActive(false);
    }

    private void BuildBossBar(Transform parent)
    {
        RectTransform panel = UIKit.CreateUIObject("BossBar", parent);
        Anchor(panel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -150), new Vector2(700, 50));
        Image bg = panel.gameObject.AddComponent<Image>();
        bg.sprite = ProceduralSprites.RoundedRect();
        bg.type = Image.Type.Sliced;
        bg.color = new Color(0f, 0f, 0f, 0.55f);

        RectTransform barBg = UIKit.CreateUIObject("FillBg", panel);
        Anchor(barBg, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(6, 6), new Vector2(688, 22));
        Image barBgImg = barBg.gameObject.AddComponent<Image>();
        barBgImg.sprite = ProceduralSprites.Square;
        barBgImg.color = new Color(0f, 0f, 0f, 0.6f);

        RectTransform fillRt = UIKit.CreateUIObject("Fill", panel);
        Anchor(fillRt, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(6, 6), new Vector2(688, 22));
        bossBarFill = fillRt.gameObject.AddComponent<Image>();
        bossBarFill.sprite = ProceduralSprites.Square;
        bossBarFill.color = new Color(0.8f, 0.1f, 0.15f);
        bossBarFill.type = Image.Type.Filled;
        bossBarFill.fillMethod = Image.FillMethod.Horizontal;
        bossBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        bossBarFill.fillAmount = 1f;

        bossBarLabel = UIKit.CreateText("Label", panel, "JEFE", 18, Color.white, TextAnchor.UpperCenter);
        Anchor(bossBarLabel.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0, 8), new Vector2(0, 20));

        bossBarHpText = UIKit.CreateText("HpText", panel, "", 14, new Color(1f, 1f, 1f, 0.85f), TextAnchor.MiddleCenter);
        bossBarHpText.fontStyle = FontStyle.Normal;
        Anchor(bossBarHpText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 6), new Vector2(0, 22));

        bossBarPanel = panel.gameObject;
        bossBarPanel.SetActive(false);
    }

    private void BuildPerkPanel(Transform parent)
    {
        RectTransform root = UIKit.CreateUIObject("PerkPanel", parent);
        Anchor(root, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        Image bg = root.gameObject.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.85f);
        perkPanel = root.gameObject;

        Text title = UIKit.CreateText("Title", root, "¡SUBISTE DE NIVEL! Elegí una mejora", 30, new Color(0.55f, 0.85f, 1f), TextAnchor.MiddleCenter);
        Anchor(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 180), new Vector2(1100, 60));

        perkPanel.SetActive(false);
    }

    private void BuildMainMenuPanel(Transform parent)
    {
        RectTransform root = UIKit.CreateUIObject("MainMenu", parent);
        Anchor(root, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        Image bg = root.gameObject.AddComponent<Image>();
        bg.color = new Color(0.02f, 0.03f, 0.07f, 0.94f);
        mainMenuPanel = root.gameObject;

        Text title = UIKit.CreateText("Title", root, "DIFICULTAD ADAPTATIVA", 52, new Color(0.55f, 0.85f, 1f), TextAnchor.MiddleCenter);
        Anchor(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 220), new Vector2(1100, 80));

        Text subtitle = UIKit.CreateText("Subtitle", root, "Un shooter top-down donde una IA ajusta la dificultad en vivo según cómo juegues", 19, new Color(1f, 1f, 1f, 0.75f), TextAnchor.MiddleCenter);
        subtitle.fontStyle = FontStyle.Normal;
        Anchor(subtitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 165), new Vector2(760, 40));

        mainMenuScoresText = UIKit.CreateText("Scores", root, "", 18, new Color(1f, 0.85f, 0.4f), TextAnchor.MiddleCenter);
        mainMenuScoresText.fontStyle = FontStyle.Normal;
        mainMenuScoresText.lineSpacing = 1.35f;
        Anchor(mainMenuScoresText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 55), new Vector2(700, 100));

        Button playButton = UIKit.CreateButton("PlayButton", root, "JUGAR", new Color(0.25f, 0.65f, 0.35f), new Vector2(260, 70), HandlePlayClicked);
        Anchor(playButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -60), new Vector2(260, 70));

        Text tip = UIKit.CreateText("Tip", root, "WASD: moverte  ·  Click: disparar  ·  1-4: cambiar de arma  ·  R: reiniciar al morir", 16, new Color(1f, 1f, 1f, 0.6f), TextAnchor.MiddleCenter);
        Anchor(tip.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 50), new Vector2(900, 30));

        mainMenuPanel.SetActive(false);
    }

    // ---------- Lógica de actualización ----------

    private void UpdateCrosshair()
    {
        if (crosshair == null) return;
        crosshair.position = Input.mousePosition;
    }

    private void AnimateHealthBars()
    {
        chipHealthFraction = Mathf.MoveTowards(chipHealthFraction, displayedHealthFraction, Time.unscaledDeltaTime * 0.6f);
        healthChipBar.fillAmount = chipHealthFraction;

        Color barColor;
        if (displayedHealthFraction > 0.6f)
            barColor = Color.Lerp(Color.yellow, Color.green, Mathf.InverseLerp(0.6f, 1f, displayedHealthFraction));
        else if (displayedHealthFraction > 0.3f)
            barColor = Color.Lerp(new Color(1f, 0.55f, 0f), Color.yellow, Mathf.InverseLerp(0.3f, 0.6f, displayedHealthFraction));
        else
            barColor = Color.Lerp(Color.red, new Color(1f, 0.55f, 0f), Mathf.InverseLerp(0f, 0.3f, displayedHealthFraction));
        healthFrontBar.color = barColor;

        if (damageFlashTimer > 0f) damageFlashTimer -= Time.unscaledDeltaTime;
        float flashAlpha = Mathf.Lerp(0f, 0.45f, Mathf.Clamp01(damageFlashTimer / DamageFlashDuration));

        float lowHealthAlpha = 0f;
        if (displayedHealthFraction <= 0.25f && displayedHealthFraction > 0f)
        {
            float pulse = (Mathf.Sin(Time.unscaledTime * 6f) + 1f) * 0.5f;
            lowHealthAlpha = Mathf.Lerp(0.05f, 0.22f, pulse);
        }

        damageFlashImage.color = new Color(0.85f, 0.05f, 0.05f, Mathf.Max(flashAlpha, lowHealthAlpha));
    }

    private void UpdateTimeText()
    {
        if (GameManager.Instance == null) return;
        timeText.text = FormatTime(GameManager.Instance.SurvivalTime);
    }

    private void PollDifficultyStats()
    {
        statsPollTimer -= Time.unscaledDeltaTime;
        if (statsPollTimer > 0f) return;
        statsPollTimer = StatsPollInterval;

        DifficultyManager dm = DifficultyManager.Instance;
        if (dm == null) return;

        int activeEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;

        difficultyLevelText.text = $"NIVEL {dm.DifficultyLevel}/10";
        difficultyLevelBar.fillAmount = Mathf.InverseLerp(0, 10, dm.DifficultyLevel);

        statsText.text =
            $"Velocidad enemigos: {dm.EnemySpeed:F2}\n" +
            $"Vida enemigos: {dm.EnemyHealth}\n" +
            $"Aparición cada: {dm.SpawnInterval:F2}s\n" +
            $"Precisión: {dm.Accuracy:P0}\n" +
            $"Enemigos activos: {activeEnemies}";

        // Tinte de peligro: se va poniendo más rojo e intenso a medida que sube el nivel de dificultad.
        float dangerT = Mathf.InverseLerp(1, 10, dm.DifficultyLevel);
        Color calmVignette = new Color(0f, 0f, 0.02f, 0.5f);
        Color dangerVignette = new Color(0.4f, 0.03f, 0.03f, 0.68f);
        vignetteImage.color = Color.Lerp(calmVignette, dangerVignette, dangerT);
    }

    private string FormatTime(float t)
    {
        int minutes = Mathf.FloorToInt(t / 60f);
        int seconds = Mathf.FloorToInt(t % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    private void UpdateBossBar()
    {
        if (trackedBoss == null)
        {
            if (bossBarPanel.activeSelf) bossBarPanel.SetActive(false);
            return;
        }

        int current = Mathf.Max(0, trackedBoss.CurrentHealth);
        int max = Mathf.Max(1, trackedBoss.MaxHealth);
        bossBarFill.fillAmount = (float)current / max;
        bossBarHpText.text = $"{current} / {max}";
    }

    // ---------- Manejadores de eventos ----------

    private void SetHealthImmediate(int current, int max)
    {
        HandleHealthChanged(current, max);
        chipHealthFraction = displayedHealthFraction;
        healthChipBar.fillAmount = chipHealthFraction;
    }

    private void HandleHealthChanged(int current, int max)
    {
        displayedHealthFraction = max > 0 ? (float)current / max : 0f;
        healthFrontBar.fillAmount = displayedHealthFraction;
        healthValueText.text = $"{current} / {max}";
    }

    private void HandleDamaged(int amount)
    {
        damageFlashTimer = DamageFlashDuration;
    }

    private void HandleWeaponChanged(WeaponKind kind, int ammoLeft)
    {
        WeaponStats stats = WeaponDatabase.All[kind];
        string ammoPart = ammoLeft < 0 ? "∞" : ammoLeft.ToString();
        weaponText.text = $"{stats.DisplayName} · {stats.Damage} dmg  ({ammoPart})";
        weaponText.color = stats.Color;
    }

    private void HandleScoreChanged(int score, int combo)
    {
        scoreText.text = score.ToString("N0");

        if (combo > 1)
        {
            comboText.gameObject.SetActive(true);
            comboText.text = $"COMBO x{combo}!";
            if (comboPunchRoutine != null) StopCoroutine(comboPunchRoutine);
            comboPunchRoutine = StartCoroutine(PunchRoutine(comboText.rectTransform));
        }
        else
        {
            comboText.gameObject.SetActive(false);
        }
    }

    private void HandleDifficultyChanged(bool increased)
    {
        string msg = increased ? "▲ DIFICULTAD AUMENTADA" : "▼ DIFICULTAD REDUCIDA";
        Color c = increased ? new Color(1f, 0.4f, 0.35f) : new Color(0.4f, 0.85f, 1f);
        ShowToast(msg, c);
        SoundManager.Play(increased ? Sfx.DifficultyUp : Sfx.DifficultyDown);
    }

    private void HandleGameOver(bool isNewRecord)
    {
        DifficultyManager dm = DifficultyManager.Instance;
        int level = dm != null ? dm.DifficultyLevel : 1;
        string recordLine = isNewRecord ? "\n¡NUEVO RÉCORD!" : "";

        gameOverStatsText.text =
            $"Puntaje final: {GameManager.Instance.Score:N0}\n" +
            $"Sobreviviste: {FormatTime(GameManager.Instance.SurvivalTime)}\n" +
            $"Nivel de dificultad alcanzado: {level}/10{recordLine}";

        gameOverPanel.SetActive(true);
        Cursor.visible = true;
        Time.timeScale = 0f;
    }

    // ---------- Corrutinas ----------

    private IEnumerator FadeOutTip()
    {
        yield return new WaitForSecondsRealtime(5f);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime;
            tipGroup.alpha = 1f - t;
            yield return null;
        }
        tipGroup.alpha = 0f;
    }

    private IEnumerator PunchRoutine(RectTransform rt)
    {
        float t = 0f;
        const float duration = 0.25f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float s = Mathf.Lerp(1.4f, 1f, t / duration);
            rt.localScale = Vector3.one * s;
            yield return null;
        }
        rt.localScale = Vector3.one;
    }

    private void ShowToast(string message, Color color)
    {
        if (toastRoutine != null) StopCoroutine(toastRoutine);
        toastText.text = message;
        toastText.color = color;
        toastRoutine = StartCoroutine(ToastRoutine());
    }

    private IEnumerator ToastRoutine()
    {
        float t = 0f;
        while (t < 0.25f)
        {
            t += Time.unscaledDeltaTime;
            float p = t / 0.25f;
            toastGroup.alpha = p;
            toastRoot.anchoredPosition = new Vector2(0, Mathf.Lerp(ToastBaseY - 20f, ToastBaseY, p));
            yield return null;
        }
        toastGroup.alpha = 1f;
        toastRoot.anchoredPosition = new Vector2(0, ToastBaseY);

        yield return new WaitForSecondsRealtime(1.8f);

        t = 0f;
        while (t < 0.4f)
        {
            t += Time.unscaledDeltaTime;
            toastGroup.alpha = 1f - (t / 0.4f);
            yield return null;
        }
        toastGroup.alpha = 0f;
    }

    // ---------- API pública ----------

    public void ShowFloatingText(Vector3 worldPos, string content, Color color, float fontSize = 28f)
    {
        Camera cam = Camera.main;
        if (cam == null || floatingLayer == null) return;

        Vector2 screenPoint = cam.WorldToScreenPoint(worldPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(floatingLayer, screenPoint, null, out Vector2 localPoint);

        Text txt = UIKit.CreateText("FloatingText", floatingLayer, content, (int)fontSize, color, TextAnchor.MiddleCenter);
        RectTransform rt = txt.rectTransform;
        rt.anchoredPosition = localPoint;
        rt.sizeDelta = new Vector2(220, 60);
        txt.gameObject.AddComponent<FloatingUIText>().Init(rt, txt);
    }

    public void ShowBossBar(Enemy enemy, string displayName)
    {
        trackedBoss = enemy;
        bossBarLabel.text = displayName;
        bossBarPanel.SetActive(true);
    }

    public void ShowBossWarning()
    {
        ShowToast("⚠ UN JEFE SE ACERCA...", new Color(1f, 0.3f, 0.2f));
    }

    public void ShowPerkChoice(List<PerkDefinition> choices, Action<PerkDefinition> onChosen)
    {
        perkCallback = onChosen;
        Time.timeScale = 0f;
        Cursor.visible = true;

        List<Transform> oldCards = new List<Transform>();
        foreach (Transform child in perkPanel.transform)
        {
            if (child.name.StartsWith("Choice")) oldCards.Add(child);
        }
        foreach (Transform card in oldCards) Destroy(card.gameObject);

        const float spacing = 340f;
        float startX = -(choices.Count - 1) * spacing / 2f;

        for (int i = 0; i < choices.Count; i++)
        {
            PerkDefinition perk = choices[i];

            RectTransform card = UIKit.CreateUIObject($"Choice{i}", perkPanel.transform);
            Anchor(card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(startX + i * spacing, 0), new Vector2(300, 220));
            Image cardBg = card.gameObject.AddComponent<Image>();
            cardBg.sprite = ProceduralSprites.RoundedRect();
            cardBg.type = Image.Type.Sliced;
            cardBg.color = new Color(perk.Color.r * 0.25f, perk.Color.g * 0.25f, perk.Color.b * 0.25f, 0.95f);

            Text name = UIKit.CreateText("Name", card, perk.Name, 23, perk.Color, TextAnchor.MiddleCenter);
            Anchor(name.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -22), new Vector2(0, 60));

            Text desc = UIKit.CreateText("Desc", card, perk.Description, 17, Color.white, TextAnchor.MiddleCenter);
            desc.fontStyle = FontStyle.Normal;
            Anchor(desc.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0, -20), new Vector2(-30, -110));

            Button btn = card.gameObject.AddComponent<Button>();
            btn.targetGraphic = cardBg;
            PerkDefinition capturedPerk = perk;
            btn.onClick.AddListener(() => HandlePerkChosen(capturedPerk));
        }

        perkPanel.SetActive(true);
    }

    private void HandlePerkChosen(PerkDefinition perk)
    {
        perkPanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.visible = false;
        SoundManager.Play(Sfx.UIClick);
        perkCallback?.Invoke(perk);
    }

    private void ShowMainMenu()
    {
        Time.timeScale = 0f;
        Cursor.visible = true;

        mainMenuScoresText.text =
            $"Mejor puntaje: {HighScoreManager.BestScore:N0}    Mejor racha: x{HighScoreManager.BestCombo}\n" +
            $"Nivel máximo alcanzado: {HighScoreManager.BestLevel}/10    Mejor tiempo: {FormatTime(HighScoreManager.BestSurvivalTime)}";

        mainMenuPanel.SetActive(true);
    }

    private void HandlePlayClicked()
    {
        mainMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.visible = false;
        StartCoroutine(FadeOutTip());
    }
}
