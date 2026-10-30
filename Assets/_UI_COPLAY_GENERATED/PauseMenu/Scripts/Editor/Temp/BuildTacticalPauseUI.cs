using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using UnityEditor.SceneManagement;

/// <summary>
/// Builds the tactical-style Pause Menu UI in the currently active scene.
/// Matches the reference: dark panel, hazard stripes, orange title, button rows.
/// Preserves all PauseMenuManager button wiring.
/// </summary>
public class BuildTacticalPauseUI
{
    const string SPRITE_PATH = "Assets/_UI_COPLAY_GENERATED/PauseMenu/Sprites/";

    public static void Execute()
    {
        BuildInCurrentScene();
    }

    [MenuItem("Tools/Build Tactical Pause UI (Active Scene)")]
    public static void BuildInCurrentScene()
    {
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/BebasNeue-Regular SDF.asset");

        // Fix sprite importers
        FixSpriteImporter(SPRITE_PATH + "PanelBg.png");
        FixSpriteImporter(SPRITE_PATH + "HazardStripe.png");
        FixSpriteImporter(SPRITE_PATH + "ButtonBg.png");
        AssetDatabase.Refresh();

        Sprite panelSprite   = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_PATH + "PanelBg.png");
        Sprite hazardSprite  = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_PATH + "HazardStripe.png");
        Sprite buttonSprite  = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_PATH + "ButtonBg.png");

        // Colors
        Color panelBg       = new Color(0.10f, 0.09f, 0.04f, 1f);
        Color overlayBg     = new Color(0f, 0f, 0f, 0.80f);
        Color orangeTitle   = new Color(1f, 0.65f, 0f, 1f);
        Color subtitleColor = new Color(1f, 0.65f, 0f, 0.75f);
        Color btnTextColor  = new Color(1f, 1f, 1f, 1f);
        Color btnBg         = new Color(0.12f, 0.11f, 0.06f, 1f);
        Color btnHighlight  = new Color(0.20f, 0.18f, 0.08f, 1f);
        Color btnPress      = new Color(0.08f, 0.07f, 0.03f, 1f);
        Color dimText       = new Color(0.7f, 0.55f, 0.1f, 0.6f);
        Color stripeColor   = new Color(1f, 0.60f, 0f, 1f);

        // Remove existing PauseCanvas
        var existing = GameObject.Find("PauseCanvas");
        if (existing != null) Undo.DestroyObjectImmediate(existing);

        // Root Canvas
        GameObject canvasGO = new GameObject("PauseCanvas");
        Undo.RegisterCreatedObjectUndo(canvasGO, "Create PauseCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 40;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // Full-screen overlay
        GameObject overlay = MakeImage("Overlay", canvasGO, overlayBg, null);
        StretchFull(overlay.GetComponent<RectTransform>());

        // Main panel — centered, portrait
        GameObject panel = MakeImage("PausePanel", overlay, panelBg, panelSprite);
        var panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin = panelRT.anchorMax = new Vector2(0.5f, 0.5f);
        panelRT.pivot     = new Vector2(0.5f, 0.5f);
        panelRT.sizeDelta = new Vector2(412, 695);
        panelRT.anchoredPosition = Vector2.zero;

        // Top hazard stripe
        MakeStripe("TopStripe", panel, new Vector2(0, 330), new Vector2(412, 18), hazardSprite, stripeColor);
        // Bottom hazard stripe
        MakeStripe("BotStripe", panel, new Vector2(0, -330), new Vector2(412, 18), hazardSprite, stripeColor);

        // Corner bolts
        MakeBolt("BoltTL", panel, new Vector2(-192, 322));
        MakeBolt("BoltTR", panel, new Vector2( 192, 322));
        MakeBolt("BoltBL", panel, new Vector2(-192, -322));
        MakeBolt("BoltBR", panel, new Vector2( 192, -322));

        // "SYSTEM PAUSED" title
        MakeTMP("TitleText", panel, "SYSTEM PAUSED", font, 52, orangeTitle, FontStyles.Bold,
            new Vector2(0, 270), new Vector2(380, 60));

        // Subtitle
        MakeTMP("SubtitleText", panel, "PRIMARY OBJECTIVE: SURVIVE", font, 18, subtitleColor, FontStyles.Normal,
            new Vector2(0, 222), new Vector2(380, 28));

        // Divider line under subtitle
        MakeDivider("Divider", panel, new Vector2(0, 200), new Vector2(380, 2), stripeColor);

        // Buttons — 6 buttons stacked
        string[] labels   = { "RESUME", "SAVE GAME", "SETTINGS", "CONTROLS", "MAIN MENU", "QUIT" };
        float    btnH     = 70f;
        float    btnGap   = 8f;
        float    startY   = 145f;

        GameObject[] btns = new GameObject[labels.Length];
        for (int i = 0; i < labels.Length; i++)
        {
            float y = startY - i * (btnH + btnGap);
            btns[i] = MakeButton("Btn_" + labels[i].Replace(" ", ""), panel, labels[i],
                font, 26, btnTextColor, btnBg, btnHighlight, btnPress,
                buttonSprite, new Vector2(0, y), new Vector2(380, btnH));
        }

        // Footer texts
        MakeTMP("SecIdText", panel, "SEC-ID: 884-A", font, 14, dimText, FontStyles.Normal,
            new Vector2(-130, -318), new Vector2(160, 22));
        MakeTMP("BuildText", panel, "BUILD v2.0.8", font, 14, dimText, FontStyles.Normal,
            new Vector2(130, -318), new Vector2(160, 22));

        // PauseMenuManager
        PauseMenuManager pmm = canvasGO.AddComponent<PauseMenuManager>();
        pmm.pauseMenuPanel = panel;

        // Wire buttons
        WireBtn(btns[0].GetComponent<Button>(), pmm, "Resume");
        WireBtn(btns[1].GetComponent<Button>(), pmm, "SaveGame");
        WireBtn(btns[2].GetComponent<Button>(), pmm, "OpenSettings");
        WireBtn(btns[3].GetComponent<Button>(), pmm, "OpenControls");
        WireBtn(btns[4].GetComponent<Button>(), pmm, "GoToMainMenu");
        WireBtn(btns[5].GetComponent<Button>(), pmm, "QuitToDesktop");

        // Canvas stays ACTIVE, panel starts hidden (PauseMenuManager.Awake hides it)
        canvasGO.SetActive(true);
        panel.SetActive(false);

        EditorUtility.SetDirty(canvasGO);
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[BuildTacticalPauseUI] Built in: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    static void FixSpriteImporter(string path)
    {
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp == null) return;
        imp.textureType     = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.mipmapEnabled   = false;
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
    }

    static GameObject MakeImage(string name, GameObject parent, Color color, Sprite sprite)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        img.type  = Image.Type.Simple;
        if (sprite != null) { img.sprite = sprite; img.color = Color.white; }
        return go;
    }

    static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static void MakeStripe(string name, GameObject parent, Vector2 pos, Vector2 size, Sprite sprite, Color fallback)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var img = go.AddComponent<Image>();
        if (sprite != null) { img.sprite = sprite; img.type = Image.Type.Tiled; img.color = Color.white; }
        else { img.color = fallback; img.type = Image.Type.Simple; }
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
    }

    static void MakeBolt(string name, GameObject parent, Vector2 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.35f, 0.28f, 0.10f, 1f);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(12, 12);
        rt.anchoredPosition = pos;
    }

    static void MakeDivider(string name, GameObject parent, Vector2 pos, Vector2 size, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
    }

    static GameObject MakeTMP(string name, GameObject parent, string text,
        TMP_FontAsset font, float size, Color color, FontStyles style, Vector2 pos, Vector2 sizeDelta)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        if (font != null) tmp.font = font;
        tmp.fontSize  = size;
        tmp.color     = color;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
        tmp.characterSpacing = 3f;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = sizeDelta;
        rt.anchoredPosition = pos;
        return go;
    }

    static GameObject MakeButton(string name, GameObject parent, string label,
        TMP_FontAsset font, float fontSize, Color textColor,
        Color normalColor, Color hoverColor, Color pressColor,
        Sprite sprite, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var img = go.AddComponent<Image>();
        if (sprite != null) { img.sprite = sprite; img.type = Image.Type.Sliced; img.color = Color.white; }
        else { img.color = normalColor; img.type = Image.Type.Simple; }
        var btn = go.AddComponent<Button>();
        var cb = btn.colors;
        cb.normalColor      = sprite != null ? Color.white : normalColor;
        cb.highlightedColor = hoverColor;
        cb.pressedColor     = pressColor;
        cb.selectedColor    = cb.normalColor;
        cb.fadeDuration     = 0.08f;
        btn.colors = cb;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;

        // Label
        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(go.transform, false);
        var tmp = labelGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        if (font != null) tmp.font = font;
        tmp.fontSize  = fontSize;
        tmp.color     = textColor;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
        tmp.characterSpacing = 4f;
        var lrt = labelGO.GetComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        return go;
    }

    static void WireBtn(Button btn, Object target, string method)
    {
        var m = target.GetType().GetMethod(method,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (m == null) { Debug.LogWarning("[BuildTacticalPauseUI] Method not found: " + method); return; }
        var action = (UnityEngine.Events.UnityAction)System.Delegate.CreateDelegate(
            typeof(UnityEngine.Events.UnityAction), target, m);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, action);
        EditorUtility.SetDirty(btn);
    }
}
