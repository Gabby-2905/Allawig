using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine.Events;
using TMPro;

/// <summary>
/// Fixes the Pause Menu and Inventory UI:
/// - Wires up all button onClick listeners to PauseMenuManager methods
/// - Fixes PausePanel size so all 6 buttons fit properly
/// - Fixes CanvasScaler to use proper screen-space scaling
/// - Fixes InventoryRoot layout and size
/// </summary>
public class FixPauseAndInventoryUI
{
    [MenuItem("Tools/Fix Pause & Inventory UI")]
    public static void Fix()
    {
        FixPauseMenu();
        FixInventoryUI();
        EditorUtility.SetDirty(GameObject.Find("PauseCanvas"));
        EditorUtility.SetDirty(GameObject.Find("InventoryCanvas"));
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        Debug.Log("[FixPauseAndInventoryUI] Done! All buttons wired and layouts fixed.");
    }

    static void FixPauseMenu()
    {
        // ── Find PauseCanvas & PauseMenuManager ──────────────────────────────
        GameObject pauseCanvas = GameObject.Find("PauseCanvas");
        if (pauseCanvas == null) { Debug.LogError("PauseCanvas not found!"); return; }

        PauseMenuManager pmm = pauseCanvas.GetComponent<PauseMenuManager>();
        if (pmm == null) { Debug.LogError("PauseMenuManager not found on PauseCanvas!"); return; }

        // ── Fix CanvasScaler ─────────────────────────────────────────────────
        CanvasScaler scaler = pauseCanvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        // Fix Canvas position/scale
        RectTransform canvasRT = pauseCanvas.GetComponent<RectTransform>();
        if (canvasRT != null)
        {
            canvasRT.anchorMin = Vector2.zero;
            canvasRT.anchorMax = Vector2.one;
            canvasRT.offsetMin = Vector2.zero;
            canvasRT.offsetMax = Vector2.zero;
            canvasRT.localPosition = Vector3.zero;
            canvasRT.localScale = Vector3.one;
        }

        // ── Fix PausePanel size & layout ─────────────────────────────────────
        Transform pausePanel = pauseCanvas.transform.Find("PausePanel");
        if (pausePanel != null)
        {
            RectTransform ppRT = pausePanel.GetComponent<RectTransform>();
            if (ppRT != null)
            {
                // Make panel tall enough for title + 6 buttons with spacing
                ppRT.sizeDelta = new Vector2(320f, 460f);
                ppRT.anchoredPosition = Vector2.zero;
            }

            // Fix button positions - evenly spaced below title
            // Title at top, then 6 buttons spaced 52px apart
            string[] btnNames = { "ResumeBtn", "Save GameBtn", "SettingsBtn", "ControlsBtn", "Main MenuBtn", "QuitBtn" };
            float startY = 140f;   // top of first button (from panel center)
            float spacing = 52f;

            for (int i = 0; i < btnNames.Length; i++)
            {
                Transform btn = pausePanel.Find(btnNames[i]);
                if (btn != null)
                {
                    RectTransform btnRT = btn.GetComponent<RectTransform>();
                    if (btnRT != null)
                    {
                        btnRT.anchorMin = new Vector2(0.5f, 0.5f);
                        btnRT.anchorMax = new Vector2(0.5f, 0.5f);
                        btnRT.pivot = new Vector2(0.5f, 0.5f);
                        btnRT.sizeDelta = new Vector2(260f, 44f);
                        btnRT.anchoredPosition = new Vector2(0f, startY - i * spacing);
                    }
                }
            }

            // Fix title position
            Transform title = pausePanel.Find("PauseTitle");
            if (title != null)
            {
                RectTransform titleRT = title.GetComponent<RectTransform>();
                if (titleRT != null)
                {
                    titleRT.anchorMin = new Vector2(0.5f, 0.5f);
                    titleRT.anchorMax = new Vector2(0.5f, 0.5f);
                    titleRT.pivot = new Vector2(0.5f, 0.5f);
                    titleRT.anchoredPosition = new Vector2(0f, 205f);
                    titleRT.sizeDelta = new Vector2(280f, 50f);
                }
                // Fix title text
                TextMeshProUGUI titleTMP = title.GetComponent<TextMeshProUGUI>();
                if (titleTMP != null)
                {
                    titleTMP.text = "PAUSED";
                    titleTMP.fontSize = 32f;
                    titleTMP.alignment = TextAlignmentOptions.Center;
                    titleTMP.color = Color.white;
                }
            }
        }

        // ── Wire up Pause Panel buttons ───────────────────────────────────────
        WireButton(pauseCanvas, "PausePanel/ResumeBtn",    pmm, "Resume");
        WireButton(pauseCanvas, "PausePanel/Save GameBtn", pmm, "SaveGame");
        WireButton(pauseCanvas, "PausePanel/SettingsBtn",  pmm, "OpenSettings");
        WireButton(pauseCanvas, "PausePanel/ControlsBtn",  pmm, "OpenControls");
        WireButton(pauseCanvas, "PausePanel/Main MenuBtn", pmm, "GoToMainMenu");
        WireButton(pauseCanvas, "PausePanel/QuitBtn",      pmm, "QuitToDesktop");

        // ── Wire up Settings Panel back button ────────────────────────────────
        WireButton(pauseCanvas, "SettingsPanel/BackBtn", pmm, "CloseSettings");

        // ── Wire up Controls Panel back button ────────────────────────────────
        WireButton(pauseCanvas, "ControlsPanel/BackBtn", pmm, "CloseControls");

        // ── Fix button label text ─────────────────────────────────────────────
        FixButtonLabel(pauseCanvas, "PausePanel/ResumeBtn/Label",    "Resume");
        FixButtonLabel(pauseCanvas, "PausePanel/Save GameBtn/Label", "Save Game");
        FixButtonLabel(pauseCanvas, "PausePanel/SettingsBtn/Label",  "Settings");
        FixButtonLabel(pauseCanvas, "PausePanel/ControlsBtn/Label",  "Controls");
        FixButtonLabel(pauseCanvas, "PausePanel/Main MenuBtn/Label", "Main Menu");
        FixButtonLabel(pauseCanvas, "PausePanel/QuitBtn/Label",      "Quit");
        FixButtonLabel(pauseCanvas, "SettingsPanel/BackBtn/Label",   "Back");
        FixButtonLabel(pauseCanvas, "ControlsPanel/BackBtn/Label",   "Back");

        // ── Fix SettingsPanel layout ──────────────────────────────────────────
        Transform settingsPanel = pauseCanvas.transform.Find("SettingsPanel");
        if (settingsPanel != null)
        {
            RectTransform spRT = settingsPanel.GetComponent<RectTransform>();
            if (spRT != null)
            {
                spRT.sizeDelta = new Vector2(380f, 320f);
                spRT.anchoredPosition = Vector2.zero;
            }
            // Fix title
            Transform spTitle = settingsPanel.Find("Title");
            if (spTitle != null)
            {
                RectTransform titleRT = spTitle.GetComponent<RectTransform>();
                if (titleRT != null)
                {
                    titleRT.anchoredPosition = new Vector2(0f, 130f);
                    titleRT.sizeDelta = new Vector2(300f, 50f);
                }
                TextMeshProUGUI tmp = spTitle.GetComponent<TextMeshProUGUI>();
                if (tmp != null) { tmp.text = "SETTINGS"; tmp.fontSize = 28f; tmp.alignment = TextAlignmentOptions.Center; tmp.color = Color.white; }
            }
            // Fix back button
            Transform backBtn = settingsPanel.Find("BackBtn");
            if (backBtn != null)
            {
                RectTransform btnRT = backBtn.GetComponent<RectTransform>();
                if (btnRT != null)
                {
                    btnRT.anchoredPosition = new Vector2(0f, -120f);
                    btnRT.sizeDelta = new Vector2(200f, 44f);
                }
            }
        }

        // ── Fix ControlsPanel layout ──────────────────────────────────────────
        Transform controlsPanel = pauseCanvas.transform.Find("ControlsPanel");
        if (controlsPanel != null)
        {
            RectTransform cpRT = controlsPanel.GetComponent<RectTransform>();
            if (cpRT != null)
            {
                cpRT.sizeDelta = new Vector2(420f, 420f);
                cpRT.anchoredPosition = Vector2.zero;
            }
            // Fix title
            Transform cpTitle = controlsPanel.Find("Title");
            if (cpTitle != null)
            {
                RectTransform titleRT = cpTitle.GetComponent<RectTransform>();
                if (titleRT != null)
                {
                    titleRT.anchoredPosition = new Vector2(0f, 185f);
                    titleRT.sizeDelta = new Vector2(360f, 50f);
                }
                TextMeshProUGUI tmp = cpTitle.GetComponent<TextMeshProUGUI>();
                if (tmp != null) { tmp.text = "CONTROLS"; tmp.fontSize = 28f; tmp.alignment = TextAlignmentOptions.Center; tmp.color = Color.white; }
            }
            // Fix control labels positions
            string[] ctrlNames = { "ctrl0", "ctrl1", "ctrl2", "ctrl3", "ctrl4", "ctrl5", "ctrl6" };
            float ctrlStartY = 145f;
            float ctrlSpacing = 38f;
            for (int i = 0; i < ctrlNames.Length; i++)
            {
                Transform ctrl = controlsPanel.Find(ctrlNames[i]);
                if (ctrl != null)
                {
                    RectTransform ctrlRT = ctrl.GetComponent<RectTransform>();
                    if (ctrlRT != null)
                    {
                        ctrlRT.anchorMin = new Vector2(0.5f, 0.5f);
                        ctrlRT.anchorMax = new Vector2(0.5f, 0.5f);
                        ctrlRT.pivot = new Vector2(0.5f, 0.5f);
                        ctrlRT.anchoredPosition = new Vector2(0f, ctrlStartY - i * ctrlSpacing);
                        ctrlRT.sizeDelta = new Vector2(380f, 32f);
                    }
                }
            }
            // Fix back button
            Transform backBtn = controlsPanel.Find("BackBtn");
            if (backBtn != null)
            {
                RectTransform btnRT = backBtn.GetComponent<RectTransform>();
                if (btnRT != null)
                {
                    btnRT.anchoredPosition = new Vector2(0f, -175f);
                    btnRT.sizeDelta = new Vector2(200f, 44f);
                }
            }
        }

        Debug.Log("[FixPauseAndInventoryUI] Pause menu fixed.");
    }

