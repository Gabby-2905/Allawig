using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// Builds PauseCanvas and InventoryCanvas in Level2Scene,
/// matching the style shown in the reference screenshots.
/// </summary>
public class BuildLevel2UI
{
    [MenuItem("Tools/Build Level2 UI (Pause + Inventory)")]
    public static void Build()
    {
        // ── Fonts ──────────────────────────────────────────────────────────
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/BebasNeue-Regular SDF.asset");
        if (font == null)
            font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");

        // ── Colors (matching reference screenshots) ────────────────────────
        Color overlayBg      = new Color(0f, 0f, 0f, 0.55f);
        Color panelBg        = new Color(0.08f, 0.08f, 0.08f, 0.85f);
        Color tealBtn        = new Color(0.18f, 0.55f, 0.60f, 1f);
        Color tealBtnHover   = new Color(0.22f, 0.65f, 0.72f, 1f);
        Color tealBtnPress   = new Color(0.12f, 0.40f, 0.45f, 1f);
        Color whiteText      = new Color(1f, 1f, 1f, 1f);
        Color titleText      = new Color(1f, 1f, 1f, 1f);
        Color invSlotBg      = new Color(0.35f, 0.35f, 0.50f, 0.80f);
        Color invSlotBorder  = new Color(0.20f, 0.20f, 0.35f, 1f);
        Color invPanelBg     = new Color(0.05f, 0.05f, 0.10f, 0.80f);

        // ── Ensure EventSystem exists ──────────────────────────────────────
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Undo.RegisterCreatedObjectUndo(esGO, "Create EventSystem");
        }

        // ══════════════════════════════════════════════════════════════════
        // PAUSE CANVAS
        // ══════════════════════════════════════════════════════════════════
        DestroyExisting("PauseCanvas");
        GameObject pauseCanvasGO = CreateCanvas("PauseCanvas", 40);

        // Full-screen overlay
        GameObject pauseOverlay = CreateImageGO("Overlay", pauseCanvasGO, overlayBg);
        StretchFull(pauseOverlay.GetComponent<RectTransform>());

        // Pause panel — narrow vertical panel on left side (matching screenshot)
        GameObject pausePanel = CreateImageGO("PausePanel", pauseOverlay, panelBg);
        RectTransform pausePanelRT = pausePanel.GetComponent<RectTransform>();
        pausePanelRT.anchorMin = new Vector2(0f, 0f);
        pausePanelRT.anchorMax = new Vector2(0f, 1f);
        pausePanelRT.pivot     = new Vector2(0f, 0.5f);
        pausePanelRT.offsetMin = new Vector2(0, 0);
        pausePanelRT.offsetMax = new Vector2(192, 0);

        // "PAUSED" title
        GameObject titleGO = CreateTMPText(pausePanel, "TitleText", "PAUSED",
            font, 36, titleText, FontStyles.Bold,
            new Vector2(96, -40), new Vector2(180, 50));
        titleGO.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
        titleGO.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
        titleGO.GetComponent<RectTransform>().pivot     = new Vector2(0.5f, 1f);
        titleGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(96, -20);

        // Buttons — stacked vertically
        string[] btnLabels = { "Resume", "Save Game", "Settings", "Controls", "Main Menu", "Quit" };
        float btnStartY = -90f;
        float btnSpacing = 52f;
        float btnW = 160f;
        float btnH = 44f;

        GameObject[] pauseBtns = new GameObject[btnLabels.Length];
        for (int i = 0; i < btnLabels.Length; i++)
        {
            float yPos = btnStartY - i * btnSpacing;
            pauseBtns[i] = CreatePauseButton(pausePanel, btnLabels[i] + "Button", btnLabels[i],
                font, 20, whiteText, tealBtn, tealBtnHover, tealBtnPress,
                new Vector2(96, yPos), new Vector2(btnW, btnH));
        }

        // PauseMenuManager component on canvas
        PauseMenuManager pmm = pauseCanvasGO.AddComponent<PauseMenuManager>();
        pmm.pauseMenuPanel = pausePanel;

