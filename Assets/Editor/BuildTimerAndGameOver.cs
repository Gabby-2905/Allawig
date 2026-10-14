using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Builds the timer HUD element, game over panel, wires GameTimer + GameOverManager,
/// and resizes the map to a more appropriate player scale.
/// </summary>
public class BuildTimerAndGameOver : MonoBehaviour
{
    [MenuItem("Tools/Build Timer + Game Over System")]
    public static void Execute()
    {
        // ── Find scene objects ─────────────────────────────────────────────
        GameObject hudCanvas = GameObject.Find("HUDCanvas");
        GameObject pauseCanvas = GameObject.Find("PauseMenuCanvas");
        if (hudCanvas == null)  { Debug.LogError("HUDCanvas not found!"); return; }
        if (pauseCanvas == null){ Debug.LogError("PauseMenuCanvas not found!"); return; }

        // ── 1. Build Timer UI on HUDCanvas ─────────────────────────────────
        GameObject timerGO = BuildTimerUI(hudCanvas);

        // ── 2. Build Game Over Panel on PauseMenuCanvas ────────────────────
        GameObject gameOverPanel = BuildGameOverPanel(pauseCanvas);

        // ── 3. Add GameTimer component to HUDCanvas ────────────────────────
        GameTimer gameTimer = hudCanvas.GetComponent<GameTimer>();
        if (gameTimer == null) gameTimer = hudCanvas.AddComponent<GameTimer>();
        gameTimer.totalTime       = 90f;
        gameTimer.warningThreshold = 20f;
        gameTimer.timerText       = timerGO.GetComponentInChildren<TextMeshProUGUI>();
        gameTimer.normalColor     = Color.white;
        gameTimer.warningColor    = new Color(1f, 0.25f, 0.15f, 1f);

        // ── 4. Add GameOverManager to PauseMenuCanvas ──────────────────────
        GameOverManager gom = pauseCanvas.GetComponent<GameOverManager>();
        if (gom == null) gom = pauseCanvas.AddComponent<GameOverManager>();
        gom.gameOverPanel = gameOverPanel;
        gom.gameTimer     = gameTimer;

        // Wire reason/subtitle text
        Transform reasonT   = gameOverPanel.transform.Find("ReasonText");
        Transform subtitleT = gameOverPanel.transform.Find("SubtitleText");
        if (reasonT   != null) gom.reasonText   = reasonT.GetComponent<TextMeshProUGUI>();
        if (subtitleT != null) gom.subtitleText  = subtitleT.GetComponent<TextMeshProUGUI>();

        // Wire PauseMenuManager
        PauseMenuManager pmm = GameObject.Find("PauseMenuManager")?.GetComponent<PauseMenuManager>();
        if (pmm != null)
        {
            pmm.gameTimer = gameTimer;
            gom.pauseMenuManager = pmm;
            EditorUtility.SetDirty(pmm.gameObject);
        }

        // Wire retry/menu buttons
        WireGameOverButtons(gameOverPanel, gom);

        // ── 5. Resize map to appropriate scale ─────────────────────────────
        ResizeMap();

        // ── 6. Save ────────────────────────────────────────────────────────
        EditorUtility.SetDirty(hudCanvas);
        EditorUtility.SetDirty(pauseCanvas);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("✅ Timer + Game Over system built and wired.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Timer UI — top-center of screen
    // ─────────────────────────────────────────────────────────────────────────
    static GameObject BuildTimerUI(GameObject hudCanvas)
    {
        // Remove existing
        Transform existing = hudCanvas.transform.Find("TimerDisplay");
        if (existing != null) Object.DestroyImmediate(existing.gameObject);

        // Container
        GameObject timerGO = new GameObject("TimerDisplay");
        timerGO.transform.SetParent(hudCanvas.transform, false);

        RectTransform rt = timerGO.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot     = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -20f);
        rt.sizeDelta = new Vector2(280f, 90f);

        // Dark background pill
        Image bg = timerGO.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.05f, 0.08f, 0.82f);

        // Clock icon label
        GameObject iconGO = new GameObject("ClockIcon");
        iconGO.transform.SetParent(timerGO.transform, false);
        RectTransform iconRT = iconGO.AddComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0f, 0.5f);
        iconRT.anchorMax = new Vector2(0f, 0.5f);
        iconRT.pivot     = new Vector2(0f, 0.5f);
        iconRT.anchoredPosition = new Vector2(18f, 0f);
        iconRT.sizeDelta = new Vector2(36f, 36f);
        TextMeshProUGUI iconTMP = iconGO.AddComponent<TextMeshProUGUI>();
        iconTMP.text      = "⏱";
        iconTMP.fontSize  = 26f;
        iconTMP.alignment = TextAlignmentOptions.Center;
        iconTMP.color     = new Color(0.9f, 0.85f, 0.3f, 1f);

