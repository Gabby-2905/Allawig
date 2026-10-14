using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildSprintMeterUI
{
    public static void Execute()
    {
        // Find the HUDCanvas
        GameObject hudCanvas = GameObject.Find("HUDCanvas");
        if (hudCanvas == null)
        {
            Debug.LogError("HUDCanvas not found in scene.");
            return;
        }

        // Remove existing sprint meter if any
        Transform existing = hudCanvas.transform.Find("SprintMeter");
        if (existing != null)
            Object.DestroyImmediate(existing.gameObject);

        // --- Sprint Meter Container ---
        GameObject container = new GameObject("SprintMeter", typeof(RectTransform));
        container.transform.SetParent(hudCanvas.transform, false);

        CanvasGroup cg = container.AddComponent<CanvasGroup>();

        RectTransform containerRT = container.GetComponent<RectTransform>();
        // Bottom-left corner, above health bar area
        containerRT.anchorMin = new Vector2(0f, 0f);
        containerRT.anchorMax = new Vector2(0f, 0f);
        containerRT.pivot     = new Vector2(0f, 0f);
        containerRT.anchoredPosition = new Vector2(20f, 80f);
        containerRT.sizeDelta = new Vector2(200f, 28f);

        // --- Label ---
        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(container.transform, false);
        TextMeshProUGUI label = labelGO.AddComponent<TextMeshProUGUI>();
        label.text = "SPRINT";
        label.fontSize = 11f;
        label.fontStyle = FontStyles.Bold;
        label.color = new Color(0.9f, 0.9f, 0.9f, 1f);
        label.alignment = TextAlignmentOptions.Left;
        RectTransform labelRT = labelGO.GetComponent<RectTransform>();
        labelRT.anchorMin = new Vector2(0f, 1f);
        labelRT.anchorMax = new Vector2(1f, 1f);
        labelRT.pivot     = new Vector2(0f, 0f);
        labelRT.anchoredPosition = new Vector2(0f, 2f);
        labelRT.sizeDelta = new Vector2(0f, 14f);

        // --- Background bar ---
        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(container.transform, false);
        Image bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.75f);
        RectTransform bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0f, 0f);
        bgRT.anchorMax = new Vector2(1f, 0f);
        bgRT.pivot     = new Vector2(0f, 0f);
        bgRT.anchoredPosition = new Vector2(0f, 0f);
        bgRT.sizeDelta = new Vector2(0f, 16f);

        // --- Fill bar ---
        GameObject fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(bgGO.transform, false);
        Image fillImg = fillGO.AddComponent<Image>();
        fillImg.color = new Color(0.2f, 0.85f, 0.3f, 1f);
        fillImg.type  = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillAmount = 1f;
        RectTransform fillRT = fillGO.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = new Vector2(2f, 2f);
        fillRT.offsetMax = new Vector2(-2f, -2f);

        // --- SprintMeterUI script ---
        SprintMeterUI sprintUI = container.AddComponent<SprintMeterUI>();
        sprintUI.fillImage       = fillImg;
        sprintUI.backgroundImage = bgImg;
        sprintUI.canvasGroup     = cg;

        // Auto-find player
        GamePlayerMovement player = Object.FindFirstObjectByType<GamePlayerMovement>();
        if (player != null)
            sprintUI.player = player;
        else
            Debug.LogWarning("GamePlayerMovement not found — assign manually.");

        EditorUtility.SetDirty(hudCanvas);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("Sprint meter UI built and saved.");
    }
}
