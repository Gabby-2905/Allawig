using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;

public class BuildInventoryUI
{
    // Theme colors matching main menu
    static readonly Color ColPanel     = new Color(0.08f, 0.12f, 0.15f, 0.97f);
    static readonly Color ColTeal      = new Color(0.18f, 0.45f, 0.50f, 1.00f);
    static readonly Color ColTealDark  = new Color(0.10f, 0.28f, 0.32f, 1.00f);
    static readonly Color ColAccent    = new Color(0.18f, 0.65f, 0.70f, 1.00f);
    static readonly Color ColSlot      = new Color(0.12f, 0.18f, 0.22f, 1.00f);
    static readonly Color ColSlotBorder= new Color(0.18f, 0.65f, 0.70f, 0.60f);
    static readonly Color ColWhite     = Color.white;
    static readonly Color ColGray      = new Color(0.65f, 0.75f, 0.78f, 1.00f);
    static readonly Color ColTabActive = new Color(0.18f, 0.45f, 0.50f, 1.00f);
    static readonly Color ColTabInact  = new Color(0.14f, 0.28f, 0.33f, 1.00f);

    [MenuItem("Tools/Build Inventory UI")]
    public static void Build()
    {
        // Remove existing
        var existing = GameObject.Find("InventoryCanvas");
        if (existing != null) Object.DestroyImmediate(existing);

        // ---- Canvas ----
        var canvasGO = new GameObject("InventoryCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // ---- Root container (holds both panels side by side) ----
        var rootGO = new GameObject("InventoryRoot");
        rootGO.transform.SetParent(canvasGO.transform, false);
        var rootRT = rootGO.AddComponent<RectTransform>();
        rootRT.anchorMin = new Vector2(0.5f, 0.5f);
        rootRT.anchorMax = new Vector2(0.5f, 0.5f);
        rootRT.pivot = new Vector2(0.5f, 0.5f);
        rootRT.sizeDelta = new Vector2(920f, 520f);
        rootRT.anchoredPosition = Vector2.zero;

        // ---- CHARACTER Panel (left) ----
        var charPanel = MakePanel(rootGO.transform, "CharacterPanel", 430f, 520f, -265f, 0f);
        BuildCharacterPanel(charPanel);

        // ---- INVENTORY Panel (right) ----
        var invPanel = MakePanel(rootGO.transform, "InventoryPanel", 460f, 520f, 245f, 0f);
        BuildInventoryPanel(invPanel);

        // ---- Close button (X) on root ----
        // Already have X on each panel

        // ---- Attach InventoryUI script ----
        var existingManager = GameObject.Find("InventoryUIManager");
        if (existingManager != null) Object.DestroyImmediate(existingManager);
        var managerGO = new GameObject("InventoryUIManager");
        var invUI = managerGO.AddComponent<InventoryUI>();
        invUI.inventoryRoot = rootGO;

        // Start hidden
        rootGO.SetActive(false);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        Debug.Log("[BuildInventoryUI] Done!");
    }

    // ===================== CHARACTER PANEL =====================
    static void BuildCharacterPanel(GameObject panel)
    {
        float panelW = 430f;
        float halfW  = panelW / 2f;

        // Title bar (full width)
        var titleBar = MakeImageBox(panel.transform, "TitleBar", panelW, 36f, 0f, -18f, new Vector2(0.5f,1f), new Vector2(0.5f,1f));
        titleBar.GetComponent<Image>().color = ColTealDark;
        MakeLabel(titleBar.transform, "TitleText", "CHARACTER", 16, FontStyles.Bold, ColAccent, TextAlignmentOptions.Center, true);

        // Close X button - inside panel top-right
        var closeBtn = MakeButton(panel.transform, "CloseBtn", "✕", 14, 28f, 28f, -8f, -8f, new Vector2(1f,1f), new Vector2(1f,1f));
        closeBtn.GetComponent<Image>().color = new Color(0.5f, 0.08f, 0.08f, 1f);

        // Tab row anchored from left edge of panel
        float tabY = -54f;
        // STATISTICS: x=-140, width=130 => spans -205 to -75
        MakeTabButton(panel.transform, "TabStatistics", "STATISTICS", -140f, tabY, 130f, true);
        // NEEDS: x=-5, width=90 => spans -50 to +40
        MakeTabButton(panel.transform, "TabNeeds",      "NEEDS",        -5f, tabY,  90f, false);

        // ---- Left gear column (x = -halfW + 28 + margin) ----
        string[] gearLabels = { "HEAD", "CHEST", "LEGS" };
        for (int i = 0; i < 3; i++)
            MakeGearSlot(panel.transform, gearLabels[i], -(halfW - 50f), -100f - i * 78f);

        // ---- Character model placeholder (center-left) ----
        var modelBox = MakeImageBox(panel.transform, "CharModelBox", 115f, 215f, -25f, -155f, new Vector2(0.5f,1f), new Vector2(0.5f,1f));
        modelBox.GetComponent<Image>().color = ColSlot;
        AddOutline(modelBox, ColSlotBorder, 1f);
        MakeLabel(modelBox.transform, "ModelLabel", "CHARACTER\nMODEL", 12, FontStyles.Normal, ColGray, TextAlignmentOptions.Center, false);

        // Bag / Weapon slots
        MakeGearSlot(panel.transform, "BAG",    65f, -100f);
        MakeGearSlot(panel.transform, "WEAPON", 65f, -178f);

        // ---- Stats column (right side) ----
        // x=145, width=100 → right edge = 145+50 = 195 < 215 ✓
        string[] stats = { "HP:  100/100", "STAMINA:  100", "HUNGER:  80", "THIRST:  75" };
        for (int i = 0; i < stats.Length; i++)
            MakeStatRow(panel.transform, "Stat_" + i, stats[i], halfW - 55f, -95f - i * 70f);

        // ---- INVENTORY button at bottom ----
        var invBtn = MakeButton(panel.transform, "InventoryBtn", "INVENTORY", 13, 140f, 36f, 0f, 28f, new Vector2(0.5f,0f), new Vector2(0.5f,0f));
        invBtn.GetComponent<Image>().color = ColTeal;
    }

    // ===================== INVENTORY PANEL =====================
    static void BuildInventoryPanel(GameObject panel)
    {
        // Title bar
        var titleBar = MakeImageBox(panel.transform, "TitleBar", 460f, 36f, 0f, -18f, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
        titleBar.GetComponent<Image>().color = ColTealDark;
        MakeLabel(titleBar.transform, "TitleText", "INVENTORY", 16, FontStyles.Bold, ColAccent, TextAlignmentOptions.Center, true);

        // Close X - inside panel top-right
        var closeBtn = MakeButton(panel.transform, "CloseBtn", "✕", 14, 28f, 28f, -8f, -8f, new Vector2(1f,1f), new Vector2(1f,1f));
        closeBtn.GetComponent<Image>().color = new Color(0.5f, 0.08f, 0.08f, 1f);

        // ---- 4 rows x 5 cols grid ----
        int rows = 4, cols = 5;
        float slotSize = 68f;
        float gap = 10f;
        float gridW = cols * slotSize + (cols - 1) * gap;
        float gridH = rows * slotSize + (rows - 1) * gap;
        float startX = -gridW / 2f + slotSize / 2f;
        float startY = -70f;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                float x = startX + c * (slotSize + gap);
                float y = startY - r * (slotSize + gap);
                MakeItemSlot(panel.transform, $"Slot_{r}_{c}", x, y, slotSize);
            }
        }

        // Weight / capacity label at bottom
        var weightLabel = MakeLabel(panel.transform, "WeightLabel", "WEIGHT:  0 / 50 kg", 13, FontStyles.Normal, ColGray, TextAlignmentOptions.Center, false);
        var wRT = weightLabel.GetComponent<RectTransform>();
        wRT.anchorMin = new Vector2(0.5f, 0f);
        wRT.anchorMax = new Vector2(0.5f, 0f);
        wRT.pivot = new Vector2(0.5f, 0f);
        wRT.sizeDelta = new Vector2(300f, 30f);
        wRT.anchoredPosition = new Vector2(0f, 20f);
    }

