using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;

public class RethemePauseMenu
{
    // Main menu colors
    static readonly Color ColPanel      = new Color(0.08f, 0.12f, 0.15f, 0.97f); // dark navy bg
    static readonly Color ColTealBtn    = new Color(0.18f, 0.45f, 0.50f, 1.00f); // teal button
    static readonly Color ColTealHover  = new Color(0.22f, 0.55f, 0.62f, 1.00f); // teal hover
    static readonly Color ColTealPress  = new Color(0.12f, 0.32f, 0.36f, 1.00f); // teal pressed
    static readonly Color ColRedBtn     = new Color(0.60f, 0.08f, 0.08f, 1.00f); // red quit
    static readonly Color ColRedHover   = new Color(0.75f, 0.12f, 0.12f, 1.00f);
    static readonly Color ColRedPress   = new Color(0.45f, 0.05f, 0.05f, 1.00f);
    static readonly Color ColWhite      = Color.white;
    static readonly Color ColAccent     = new Color(0.18f, 0.65f, 0.70f, 1.00f); // bright teal accent

    public static void Execute()
    {
        // ---- Pause Panel ----
        StylePanel("PauseMenuCanvas/PausePanel");

        // Title box
        StyleImageBox("PauseMenuCanvas/PausePanel/TitleBox", ColPanel, ColAccent, 2f);
        StyleTMP("PauseMenuCanvas/PausePanel/TitleBox/TitleText", 32, FontStyles.Bold, ColWhite, true);

        // Regular buttons (teal)
        string[] tealBtns = { "ResumeButton", "SettingsButton", "ControlsButton", "SaveGameButton", "MainMenuButton" };
        foreach (var b in tealBtns)
            StyleButton("PauseMenuCanvas/PausePanel/" + b, ColTealBtn, ColTealHover, ColTealPress, ColWhite, 24, FontStyles.Bold, true);

        // Quit button (red)
        StyleButton("PauseMenuCanvas/PausePanel/QuitToDesktopButton", ColRedBtn, ColRedHover, ColRedPress, ColWhite, 18, FontStyles.Bold, true);

        // ---- Settings Panel ----
        StylePanel("PauseMenuCanvas/SettingsPanel");
        StyleImageBox("PauseMenuCanvas/SettingsPanel/TitleBox", ColPanel, ColAccent, 2f);
        StyleTMP("PauseMenuCanvas/SettingsPanel/TitleBox/TitleText", 32, FontStyles.Bold, ColWhite, true);
        StyleButton("PauseMenuCanvas/SettingsPanel/BackButton", ColTealBtn, ColTealHover, ColTealPress, ColWhite, 24, FontStyles.Bold, true);
        StyleSubPanelLines("PauseMenuCanvas/SettingsPanel");

        // ---- Controls Panel ----
        StylePanel("PauseMenuCanvas/ControlsPanel");
        StyleImageBox("PauseMenuCanvas/ControlsPanel/TitleBox", ColPanel, ColAccent, 2f);
        StyleTMP("PauseMenuCanvas/ControlsPanel/TitleBox/TitleText", 32, FontStyles.Bold, ColWhite, true);
        StyleButton("PauseMenuCanvas/ControlsPanel/BackButton", ColTealBtn, ColTealHover, ColTealPress, ColWhite, 24, FontStyles.Bold, true);
        StyleSubPanelLines("PauseMenuCanvas/ControlsPanel");

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        Debug.Log("[RethemePauseMenu] Done!");
    }

    static GameObject FindIncludingInactive(string path)
    {
        // Split path and traverse including inactive objects
        var parts = path.Split('/');
        // Find root (may be inactive)
        GameObject[] roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        GameObject current = null;
        foreach (var r in roots)
            if (r.name == parts[0]) { current = r; break; }
        if (current == null) return null;
        for (int i = 1; i < parts.Length; i++)
        {
            var t = current.transform.Find(parts[i]);
            if (t == null) return null;
            current = t.gameObject;
        }
        return current;
    }

    static void StylePanel(string path)
    {
        var go = FindIncludingInactive(path);
        if (go == null) { Debug.LogWarning("Not found: " + path); return; }

        var img = go.GetComponent<Image>();
        if (img != null) img.color = ColPanel;

        var ol = go.GetComponent<Outline>();
        if (ol != null)
        {
            ol.effectColor = ColAccent;
            ol.effectDistance = new Vector2(2f, -2f);
        }
    }

    static void StyleImageBox(string path, Color bgColor, Color outlineColor, float outlineSize)
    {
        var go = FindIncludingInactive(path);
        if (go == null) { Debug.LogWarning("Not found: " + path); return; }

        var img = go.GetComponent<Image>();
        if (img != null) img.color = bgColor;

        var ol = go.GetComponent<Outline>();
        if (ol != null)
        {
            ol.effectColor = outlineColor;
            ol.effectDistance = new Vector2(outlineSize, -outlineSize);
        }
    }

    static void StyleButton(string path, Color normal, Color hover, Color pressed, Color textColor, float fontSize, FontStyles fontStyle, bool letterSpacing)
    {
        var go = FindIncludingInactive(path);
        if (go == null) { Debug.LogWarning("Not found: " + path); return; }

        var img = go.GetComponent<Image>();
        if (img != null) img.color = normal;

        var btn = go.GetComponent<Button>();
        if (btn != null)
        {
            var cb = btn.colors;
            cb.normalColor    = Color.white;
            cb.highlightedColor = new Color(hover.r / normal.r, hover.g / normal.g, hover.b / normal.b, 1f);
            cb.pressedColor   = new Color(0.7f, 0.7f, 0.7f, 1f);
            cb.selectedColor  = Color.white;
            cb.colorMultiplier = 1f;
            btn.colors = cb;
        }

        var ol = go.GetComponent<Outline>();
        if (ol != null)
        {
            ol.effectColor = ColAccent;
            ol.effectDistance = new Vector2(1.5f, -1.5f);
        }

        // Style text child
        var tmp = go.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.color = textColor;
            tmp.fontSize = fontSize;
            tmp.fontStyle = fontStyle;
            if (letterSpacing) tmp.characterSpacing = 4f;
        }
    }

    static void StyleTMP(string path, float fontSize, FontStyles style, Color color, bool letterSpacing)
    {
        var go = FindIncludingInactive(path);
        if (go == null) { Debug.LogWarning("Not found: " + path); return; }
        var tmp = go.GetComponent<TextMeshProUGUI>();
        if (tmp == null) return;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = color;
        if (letterSpacing) tmp.characterSpacing = 4f;
    }

    static void StyleSubPanelLines(string panelPath)
    {
        var panel = FindIncludingInactive(panelPath);
        if (panel == null) return;
        foreach (Transform child in panel.transform)
        {
            if (!child.name.StartsWith("Line_")) continue;
            var tmp = child.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.color = new Color(0.75f, 0.85f, 0.87f, 1f); // light teal text
                tmp.fontSize = 20;
            }
        }
    }
}
