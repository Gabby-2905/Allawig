using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;

public class RebuildMainMenuUI
{
    [MenuItem("Tools/Rebuild Main Menu UI")]
    public static void Execute()
    {
        var scene = EditorSceneManager.GetActiveScene();

        Canvas canvas = null;
        foreach (var go in scene.GetRootGameObjects())
        {
            canvas = go.GetComponent<Canvas>();
            if (canvas != null) break;
        }
        if (canvas == null) { Debug.LogError("No Canvas found!"); return; }

        GameObject canvasGO = canvas.gameObject;

        // Remove all children except Background
        var toRemove = new System.Collections.Generic.List<GameObject>();
        for (int i = 0; i < canvasGO.transform.childCount; i++)
        {
            var child = canvasGO.transform.GetChild(i).gameObject;
            if (child.name != "Background") toRemove.Add(child);
        }
        foreach (var c in toRemove) GameObject.DestroyImmediate(c);

        // Background stretches full canvas (1920x1080 reference)
        var bg = canvasGO.transform.Find("Background");
        if (bg != null)
        {
            var bgRT = bg.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;
        }

        // ── Right-side dark panel ─────────────────────────────────────────────
        GameObject rightDark = CreateUI("RightDarkPanel", canvasGO.transform);
        SetAnchors(rightDark, 0.63f, 0f, 1f, 1f, 0, 0, 0, 0);
        rightDark.AddComponent<Image>().color = new Color(0.04f, 0.07f, 0.11f, 0.88f);

        // ── Left dark overlay ─────────────────────────────────────────────────
        GameObject leftOverlay = CreateUI("LeftOverlay", canvasGO.transform);
        SetAnchors(leftOverlay, 0f, 0f, 0.70f, 1f, 0, 0, 0, 0);
        leftOverlay.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.28f);

