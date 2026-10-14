using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RebuildSprintMeterV2
{
    public static void Execute()
    {
        GameObject hudCanvas = GameObject.Find("HUDCanvas");
        if (hudCanvas == null) { Debug.LogError("HUDCanvas not found."); return; }

        // Remove old sprint meter
        Transform old = hudCanvas.transform.Find("SprintMeter");
        if (old != null) Object.DestroyImmediate(old.gameObject);

        // ── Root container ──────────────────────────────────────
        GameObject root = new GameObject("SprintMeter", typeof(RectTransform));
        root.transform.SetParent(hudCanvas.transform, false);
        CanvasGroup cg = root.AddComponent<CanvasGroup>();
        cg.alpha = 1f;

        RectTransform rootRT = root.GetComponent<RectTransform>();
        // Bottom-left, above the HP bar
        rootRT.anchorMin        = new Vector2(0f, 0f);
        rootRT.anchorMax        = new Vector2(0f, 0f);
        rootRT.pivot            = new Vector2(0f, 0f);
        rootRT.anchoredPosition = new Vector2(20f, 90f);
        rootRT.sizeDelta        = new Vector2(220f, 36f);

        // ── Label ───────────────────────────────────────────────
        GameObject labelGO = new GameObject("Label", typeof(RectTransform));
        labelGO.transform.SetParent(root.transform, false);
        TextMeshProUGUI label = labelGO.AddComponent<TextMeshProUGUI>();
        label.text      = "STAMINA  100%";
        label.fontSize  = 10f;
        label.fontStyle = FontStyles.Bold;
        label.color     = new Color(0.88f, 0.88f, 0.88f, 1f);
        label.alignment = TextAlignmentOptions.Left;
        RectTransform labelRT = labelGO.GetComponent<RectTransform>();
        labelRT.anchorMin        = new Vector2(0f, 1f);
        labelRT.anchorMax        = new Vector2(1f, 1f);
        labelRT.pivot            = new Vector2(0f, 1f);
        labelRT.anchoredPosition = new Vector2(0f, 0f);
        labelRT.sizeDelta        = new Vector2(0f, 14f);

        // ── Border (outer frame) ────────────────────────────────
        GameObject borderGO = new GameObject("Border", typeof(RectTransform));
        borderGO.transform.SetParent(root.transform, false);
        Image borderImg = borderGO.AddComponent<Image>();
        borderImg.color = new Color(1f, 1f, 1f, 0.35f);
        RectTransform borderRT = borderGO.GetComponent<RectTransform>();
        borderRT.anchorMin        = new Vector2(0f, 0f);
        borderRT.anchorMax        = new Vector2(1f, 0f);
        borderRT.pivot            = new Vector2(0f, 0f);
        borderRT.anchoredPosition = new Vector2(0f, 0f);
        borderRT.sizeDelta        = new Vector2(0f, 20f);

        // ── Background (dark fill) ──────────────────────────────
        GameObject bgGO = new GameObject("Background", typeof(RectTransform));
        bgGO.transform.SetParent(borderGO.transform, false);
        Image bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.08f, 0.08f, 0.08f, 0.85f);
        RectTransform bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = new Vector2(1f, 1f);
        bgRT.offsetMax = new Vector2(-1f, -1f);

        // ── Fill (the actual stamina bar) ───────────────────────
        GameObject fillGO = new GameObject("Fill", typeof(RectTransform));
        fillGO.transform.SetParent(bgGO.transform, false);
        Image fillImg = fillGO.AddComponent<Image>();
        fillImg.color      = new Color(0.18f, 0.82f, 0.34f, 1f);
        fillImg.type       = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        // Fill from LEFT → RIGHT, so as fillAmount decreases the bar shrinks from the right
        fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImg.fillAmount = 1f;
        RectTransform fillRT = fillGO.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = new Vector2(2f, 2f);
        fillRT.offsetMax = new Vector2(-2f, -2f);

        // ── Shine overlay ───────────────────────────────────────
        GameObject shineGO = new GameObject("Shine", typeof(RectTransform));
        shineGO.transform.SetParent(bgGO.transform, false);
        Image shineImg = shineGO.AddComponent<Image>();
        shineImg.color = new Color(1f, 1f, 1f, 0.07f);
        RectTransform shineRT = shineGO.GetComponent<RectTransform>();
        shineRT.anchorMin = new Vector2(0f, 0.5f);
        shineRT.anchorMax = new Vector2(1f, 1f);
        shineRT.offsetMin = new Vector2(2f, 0f);
        shineRT.offsetMax = new Vector2(-2f, -1f);

        // ── SprintMeterUI script ────────────────────────────────
        SprintMeterUI sprintUI = root.AddComponent<SprintMeterUI>();
        sprintUI.fillImage       = fillImg;
        sprintUI.backgroundImage = bgImg;
        sprintUI.borderImage     = borderImg;
        sprintUI.labelText       = label;
        sprintUI.canvasGroup     = cg;

        GamePlayerMovement player = Object.FindAnyObjectByType<GamePlayerMovement>();
        if (player != null) sprintUI.player = player;
        else Debug.LogWarning("GamePlayerMovement not found — assign manually.");

        EditorUtility.SetDirty(hudCanvas);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("Sprint meter v2 built and saved.");
    }
}