        // Timer text
        GameObject textGO = new GameObject("TimerText");
        textGO.transform.SetParent(timerGO.transform, false);
        RectTransform textRT = textGO.AddComponent<RectTransform>();
        textRT.anchorMin = new Vector2(0f, 0f);
        textRT.anchorMax = new Vector2(1f, 1f);
        textRT.offsetMin = new Vector2(60f, 0f);
        textRT.offsetMax = new Vector2(-10f, 0f);
        TextMeshProUGUI timerTMP = textGO.AddComponent<TextMeshProUGUI>();
        timerTMP.text      = "1:30";
        timerTMP.fontSize  = 46f;
        timerTMP.fontStyle = FontStyles.Bold;
        timerTMP.alignment = TextAlignmentOptions.Center;
        timerTMP.color     = Color.white;

        // Thin bottom border accent
        GameObject borderGO = new GameObject("Accent");
        borderGO.transform.SetParent(timerGO.transform, false);
        RectTransform borderRT = borderGO.AddComponent<RectTransform>();
        borderRT.anchorMin = new Vector2(0f, 0f);
        borderRT.anchorMax = new Vector2(1f, 0f);
        borderRT.pivot     = new Vector2(0.5f, 0f);
        borderRT.anchoredPosition = Vector2.zero;
        borderRT.sizeDelta = new Vector2(0f, 3f);
        Image borderImg = borderGO.AddComponent<Image>();
        borderImg.color = new Color(0.9f, 0.85f, 0.3f, 0.8f);