        // ── "ALLAWIG" — top-left, fixed pixel size ────────────────────────────
        // Anchor top-left corner, use sizeDelta for size
        GameObject titleGO = CreateUI("TitleALLAWIG", canvasGO.transform);
        var titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0f, 1f);
        titleRT.anchorMax = new Vector2(0f, 1f);
        titleRT.pivot     = new Vector2(0f, 1f);
        titleRT.anchoredPosition = new Vector2(18f, -10f);
        titleRT.sizeDelta = new Vector2(700f, 130f);
        var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "ALLAWIG";
        titleTMP.fontSize = 110;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.color = Color.white;
        titleTMP.alignment = TextAlignmentOptions.BottomLeft;
        titleTMP.outlineWidth = 0.25f;
        titleTMP.outlineColor = new Color32(0, 0, 0, 200);

        // ── "PH EDITION" — top-right of ALLAWIG word ─────────────────────────
        // ALLAWIG at font 110 is roughly 580px wide. Place PH EDITION at x~590, y~-10
        GameObject phGO = CreateUI("PHEdition", canvasGO.transform);
        var phRT = phGO.GetComponent<RectTransform>();
        phRT.anchorMin = new Vector2(0f, 1f);
        phRT.anchorMax = new Vector2(0f, 1f);
        phRT.pivot     = new Vector2(0f, 1f);
        phRT.anchoredPosition = new Vector2(590f, -12f);
        phRT.sizeDelta = new Vector2(200f, 40f);
        var phTMP = phGO.AddComponent<TextMeshProUGUI>();
        phTMP.text = "PH EDITION";
        phTMP.fontSize = 18;
        phTMP.fontStyle = FontStyles.Bold;
        phTMP.color = new Color(0.80f, 0.80f, 0.80f, 1f);
        phTMP.alignment = TextAlignmentOptions.BottomLeft;
        phTMP.characterSpacing = 3f;

        // ── "LOOTER-SURVIVAL" subtitle ────────────────────────────────────────
        GameObject subGO = CreateUI("SubtitleLooter", canvasGO.transform);
        var subRT = subGO.GetComponent<RectTransform>();
        subRT.anchorMin = new Vector2(0f, 1f);
        subRT.anchorMax = new Vector2(0f, 1f);
        subRT.pivot     = new Vector2(0f, 1f);
        subRT.anchoredPosition = new Vector2(18f, -130f);
        subRT.sizeDelta = new Vector2(560f, 55f);
        var subTMP = subGO.AddComponent<TextMeshProUGUI>();
        subTMP.text = "LOOTER-SURVIVAL";
        subTMP.fontSize = 30;
        subTMP.fontStyle = FontStyles.Bold;
        subTMP.color = new Color(0.78f, 0.78f, 0.78f, 1f);
        subTMP.alignment = TextAlignmentOptions.TopLeft;
        subTMP.characterSpacing = 7f;

        // ── BUTTON PANEL ──────────────────────────────────────────────────────
        // Spans from just above player card (13%) to near top (97%)
        // childForceExpandHeight = true so buttons stretch to fill all available space
        GameObject btnPanel = CreateUI("ButtonPanel", canvasGO.transform);
        SetAnchors(btnPanel, 0.64f, 0.13f, 0.99f, 0.97f, 12, 0, -12, 0);
        var vlg = btnPanel.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 8f;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;
        vlg.childForceExpandHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.padding = new RectOffset(0, 0, 12, 12);

        MainMenuManager mmm = canvasGO.GetComponent<MainMenuManager>();

        var buttons = new (string label, bool isQuit)[]
        {
            ("START NEW GAME",    false),
            ("CONTINUE",          false),
            ("STASH / INVENTORY", false),
            ("SETTINGS",          false),
            ("QUIT",              true),
        };

        Color normalBg     = new Color(0.05f, 0.18f, 0.22f, 0.92f);
        Color normalBorder = new Color(0.18f, 0.65f, 0.70f, 1f);
        Color quitBg       = new Color(0.42f, 0.05f, 0.05f, 0.95f);
        Color quitBorder   = new Color(0.78f, 0.10f, 0.10f, 1f);

        foreach (var (label, isQuit) in buttons)
        {
            GameObject frame = CreateUI(label.Replace(" ", "_") + "_Btn", btnPanel.transform);
            var frameImg = frame.AddComponent<Image>();
            frameImg.color = isQuit ? quitBorder : normalBorder;

            GameObject fill = CreateUI("Fill", frame.transform);
            SetAnchors(fill, 0f, 0f, 1f, 1f, 2, 2, -2, -2);
            fill.AddComponent<Image>().color = isQuit ? quitBg : normalBg;

            var btn = frame.AddComponent<Button>();
            var bc = btn.colors;
            bc.normalColor      = Color.white;
            bc.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 0.9f);
            bc.pressedColor     = new Color(0.6f, 0.6f, 0.6f, 1f);
            btn.colors = bc;
            btn.targetGraphic = frameImg;

            GameObject lbl = CreateUI("Label", frame.transform);
            SetAnchors(lbl, 0f, 0f, 1f, 1f, 8, 0, -8, 0);
            var lTMP = lbl.AddComponent<TextMeshProUGUI>();
            lTMP.text = label;
            lTMP.fontSize = 22;
            lTMP.fontStyle = FontStyles.Bold;
            lTMP.color = Color.white;
            lTMP.alignment = TextAlignmentOptions.Center;
            lTMP.characterSpacing = 4f;

            if (mmm != null)
            {
                UnityEngine.Events.UnityAction action = label switch
                {
                    "START NEW GAME"    => mmm.StartNewGame,
                    "CONTINUE"          => mmm.ContinueGame,
                    "STASH / INVENTORY" => mmm.OpenInventory,
                    "SETTINGS"          => mmm.OpenSettings,
                    "QUIT"              => mmm.QuitGame,
                    _                   => null
                };
                if (action != null) { btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(action); }
            }
        }

        // ── SYSTEM STATUS LABEL ───────────────────────────────────────────────
        // Fixed pixel position: 90px from bottom, 22px tall
        GameObject sysLbl = CreateUI("SystemStatusLabel", canvasGO.transform);
        var sysRT = sysLbl.GetComponent<RectTransform>();
        sysRT.anchorMin = new Vector2(0.64f, 0f);
        sysRT.anchorMax = new Vector2(0.99f, 0f);
        sysRT.pivot     = new Vector2(0.5f, 0f);
        sysRT.anchoredPosition = new Vector2(0f, 96f);
        sysRT.sizeDelta = new Vector2(-22f, 22f);
        var sysLblTMP = sysLbl.AddComponent<TextMeshProUGUI>();
        sysLblTMP.text = "SYSTEM STATUS: <color=#FF3333>CRITICAL</color>";
        sysLblTMP.fontSize = 14;
        sysLblTMP.fontStyle = FontStyles.Bold;
        sysLblTMP.color = new Color(0.80f, 0.80f, 0.80f, 1f);
        sysLblTMP.alignment = TextAlignmentOptions.Right;
        sysLblTMP.characterSpacing = 2f;

        // ── STATUS BAR ────────────────────────────────────────────────────────
        // Fixed pixel position: 70px from bottom, 14px tall
        GameObject barBG = CreateUI("StatusBarBG", canvasGO.transform);
        var barBGRT = barBG.GetComponent<RectTransform>();
        barBGRT.anchorMin = new Vector2(0.64f, 0f);
        barBGRT.anchorMax = new Vector2(0.99f, 0f);
        barBGRT.pivot     = new Vector2(0.5f, 0f);
        barBGRT.anchoredPosition = new Vector2(0f, 74f);
        barBGRT.sizeDelta = new Vector2(-22f, 14f);
        barBG.AddComponent<Image>().color = new Color(0.08f, 0.08f, 0.08f, 0.9f);

        GameObject barFill = CreateUI("StatusBarFill", barBG.transform);
        SetAnchors(barFill, 0f, 0f, 0.25f, 1f, 1, 1, -1, -1);
        barFill.AddComponent<Image>().color = new Color(0.82f, 0.08f, 0.08f, 1f);

        // ── PLAYER CARD ───────────────────────────────────────────────────────
        // Fixed pixel height of 58px anchored to bottom of right panel
        GameObject card = CreateUI("PlayerCard", canvasGO.transform);
        var cardRT = card.GetComponent<RectTransform>();
        cardRT.anchorMin = new Vector2(0.64f, 0f);
        cardRT.anchorMax = new Vector2(0.99f, 0f);
        cardRT.pivot     = new Vector2(0.5f, 0f);
        cardRT.anchoredPosition = new Vector2(0f, 4f);
        cardRT.sizeDelta = new Vector2(-22f, 62f);
        card.AddComponent<Image>().color = new Color(0.05f, 0.08f, 0.13f, 0.95f);

        // Top teal border
        GameObject cardBorderTop = CreateUI("CardBorderTop", card.transform);
        SetAnchors(cardBorderTop, 0f, 0.88f, 1f, 1f, 0, 0, 0, 0);
        cardBorderTop.AddComponent<Image>().color = new Color(0.18f, 0.65f, 0.70f, 1f);

        // Avatar
        GameObject avatar = CreateUI("Avatar", card.transform);
        SetAnchors(avatar, 0f, 0f, 0.09f, 0.88f, 3, 2, -2, -2);
        avatar.AddComponent<Image>().color = new Color(0.25f, 0.28f, 0.32f, 1f);

        // Player name
        GameObject pName = CreateUI("PlayerName", card.transform);
        SetAnchors(pName, 0.10f, 0.48f, 0.52f, 0.88f, 0, 0, 0, 0);
        var pNameTMP = pName.AddComponent<TextMeshProUGUI>();
        pNameTMP.text = "PLAYER";
        pNameTMP.fontSize = 14;
        pNameTMP.fontStyle = FontStyles.Bold;
        pNameTMP.color = Color.white;
        pNameTMP.alignment = TextAlignmentOptions.BottomLeft;

        // Player ID
        GameObject pID = CreateUI("PlayerID", card.transform);
        SetAnchors(pID, 0.10f, 0.05f, 0.52f, 0.50f, 0, 0, 0, 0);
        var pIDTMP = pID.AddComponent<TextMeshProUGUI>();
        pIDTMP.text = "PLAYER ID: <color=#00CCDD>KAIBIGAN</color>";
        pIDTMP.fontSize = 11;
        pIDTMP.color = new Color(0.70f, 0.70f, 0.70f, 1f);
        pIDTMP.alignment = TextAlignmentOptions.TopLeft;

        // Stats
        GameObject pStats = CreateUI("PlayerStats", card.transform);
        SetAnchors(pStats, 0.52f, 0.05f, 0.92f, 0.88f, 0, 0, 0, 0);
        var pStatsTMP = pStats.AddComponent<TextMeshProUGUI>();
        pStatsTMP.text = "GAS SCRAP: <color=#FFDD44>25L</color>  |  AGIMAT: <color=#FFDD44>12</color>";
        pStatsTMP.fontSize = 11;
        pStatsTMP.color = new Color(0.70f, 0.70f, 0.70f, 1f);
        pStatsTMP.alignment = TextAlignmentOptions.MidlineRight;

        // Diamond icon
        GameObject diamond = CreateUI("DiamondIcon", card.transform);
        SetAnchors(diamond, 0.92f, 0.15f, 1f, 0.85f, 0, 0, -3, 0);
        diamond.AddComponent<Image>().color = new Color(0.18f, 0.65f, 0.70f, 1f);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[RebuildMainMenuUI] Done!");
    }

    static GameObject CreateUI(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    static void SetAnchors(GameObject go, float ax, float ay, float bx, float by,
                           float left, float bottom, float right, float top)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(ax, ay);
        rt.anchorMax = new Vector2(bx, by);
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(right, top);
    }
}