    // ===================== HELPERS =====================

    static GameObject MakePanel(Transform parent, string name, float w, float h, float x, float y)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = ColPanel;
        AddOutline(go, ColAccent, 1.5f);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = new Vector2(x, y);
        return go;
    }

    static GameObject MakeImageBox(Transform parent, string name, float w, float h, float x, float y, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = ColTealDark;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = new Vector2(x, y);
        return go;
    }

    static void MakeGearSlot(Transform parent, string label, float x, float y)
    {
        var go = new GameObject("GearSlot_" + label);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = ColSlot;
        AddOutline(go, ColSlotBorder, 1f);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(56f, 56f);
        rt.anchoredPosition = new Vector2(x, y);

        MakeLabel(go.transform, "Label", label, 9, FontStyles.Bold, ColGray, TextAlignmentOptions.Center, false);
    }

    static void MakeStatRow(Transform parent, string name, string text, float x, float y)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = ColSlot;
        AddOutline(go, ColSlotBorder, 0.8f);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(100f, 56f);  // narrower to stay inside panel
        rt.anchoredPosition = new Vector2(x, y);

        MakeLabel(go.transform, "Text", text, 10, FontStyles.Normal, ColWhite, TextAlignmentOptions.Center, false);
    }

    static void MakeItemSlot(Transform parent, string name, float x, float y, float size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = ColSlot;
        AddOutline(go, ColSlotBorder, 1f);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(size, size);
        rt.anchoredPosition = new Vector2(x, y);
    }

    static void MakeTabButton(Transform parent, string name, string label, float x, float y, float width, bool active)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = active ? ColTabActive : ColTabInact;
        AddOutline(go, ColAccent, 1.2f);
        go.AddComponent<Button>();
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(width, 30f);
        rt.anchoredPosition = new Vector2(x, y);
        MakeLabel(go.transform, "Text", label, 11, FontStyles.Bold, ColWhite, TextAlignmentOptions.Center, true);
    }

    static Button MakeButton(Transform parent, string name, string label, float fontSize,
        float w, float h, float x, float y, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = ColTeal;
        var btn = go.AddComponent<Button>();
        var cb = btn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f);
        cb.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        btn.colors = cb;
        AddOutline(go, ColAccent, 1f);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = new Vector2(x, y);
        MakeLabel(go.transform, "Text", label, fontSize, FontStyles.Bold, ColWhite, TextAlignmentOptions.Center, true);
        return btn;
    }

    static GameObject MakeLabel(Transform parent, string name, string text, float fontSize,
        FontStyles style, Color color, TextAlignmentOptions align, bool letterSpacing)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = align;
        if (letterSpacing) tmp.characterSpacing = 3f;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return go;
    }

    static void AddOutline(GameObject go, Color color, float size)
    {
        var ol = go.AddComponent<Outline>();
        ol.effectColor = color;
        ol.effectDistance = new Vector2(size, -size);
    }
}