        Debug.Log("Timer UI built.");
        return timerGO;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Game Over Panel
    // ─────────────────────────────────────────────────────────────────────────
    static GameObject BuildGameOverPanel(GameObject pauseCanvas)
    {
        Transform existing = pauseCanvas.transform.Find("GameOverPanel");
        if (existing != null) Object.DestroyImmediate(existing.gameObject);

        // Full-screen dark overlay
        GameObject panel = new GameObject("GameOverPanel");
        panel.transform.SetParent(pauseCanvas.transform, false);

        RectTransform panelRT = panel.AddComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        Image panelBg = panel.AddComponent<Image>();
        panelBg.color = new Color(0f, 0f, 0f, 0.88f);

        // ── Card container ─────────────────────────────────────────────────
        GameObject card = new GameObject("Card");
        card.transform.SetParent(panel.transform, false);
        RectTransform cardRT = card.AddComponent<RectTransform>();
        cardRT.anchorMin = new Vector2(0.5f, 0.5f);
        cardRT.anchorMax = new Vector2(0.5f, 0.5f);
        cardRT.pivot     = new Vector2(0.5f, 0.5f);
        cardRT.sizeDelta = new Vector2(600f, 500f);
        cardRT.anchoredPosition = Vector2.zero;
        Image cardBg = card.AddComponent<Image>();
        cardBg.color = new Color(0.06f, 0.06f, 0.1f, 0.97f);
        var outline = card.AddComponent<Outline>();
        outline.effectColor    = new Color(0.85f, 0.2f, 0.1f, 1f);
        outline.effectDistance = new Vector2(2f, -2f);

        // ── Skull / icon ───────────────────────────────────────────────────
        MakeTMP(card, "SkullIcon", "💀", new Vector2(0f, 170f), new Vector2(120f, 120f), 72f, FontStyles.Normal,
            new Color(0.85f, 0.2f, 0.1f, 1f));

        // ── Reason text ────────────────────────────────────────────────────
        MakeTMP(card, "ReasonText", "TIME'S UP!", new Vector2(0f, 80f), new Vector2(540f, 80f), 52f, FontStyles.Bold,
            new Color(0.95f, 0.25f, 0.1f, 1f));

        // ── Subtitle ───────────────────────────────────────────────────────
        MakeTMP(card, "SubtitleText", "You ran out of time.", new Vector2(0f, 20f), new Vector2(500f, 50f), 22f, FontStyles.Normal,
            new Color(0.75f, 0.75f, 0.75f, 1f));

        // ── Divider ────────────────────────────────────────────────────────
        MakeDivider(card, new Vector2(0f, -30f), new Vector2(500f, 2f), new Color(0.85f, 0.2f, 0.1f, 0.5f));

        // ── Retry button ───────────────────────────────────────────────────
        MakeButton(card, "RetryBtn",    "▶  RETRY",     new Vector2(-110f, -120f), new Vector2(220f, 60f),
            new Color(0.85f, 0.2f, 0.1f, 1f), new Color(0.06f, 0.06f, 0.1f, 1f));

        // ── Main Menu button ───────────────────────────────────────────────
        MakeButton(card, "MenuBtn",     "⌂  MAIN MENU", new Vector2(110f, -120f), new Vector2(220f, 60f),
            new Color(0.15f, 0.15f, 0.22f, 1f), new Color(0.7f, 0.7f, 0.7f, 1f));

        // ── Quit button ────────────────────────────────────────────────────
        MakeButton(card, "QuitBtn",     "✕  QUIT",      new Vector2(0f, -200f), new Vector2(160f, 48f),
            new Color(0.1f, 0.1f, 0.14f, 1f), new Color(0.5f, 0.5f, 0.5f, 1f));

        panel.SetActive(false);
        Debug.Log("Game Over panel built.");
        return panel;
    }

