using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Builds the Level Complete UI canvas in the active scene.
/// </summary>
public class BuildLevelCompleteUI : EditorWindow
{
    [MenuItem("Tools/Build Level Complete UI")]
    public static void Build()
    {
        // ── Fonts ──────────────────────────────────────────────────────────
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/BebasNeue-Regular SDF.asset");
        if (font == null)
            font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");

        // ── Sprites ────────────────────────────────────────────────────────
        string spritePath = "Assets/_UI_COPLAY_GENERATED/LevelComplete/Sprites/";
        Sprite starFilledSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "StarFilled.png");
        Sprite starEmptySprite  = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "StarEmpty.png");
        Sprite hazardSprite     = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "HazardStripe.png");

        // ── Colors ─────────────────────────────────────────────────────────
        Color panelBg          = new Color(0.10f, 0.09f, 0.07f, 0.97f);
        Color overlayBg        = new Color(0f, 0f, 0f, 0.75f);
        Color goldText         = new Color(1f, 0.78f, 0.1f, 1f);
        Color whiteText        = new Color(0.95f, 0.95f, 0.95f, 1f);
        Color darkBtn          = new Color(0.22f, 0.26f, 0.30f, 1f);
        Color orangeBtn        = new Color(0.95f, 0.60f, 0.05f, 1f);
        Color objectivePanelBg = new Color(0.08f, 0.08f, 0.06f, 1f);
        Color borderStripe     = new Color(0.95f, 0.60f, 0.05f, 1f);

        // ── Remove existing canvas ─────────────────────────────────────────
        var existing = GameObject.Find("LevelCompleteCanvas");
        if (existing != null)
            Undo.DestroyObjectImmediate(existing);

        // ── Root Canvas ────────────────────────────────────────────────────
        GameObject canvasGO = new GameObject("LevelCompleteCanvas");
        Undo.RegisterCreatedObjectUndo(canvasGO, "Create LevelCompleteCanvas");

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // ── Full-screen dim overlay ────────────────────────────────────────
        GameObject overlay = CreatePanel(canvasGO, "Overlay", overlayBg);
        StretchFull(overlay.GetComponent<RectTransform>());

        // ── Main Panel ─────────────────────────────────────────────────────
        GameObject panel = CreatePanel(overlay, "Panel", panelBg);
        RectTransform panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin = panelRT.anchorMax = new Vector2(0.5f, 0.5f);
        panelRT.pivot = new Vector2(0.5f, 0.5f);
        panelRT.sizeDelta = new Vector2(760, 480);
        panelRT.anchoredPosition = Vector2.zero;

        // ── Top stripe (hazard yellow-black) ──────────────────────────────
        AddHazardStripe(panel, "TopStripe",  new Vector2(0,  240), new Vector2(760, 20), hazardSprite, borderStripe);
        AddHazardStripe(panel, "BotStripe",  new Vector2(0, -240), new Vector2(760, 20), hazardSprite, borderStripe);

        // Corner bolts
        AddBolt(panel, "BoltTL", new Vector2(-355,  225));
        AddBolt(panel, "BoltTR", new Vector2( 355,  225));
        AddBolt(panel, "BoltBL", new Vector2(-355, -225));
        AddBolt(panel, "BoltBR", new Vector2( 355, -225));

        // ── Stars Row ─────────────────────────────────────────────────────
        GameObject starCenterGO = CreateStarBadge(panel, "StarCenter", new Vector2(0,    130), 130, true,  starFilledSprite, starEmptySprite);
        GameObject starLeftGO   = CreateStarBadge(panel, "StarLeft",   new Vector2(-185, 115), 110, true,  starFilledSprite, starEmptySprite);
        GameObject starRightGO  = CreateStarBadge(panel, "StarRight",  new Vector2( 185, 115), 110, false, starFilledSprite, starEmptySprite);

        // ── LEVEL COMPLETED text ──────────────────────────────────────────
        CreateTMPText(panel, "LevelCompletedText",
            "LEVEL COMPLETED",
            font, 62, goldText, FontStyles.Bold,
            new Vector2(0, 20), new Vector2(680, 80));

        // ── Objective Panel ───────────────────────────────────────────────
        GameObject objPanel = CreatePanel(panel, "ObjectivePanel", objectivePanelBg);
        RectTransform objRT = objPanel.GetComponent<RectTransform>();
        objRT.anchorMin = objRT.anchorMax = new Vector2(0.5f, 0.5f);
        objRT.pivot = new Vector2(0.5f, 0.5f);
        objRT.sizeDelta = new Vector2(500, 58);
        objRT.anchoredPosition = new Vector2(0, -60);

        // Objective icon (crosshair text symbol)
        GameObject iconGO = new GameObject("ObjectiveIcon");
        iconGO.transform.SetParent(objPanel.transform, false);
        TextMeshProUGUI iconTMP = iconGO.AddComponent<TextMeshProUGUI>();
        iconTMP.text = "⊕";
        iconTMP.fontSize = 22;
        iconTMP.color = goldText;
        iconTMP.alignment = TextAlignmentOptions.Center;
        RectTransform iconRT = iconGO.GetComponent<RectTransform>();
        iconRT.anchorMin = iconRT.anchorMax = new Vector2(0.5f, 0.5f);
        iconRT.pivot = new Vector2(0.5f, 0.5f);
        iconRT.sizeDelta = new Vector2(30, 30);
        iconRT.anchoredPosition = new Vector2(-185, 0);

        // Objective text
        GameObject objTextGO = CreateTMPText(objPanel, "ObjectiveText",
            "PRIMARY OBJECTIVE:  <b>8/8</b>",
            font, 22, whiteText, FontStyles.Normal,
            new Vector2(20, 0), new Vector2(460, 50));
        objTextGO.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // ── Buttons Row ───────────────────────────────────────────────────
        GameObject btnBack = CreateButton(panel, "BackToMainMenuButton",
            "BACK TO MAIN MENU", font, 18, whiteText, darkBtn,
            new Vector2(-155, -165), new Vector2(270, 62));

        GameObject btnNext = CreateButton(panel, "NextSectorButton",
            "NEXT SECTOR  →", font, 18, new Color(0.1f, 0.07f, 0f, 1f), orangeBtn,
            new Vector2(155, -165), new Vector2(270, 62));

        // ── LevelCompleteManager component ────────────────────────────────
        LevelCompleteManager mgr = canvasGO.AddComponent<LevelCompleteManager>();
        mgr.levelCompleteCanvas  = canvasGO;
        mgr.starLeft             = starLeftGO.GetComponent<Image>();
        mgr.starCenter           = starCenterGO.GetComponent<Image>();
        mgr.starRight            = starRightGO.GetComponent<Image>();
        mgr.objectiveText        = objTextGO.GetComponent<TextMeshProUGUI>();
        mgr.backToMainMenuButton = btnBack.GetComponent<Button>();
        mgr.nextSectorButton     = btnNext.GetComponent<Button>();
        mgr.starsEarned          = 2;
        mgr.objectiveCurrent     = 8;
        mgr.objectiveTotal       = 8;
        mgr.starFilledSprite     = starFilledSprite;
        mgr.starEmptySprite      = starEmptySprite;
        mgr.mainMenuSceneName    = "MainMenuScene";
        mgr.nextSceneName        = "Level2Scene";

        // Start hidden
        canvasGO.SetActive(false);

        EditorUtility.SetDirty(canvasGO);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[BuildLevelCompleteUI] Level Complete Canvas created successfully!");
        Selection.activeGameObject = canvasGO;
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    static GameObject CreatePanel(GameObject parent, string name, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        Image img = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static void AddHazardStripe(GameObject parent, string name, Vector2 pos, Vector2 size, Sprite sprite, Color fallbackColor)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        Image img = go.AddComponent<Image>();
        if (sprite != null)
        {
            img.sprite = sprite;
            img.type   = Image.Type.Tiled;
            img.color  = Color.white;
        }
        else
        {
            img.color = fallbackColor;
        }
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
    }

    static void AddBolt(GameObject parent, string name, Vector2 pos)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        Image img = go.AddComponent<Image>();
        img.color = new Color(0.3f, 0.28f, 0.22f, 1f);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(14, 14);
        rt.anchoredPosition = pos;
    }

    static GameObject CreateStarBadge(GameObject parent, string name, Vector2 pos, float size, bool filled,
        Sprite filledSprite, Sprite emptySprite)
    {
        Color frameColor  = filled ? new Color(0.85f, 0.50f, 0.02f, 1f) : new Color(0.50f, 0.35f, 0.02f, 1f);
        Color starColor   = filled ? new Color(1f, 0.82f, 0.1f, 1f)     : new Color(0.45f, 0.32f, 0.04f, 1f);

        // Outer frame
        GameObject frame = new GameObject(name);
        frame.transform.SetParent(parent.transform, false);
        Image frameImg = frame.AddComponent<Image>();

        Sprite chosenSprite = filled ? filledSprite : emptySprite;
        if (chosenSprite != null)
        {
            frameImg.sprite = chosenSprite;
            frameImg.color  = Color.white;
            frameImg.type   = Image.Type.Simple;
            frameImg.preserveAspect = true;
        }
        else
        {
            // Fallback: colored square with star text
            frameImg.color = frameColor;

            // Inner dark circle
            GameObject inner = new GameObject("Inner");
            inner.transform.SetParent(frame.transform, false);
            Image innerImg = inner.AddComponent<Image>();
            innerImg.color = new Color(0.10f, 0.09f, 0.07f, 1f);
            RectTransform innerRT = inner.GetComponent<RectTransform>();
            innerRT.anchorMin = innerRT.anchorMax = new Vector2(0.5f, 0.5f);
            innerRT.pivot     = new Vector2(0.5f, 0.5f);
            innerRT.sizeDelta = new Vector2(size * 0.78f, size * 0.78f);
            innerRT.anchoredPosition = Vector2.zero;

            // Star symbol text
            GameObject starText = new GameObject("StarSymbol");
            starText.transform.SetParent(frame.transform, false);
            TextMeshProUGUI tmp = starText.AddComponent<TextMeshProUGUI>();
            tmp.text      = "★";
            tmp.fontSize  = size * 0.52f;
            tmp.color     = starColor;
            tmp.alignment = TextAlignmentOptions.Center;
            RectTransform starTextRT = starText.GetComponent<RectTransform>();
            starTextRT.anchorMin = starTextRT.anchorMax = new Vector2(0.5f, 0.5f);
            starTextRT.pivot     = new Vector2(0.5f, 0.5f);
            starTextRT.sizeDelta = new Vector2(size, size);
            starTextRT.anchoredPosition = Vector2.zero;
        }

        RectTransform frameRT = frame.GetComponent<RectTransform>();
        frameRT.anchorMin = frameRT.anchorMax = new Vector2(0.5f, 0.5f);
        frameRT.pivot     = new Vector2(0.5f, 0.5f);
        frameRT.sizeDelta = new Vector2(size, size);
        frameRT.anchoredPosition = pos;

        return frame;
    }

    static GameObject CreateTMPText(GameObject parent, string name, string text,
        TMP_FontAsset font, float fontSize, Color color, FontStyles style,
        Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
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

    static GameObject CreateButton(GameObject parent, string name, string label,
        TMP_FontAsset font, float fontSize, Color textColor, Color bgColor,
        Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        Image img = go.AddComponent<Image>();
        img.color = bgColor;
        Button btn = go.AddComponent<Button>();

        ColorBlock cb = btn.colors;
        cb.normalColor      = bgColor;
        cb.highlightedColor = new Color(
            Mathf.Min(bgColor.r + 0.15f, 1f),
            Mathf.Min(bgColor.g + 0.15f, 1f),
            Mathf.Min(bgColor.b + 0.15f, 1f), 1f);
        cb.pressedColor  = new Color(bgColor.r * 0.8f, bgColor.g * 0.8f, bgColor.b * 0.8f, 1f);
        cb.selectedColor = cb.normalColor;
        btn.colors = cb;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;

        // Label
        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(go.transform, false);
        TextMeshProUGUI tmp = labelGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        if (font != null) tmp.font = font;
        tmp.fontSize  = fontSize;
        tmp.color     = textColor;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
        RectTransform labelRT = labelGO.GetComponent<RectTransform>();
        labelRT.anchorMin = Vector2.zero;
        labelRT.anchorMax = Vector2.one;
        labelRT.offsetMin = labelRT.offsetMax = Vector2.zero;

        return go;
    }
}