    static void FixInventoryUI()
    {
        GameObject invCanvas = GameObject.Find("InventoryCanvas");
        if (invCanvas == null) { Debug.LogError("InventoryCanvas not found!"); return; }

        // Fix CanvasScaler
        CanvasScaler scaler = invCanvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        // Fix Canvas RectTransform
        RectTransform canvasRT = invCanvas.GetComponent<RectTransform>();
        if (canvasRT != null)
        {
            canvasRT.anchorMin = Vector2.zero;
            canvasRT.anchorMax = Vector2.one;
            canvasRT.offsetMin = Vector2.zero;
            canvasRT.offsetMax = Vector2.zero;
            canvasRT.localPosition = Vector3.zero;
            canvasRT.localScale = Vector3.one;
        }

        Transform invRoot = invCanvas.transform.Find("InventoryRoot");
        if (invRoot == null) { Debug.LogError("InventoryRoot not found!"); return; }

        // Fix InventoryRoot size - make it bigger to show 8 slots in 2 rows of 4
        RectTransform rootRT = invRoot.GetComponent<RectTransform>();
        if (rootRT != null)
        {
            rootRT.anchorMin = new Vector2(0.5f, 0.5f);
            rootRT.anchorMax = new Vector2(0.5f, 0.5f);
            rootRT.pivot = new Vector2(0.5f, 0.5f);
            rootRT.sizeDelta = new Vector2(600f, 340f);
            rootRT.anchoredPosition = Vector2.zero;
        }

        // Fix title
        Transform invTitle = invRoot.Find("InventoryTitle");
        if (invTitle != null)
        {
            RectTransform titleRT = invTitle.GetComponent<RectTransform>();
            if (titleRT != null)
            {
                titleRT.anchorMin = new Vector2(0.5f, 0.5f);
                titleRT.anchorMax = new Vector2(0.5f, 0.5f);
                titleRT.pivot = new Vector2(0.5f, 0.5f);
                titleRT.anchoredPosition = new Vector2(0f, 145f);
                titleRT.sizeDelta = new Vector2(500f, 50f);
            }
            TextMeshProUGUI titleTMP = invTitle.GetComponent<TextMeshProUGUI>();
            if (titleTMP != null)
            {
                titleTMP.text = "INVENTORY";
                titleTMP.fontSize = 28f;
                titleTMP.alignment = TextAlignmentOptions.Center;
                titleTMP.color = Color.white;
            }
        }

        // Arrange 8 slots in 2 rows of 4
        // Row 1: Slot0-3, Row 2: Slot4-7
        float slotSize = 110f;
        float slotSpacing = 120f;
        float rowSpacing = 120f;
        float row1Y = 40f;
        float row2Y = row1Y - rowSpacing;
        float startX = -1.5f * slotSpacing; // center 4 slots

        for (int i = 0; i < 8; i++)
        {
            Transform slot = invRoot.Find("Slot" + i);
            if (slot == null) continue;

            RectTransform slotRT = slot.GetComponent<RectTransform>();
            if (slotRT != null)
            {
                slotRT.anchorMin = new Vector2(0.5f, 0.5f);
                slotRT.anchorMax = new Vector2(0.5f, 0.5f);
                slotRT.pivot = new Vector2(0.5f, 0.5f);
                slotRT.sizeDelta = new Vector2(slotSize, slotSize);

                int col = i % 4;
                int row = i / 4;
                float x = startX + col * slotSpacing;
                float y = row == 0 ? row1Y : row2Y;
                slotRT.anchoredPosition = new Vector2(x, y);
            }

            // Fix slot number label
            Transform slotNum = slot.Find("SlotNum");
            if (slotNum != null)
            {
                RectTransform snRT = slotNum.GetComponent<RectTransform>();
                if (snRT != null)
                {
                    snRT.anchorMin = new Vector2(0f, 1f);
                    snRT.anchorMax = new Vector2(0f, 1f);
                    snRT.pivot = new Vector2(0f, 1f);
                    snRT.anchoredPosition = new Vector2(4f, -4f);
                    snRT.sizeDelta = new Vector2(30f, 20f);
                }
                TextMeshProUGUI snTMP = slotNum.GetComponent<TextMeshProUGUI>();
                if (snTMP != null)
                {
                    snTMP.text = (i + 1).ToString();
                    snTMP.fontSize = 12f;
                    snTMP.color = new Color(0.7f, 0.7f, 0.7f, 1f);
                }
            }

            // Fix item label
            Transform itemLabel = slot.Find("ItemLabel");
            if (itemLabel != null)
            {
                RectTransform ilRT = itemLabel.GetComponent<RectTransform>();
                if (ilRT != null)
                {
                    ilRT.anchorMin = new Vector2(0f, 0f);
                    ilRT.anchorMax = new Vector2(1f, 0f);
                    ilRT.pivot = new Vector2(0.5f, 0f);
                    ilRT.anchoredPosition = new Vector2(0f, 4f);
                    ilRT.sizeDelta = new Vector2(0f, 24f);
                }
                TextMeshProUGUI ilTMP = itemLabel.GetComponent<TextMeshProUGUI>();
                if (ilTMP != null)
                {
                    ilTMP.fontSize = 10f;
                    ilTMP.alignment = TextAlignmentOptions.Center;
                    ilTMP.color = Color.white;
                }
            }

            // Fix border
            Transform border = slot.Find("Border");
            if (border != null)
            {
                RectTransform borderRT = border.GetComponent<RectTransform>();
                if (borderRT != null)
                {
                    borderRT.anchorMin = Vector2.zero;
                    borderRT.anchorMax = Vector2.one;
                    borderRT.offsetMin = Vector2.zero;
                    borderRT.offsetMax = Vector2.zero;
                }
            }
        }

        Debug.Log("[FixPauseAndInventoryUI] Inventory UI fixed.");
    }