        // Wire button listeners via persistent onClick
        AddPersistentListener(pauseBtns[0].GetComponent<Button>(), pmm, "Resume");
        AddPersistentListener(pauseBtns[1].GetComponent<Button>(), pmm, "SaveGame");
        AddPersistentListener(pauseBtns[2].GetComponent<Button>(), pmm, "OpenSettings");
        AddPersistentListener(pauseBtns[3].GetComponent<Button>(), pmm, "OpenControls");
        AddPersistentListener(pauseBtns[4].GetComponent<Button>(), pmm, "GoToMainMenu");
        AddPersistentListener(pauseBtns[5].GetComponent<Button>(), pmm, "QuitToDesktop");

        pauseCanvasGO.SetActive(false);
        EditorUtility.SetDirty(pauseCanvasGO);

        // ══════════════════════════════════════════════════════════════════
        // INVENTORY CANVAS
        // ══════════════════════════════════════════════════════════════════
        DestroyExisting("InventoryCanvas");
        GameObject invCanvasGO = CreateCanvas("InventoryCanvas", 35);

        // Full-screen semi-transparent overlay
        GameObject invOverlay = CreateImageGO("Overlay", invCanvasGO, new Color(0f, 0f, 0f, 0.45f));
        StretchFull(invOverlay.GetComponent<RectTransform>());

        // Inventory root (the panel shown in screenshot — wide, centered, lower half)
        GameObject invRoot = CreateImageGO("InventoryRoot", invOverlay, invPanelBg);
        RectTransform invRootRT = invRoot.GetComponent<RectTransform>();
        invRootRT.anchorMin = new Vector2(0.5f, 0.5f);
        invRootRT.anchorMax = new Vector2(0.5f, 0.5f);
        invRootRT.pivot     = new Vector2(0.5f, 0.5f);
        invRootRT.sizeDelta = new Vector2(580, 420);
        invRootRT.anchoredPosition = new Vector2(0, -40);

        // "INVENTORY" title
        GameObject invTitleGO = CreateTMPText(invRoot, "TitleText", "INVENTORY",
            font, 30, titleText, FontStyles.Bold,
            new Vector2(0, 0), new Vector2(680, 44));
        RectTransform invTitleRT = invTitleGO.GetComponent<RectTransform>();
        invTitleRT.anchorMin = new Vector2(0.5f, 1f);
        invTitleRT.anchorMax = new Vector2(0.5f, 1f);
        invTitleRT.pivot     = new Vector2(0.5f, 1f);
        invTitleRT.anchoredPosition = new Vector2(0, -12);
        invTitleRT.sizeDelta = new Vector2(560, 44);

        // Grid of 8 slots (2 rows x 4 cols) matching screenshot
        int cols = 4;
        int rows = 2;
        float slotSize   = 110f;
        float slotPad    = 8f;
        float gridW      = cols * slotSize + (cols - 1) * slotPad;  // 4*110 + 3*8 = 464
        float gridH      = rows * slotSize + (rows - 1) * slotPad;  // 2*110 + 1*8 = 228
        float gridStartX = -(gridW / 2f) + slotSize / 2f;           // -232 + 55 = -177
        // Title takes ~52px from top, then 16px gap, then grid starts
        // Panel height 420, pivot center => top = +210, title at +185
        // Grid top row center = 210 - 52 - 16 - 55 = +87 from panel center
        float gridStartY = 130f;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                int slotIndex = r * cols + c + 1;
                float x = gridStartX + c * (slotSize + slotPad);
                float y = gridStartY - r * (slotSize + slotPad);

                GameObject slot = CreateImageGO("Slot_" + slotIndex, invRoot, invSlotBg);
                RectTransform slotRT = slot.GetComponent<RectTransform>();
                slotRT.anchorMin = new Vector2(0.5f, 0.5f);
                slotRT.anchorMax = new Vector2(0.5f, 0.5f);
                slotRT.pivot     = new Vector2(0.5f, 0.5f);
                slotRT.sizeDelta = new Vector2(slotSize, slotSize);
                slotRT.anchoredPosition = new Vector2(x, y);