    static void WireGameOverButtons(GameObject panel, GameOverManager gom)
    {
        Transform card = panel.transform.Find("Card");
        if (card == null) return;

        Button retryBtn = card.Find("RetryBtn")?.GetComponent<Button>();
        Button menuBtn  = card.Find("MenuBtn")?.GetComponent<Button>();
        Button quitBtn  = card.Find("QuitBtn")?.GetComponent<Button>();

        if (retryBtn != null) retryBtn.onClick.AddListener(gom.RetryLevel);
        if (menuBtn  != null) menuBtn.onClick.AddListener(gom.GoToMainMenu);
        if (quitBtn  != null) quitBtn.onClick.AddListener(gom.QuitGame);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Resize Map
    // ─────────────────────────────────────────────────────────────────────────
    static void ResizeMap()
    {
        GameObject mapGO = GameObject.Find("1stMap");
        if (mapGO == null) { Debug.LogWarning("1stMap not found for resize."); return; }

        // Scale 60 = ~59x56 units — good arena size for a single player
        float newScale = 60f;
        mapGO.transform.localScale = Vector3.one * newScale;

        // Reposition floor to Y=0
        Bounds b = GetBounds(mapGO);
        mapGO.transform.position = new Vector3(-b.center.x, -b.min.y, -b.center.z);
        b = GetBounds(mapGO);

        Debug.Log($"Map resized to scale {newScale}. Area: {b.size.x:F0}x{b.size.z:F0} units.");

        // Reposition player on surface
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            float fromY = b.max.y + 5f;
            float roadOffset = b.size.x * 0.25f;
            RaycastHit hit;
            float spawnY = b.min.y + 0.3f;
            float spawnX = roadOffset;
            float spawnZ = 0f;

            if (Physics.Raycast(new Vector3(spawnX, fromY, spawnZ), Vector3.down, out hit, fromY + 50f))
            {
                spawnY = hit.point.y + 0.2f;
                spawnX = hit.point.x;
                spawnZ = hit.point.z;
            }

            player.transform.position = new Vector3(spawnX, spawnY, spawnZ);

            // Adjust speed for smaller map
            GamePlayerMovement gpm = player.GetComponent<GamePlayerMovement>();
            if (gpm != null) { gpm.walkSpeed = 4f; gpm.runSpeed = 9f; }
            EditorUtility.SetDirty(player);
            Debug.Log($"Player placed at ({spawnX:F1}, {spawnY:F2}, {spawnZ:F1})");
        }

        // Camera offset for smaller map
        GameObject camGO = GameObject.Find("Main Camera");
        if (camGO != null)
        {
            GameCameraFollow gcf = camGO.GetComponent<GameCameraFollow>();
            if (gcf != null) { gcf.offset = new Vector3(0f, 13f, -9f); EditorUtility.SetDirty(camGO); }
        }

        // Rain above map
        GameObject rain = GameObject.Find("Rain");
        if (rain != null)
        {
            rain.transform.position = new Vector3(0f, b.max.y + 15f, 0f);
            EditorUtility.SetDirty(rain);
        }

        // Containers spread
        float sy = b.min.y + 0.5f;
        float sp = Mathf.Min(b.size.x, b.size.z) * 0.28f;
        Vector3[] pos = {
            new Vector3( sp*.4f, sy, sp*.4f),  new Vector3(-sp*.4f, sy, sp*.4f),
            new Vector3( sp*.4f, sy,-sp*.4f),  new Vector3(-sp*.4f, sy,-sp*.4f),
            new Vector3( sp*.7f, sy, 0),       new Vector3(-sp*.7f, sy, 0),
            new Vector3( 0, sy, sp*.7f),       new Vector3( 0, sy,-sp*.7f),
            new Vector3( sp*.6f, sy, sp*.6f),  new Vector3(-sp*.6f, sy,-sp*.6f),
            new Vector3( sp*.8f, sy, sp*.3f),  new Vector3(-sp*.3f, sy, sp*.8f),
            new Vector3( sp*.5f, sy,-sp*.7f),  new Vector3(-sp*.7f, sy, sp*.5f),
        };
        int idx = 0;
        foreach (var go in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (!go.name.StartsWith("Container") || idx >= pos.Length) continue;
            float fromY2 = b.max.y + 5f;
            RaycastHit ch;
            float cy = Physics.Raycast(new Vector3(pos[idx].x, fromY2, pos[idx].z), Vector3.down, out ch, fromY2 + 50f)
                ? ch.point.y + 0.3f : sy;
            go.transform.position = new Vector3(pos[idx].x, cy, pos[idx].z);
            EditorUtility.SetDirty(go);
            idx++;
        }
        Debug.Log($"Repositioned {idx} containers.");
        EditorUtility.SetDirty(mapGO);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // UI Helpers
    // ─────────────────────────────────────────────────────────────────────────
    static void MakeTMP(GameObject parent, string name, string text, Vector2 pos, Vector2 size,
        float fontSize, FontStyles style, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.fontStyle = style;
        tmp.color     = color;
        tmp.alignment = TextAlignmentOptions.Center;
    }

    static void MakeDivider(GameObject parent, Vector2 pos, Vector2 size, Color color)
    {
        GameObject go = new GameObject("Divider");
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        go.AddComponent<Image>().color = color;
    }

    static void MakeButton(GameObject parent, string name, string label, Vector2 pos, Vector2 size,
        Color bgColor, Color textColor)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        Image img = go.AddComponent<Image>();
        img.color = bgColor;
        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(go.transform, false);
        RectTransform lrt = labelGO.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero;
        lrt.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = labelGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 20f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color     = textColor;
        tmp.alignment = TextAlignmentOptions.Center;
    }

    static Bounds GetBounds(GameObject go)
    {
        var rs = go.GetComponentsInChildren<Renderer>();
        if (rs.Length == 0) return new Bounds(go.transform.position, Vector3.one * 10f);
        Bounds b = rs[0].bounds;
        foreach (var r in rs) b.Encapsulate(r.bounds);
        return b;
    }
}