    /// <summary>
    /// Wires a button's onClick to a method on the given MonoBehaviour.
    /// Clears existing listeners first to avoid duplicates.
    /// </summary>
    static void WireButton(GameObject root, string path, MonoBehaviour target, string methodName)
    {
        Transform t = root.transform.Find(path);
        if (t == null)
        {
            Debug.LogWarning($"[FixPauseAndInventoryUI] Button not found at path: {path}");
            return;
        }

        Button btn = t.GetComponent<Button>();
        if (btn == null)
        {
            Debug.LogWarning($"[FixPauseAndInventoryUI] No Button component at: {path}");
            return;
        }

        // Set the targetGraphic to the button's own Image so color tint works
        Image img = t.GetComponent<Image>();
        if (img != null) btn.targetGraphic = img;

        // Clear existing persistent listeners
        btn.onClick.RemoveAllListeners();
        // Remove all persistent listeners safely
        for (int i = btn.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
            UnityEventTools.RemovePersistentListener(btn.onClick, i);

        // Add the persistent listener
        var method = target.GetType().GetMethod(methodName,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (method == null)
        {
            Debug.LogWarning($"[FixPauseAndInventoryUI] Method '{methodName}' not found on {target.GetType().Name}");
            return;
        }

        UnityAction action = (UnityAction)System.Delegate.CreateDelegate(typeof(UnityAction), target, method);
        UnityEventTools.AddPersistentListener(btn.onClick, action);
        EditorUtility.SetDirty(btn);

        Debug.Log($"[FixPauseAndInventoryUI] Wired {path} -> {target.GetType().Name}.{methodName}()");
    }

    static void FixButtonLabel(GameObject root, string path, string labelText)
    {
        Transform t = root.transform.Find(path);
        if (t == null) return;

        TextMeshProUGUI tmp = t.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = labelText;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.fontSize = 18f;
        }

        // Fix label RectTransform to fill button
        RectTransform rt = t.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
