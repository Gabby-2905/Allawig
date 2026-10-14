using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor script that:
/// 1. Wires the GameTimer on HUDCanvas to the TimerText UI element
/// 2. Adds a GameOverManager to the scene and connects it to the GameOverPanel + buttons
/// 3. Scales down the 1stMap to a more player-friendly size
/// </summary>
public class WireTimerAndFixMap
{
    [MenuItem("Tools/Wire Timer + Fix Map")]
    public static void Execute()
    {
        // ── 1. GameTimer wiring ────────────────────────────────────────────
        GameObject hudCanvas = GameObject.Find("HUDCanvas");
        if (hudCanvas == null) { Debug.LogError("HUDCanvas not found!"); return; }

        GameTimer gameTimer = hudCanvas.GetComponent<GameTimer>();
        if (gameTimer == null) { Debug.LogError("GameTimer not found on HUDCanvas!"); return; }

        // Set total time to 90 seconds (1:30)
        gameTimer.totalTime = 90f;

        // Wire TimerText
        Transform timerTextTf = hudCanvas.transform.Find("TimerDisplay/TimerText");
        if (timerTextTf != null)
            gameTimer.timerText = timerTextTf.GetComponent<TextMeshProUGUI>();

        // ── 2. GameOverManager ─────────────────────────────────────────────
        // Find or create a GameOverManager GameObject
        GameObject gomGO = GameObject.Find("GameOverManager");
        if (gomGO == null)
        {
            gomGO = new GameObject("GameOverManager");
            Undo.RegisterCreatedObjectUndo(gomGO, "Create GameOverManager");
        }

        GameOverManager gom = gomGO.GetComponent<GameOverManager>();
        if (gom == null)
            gom = Undo.AddComponent<GameOverManager>(gomGO);

        // Wire GameOverPanel
        GameObject pauseCanvas = GameObject.Find("PauseMenuCanvas");
        if (pauseCanvas == null) { Debug.LogError("PauseMenuCanvas not found!"); return; }

        Transform gameOverPanelTf = pauseCanvas.transform.Find("GameOverPanel");
        if (gameOverPanelTf != null)
            gom.gameOverPanel = gameOverPanelTf.gameObject;

        // Wire ReasonText and SubtitleText
        if (gameOverPanelTf != null)
        {
            Transform reasonTf   = gameOverPanelTf.Find("Card/ReasonText");
            Transform subtitleTf = gameOverPanelTf.Find("Card/SubtitleText");
            if (reasonTf   != null) gom.reasonText   = reasonTf.GetComponent<TextMeshProUGUI>();
            if (subtitleTf != null) gom.subtitleText  = subtitleTf.GetComponent<TextMeshProUGUI>();

            // Wire buttons
            Transform retryBtnTf = gameOverPanelTf.Find("Card/RetryBtn");
            Transform menuBtnTf  = gameOverPanelTf.Find("Card/MenuBtn");
            Transform quitBtnTf  = gameOverPanelTf.Find("Card/QuitBtn");

            if (retryBtnTf != null)
            {
                Button retryBtn = retryBtnTf.GetComponent<Button>();
                if (retryBtn != null)
                {
                    retryBtn.onClick.RemoveAllListeners();
                    retryBtn.onClick.AddListener(gom.RetryLevel);
                }
            }
            if (menuBtnTf != null)
            {
                Button menuBtn = menuBtnTf.GetComponent<Button>();
                if (menuBtn != null)
                {
                    menuBtn.onClick.RemoveAllListeners();
                    menuBtn.onClick.AddListener(gom.GoToMainMenu);
                }
            }
            if (quitBtnTf != null)
            {
                Button quitBtn = quitBtnTf.GetComponent<Button>();
                if (quitBtn != null)
                {
                    quitBtn.onClick.RemoveAllListeners();
                    quitBtn.onClick.AddListener(gom.QuitGame);
                }
            }
        }

        // Wire GameTimer reference into GameOverManager
        gom.gameTimer = gameTimer;

        // Wire PauseMenuManager reference
        GameObject pmmGO = GameObject.Find("PauseMenuManager");
        if (pmmGO != null)
            gom.pauseMenuManager = pmmGO.GetComponent<PauseMenuManager>();

        // Also wire GameTimer into PauseMenuManager
        if (pmmGO != null)
        {
            PauseMenuManager pmm = pmmGO.GetComponent<PauseMenuManager>();
            if (pmm != null)
                pmm.gameTimer = gameTimer;
        }

        // ── 3. Scale down the map ──────────────────────────────────────────
        // Current scale is 60,60,60 which makes it ~58x56 units — reduce to ~20x20
        GameObject map = GameObject.Find("1stMap");
        if (map != null)
        {
            Undo.RecordObject(map.transform, "Scale Down Map");
            map.transform.localScale = new Vector3(20f, 20f, 20f);
            Debug.Log("1stMap scaled to 20,20,20");
        }
        else
        {
            Debug.LogWarning("1stMap not found — skipping map scale.");
        }

        // Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("Timer wired + Map scaled. Save the scene to persist changes.");
    }
}