                // Slot number label (top-left, small)
                GameObject numGO = new GameObject("SlotNum");
                numGO.transform.SetParent(slot.transform, false);
                TextMeshProUGUI numTMP = numGO.AddComponent<TextMeshProUGUI>();
                numTMP.text = slotIndex.ToString();
                if (font != null) numTMP.font = font;
                numTMP.fontSize = 14;
                numTMP.color = new Color(0.8f, 0.8f, 0.9f, 0.7f);
                numTMP.alignment = TextAlignmentOptions.TopLeft;
                RectTransform numRT = numGO.GetComponent<RectTransform>();
                numRT.anchorMin = Vector2.zero;
                numRT.anchorMax = Vector2.one;
                numRT.offsetMin = new Vector2(6, 6);
                numRT.offsetMax = new Vector2(-6, -6);
            }
        }

        // InventoryUI component
        InventoryUI invUI = invCanvasGO.AddComponent<InventoryUI>();
        invUI.inventoryRoot = invRoot;

        invCanvasGO.SetActive(false);
        EditorUtility.SetDirty(invCanvasGO);

        // ── Save scene ─────────────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

        Debug.Log("[BuildLevel2UI] PauseCanvas and InventoryCanvas built in Level2Scene!");
        Selection.activeGameObject = pauseCanvasGO;
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    static void DestroyExisting(string name)
    {
        var go = GameObject.Find(name);
        if (go != null) Undo.DestroyObjectImmediate(go);
    }

    static GameObject CreateCanvas(string name, int sortOrder)
    {
        GameObject go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        Canvas c = go.AddComponent<Canvas>();
        c.renderMode  = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = sortOrder;
        CanvasScaler cs = go.AddComponent<CanvasScaler>();
        cs.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cs.matchWidthOrHeight  = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        return go;
    }

    static GameObject CreateImageGO(string name, GameObject parent, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        Image img = go.AddComponent<Image>();
        img.color = color;
        img.type  = Image.Type.Simple;
        return go;
    }

    static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static GameObject CreateTMPText(GameObject parent, string name, string text,
        TMP_FontAsset font, float fontSize, Color color, FontStyles style,
        Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        if (font != null) tmp.font = font;
        tmp.fontSize  = fontSize;
        tmp.color     = color;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        return go;
    }

    static GameObject CreatePauseButton(GameObject parent, string name, string label,
        TMP_FontAsset font, float fontSize, Color textColor,
        Color normalColor, Color hoverColor, Color pressColor,
        Vector2 anchoredPos, Vector2 size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);

        Image img = go.AddComponent<Image>();
        img.color = normalColor;
        img.type  = Image.Type.Simple;

        Button btn = go.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor      = normalColor;
        cb.highlightedColor = hoverColor;
        cb.pressedColor     = pressColor;
        cb.selectedColor    = normalColor;
        cb.fadeDuration     = 0.1f;
        btn.colors = cb;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = anchoredPos;

        // Label
        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(go.transform, false);
        TextMeshProUGUI tmp = labelGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        if (font != null) tmp.font = font;
        tmp.fontSize  = fontSize;
        tmp.color     = textColor;
        tmp.fontStyle = FontStyles.Normal;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
        RectTransform labelRT = labelGO.GetComponent<RectTransform>();
        labelRT.anchorMin = Vector2.zero;
        labelRT.anchorMax = Vector2.one;
        labelRT.offsetMin = labelRT.offsetMax = Vector2.zero;

        return go;
    }

    static void AddPersistentListener(Button btn, Object target, string methodName)
    {
        UnityEngine.Events.UnityAction action = null;
        var method = target.GetType().GetMethod(methodName,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (method == null)
        {
            Debug.LogWarning($"[BuildLevel2UI] Method not found: {methodName}");
            return;
        }
        action = (UnityEngine.Events.UnityAction)System.Delegate.CreateDelegate(
            typeof(UnityEngine.Events.UnityAction), target, method);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, action);
        EditorUtility.SetDirty(btn);
    }
}
