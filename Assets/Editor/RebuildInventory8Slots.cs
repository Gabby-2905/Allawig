using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Rebuilds the InventoryCanvas with 8 slots arranged in 2 rows of 4.
/// </summary>
public class RebuildInventory8Slots
{
    const string SCENE_PATH = "Assets/Scenes/GameScene.unity";
    const string FONT_GUID  = "8f586378b4e144a9851e7b34d9b748ee";

    [MenuItem("Tools/Rebuild Inventory 8 Slots")]
    public static void Execute()
    {
        var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
        SceneManager.SetActiveScene(scene);

        // ── Remove old InventoryCanvas ────────────────────────────────────
        var oldInv = GameObject.Find("InventoryCanvas");
        if (oldInv != null) Object.DestroyImmediate(oldInv);

        // ── Create new InventoryCanvas ────────────────────────────────────
        var invCanvasGO = new GameObject("InventoryCanvas");
        EditorSceneManager.MoveGameObjectToScene(invCanvasGO, scene);
        var invCanvas = invCanvasGO.AddComponent<Canvas>();
        invCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        invCanvas.sortingOrder = 5;
        var scaler = invCanvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        invCanvasGO.AddComponent<GraphicRaycaster>();

        // ── Inventory root panel (hidden by default) ──────────────────────
        var invRoot = MakePanel(invCanvasGO.transform, "InventoryRoot",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(560f, 280f),
            new Color(0.05f, 0.05f, 0.1f, 0.92f));
        invRoot.SetActive(false);

        // Title
        MakeText(invRoot.transform, "InventoryTitle",
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -12f), new Vector2(0f, -48f),
            "INVENTORY  [TAB]", 26, TextAlignmentOptions.Center);

        // ── 8 slots: 2 rows × 4 columns ──────────────────────────────────
        var slots  = new Image[8];
        var labels = new TextMeshProUGUI[8];

        float slotSize   = 100f;
        float slotSpacing = 16f;
        float totalW = 4 * slotSize + 3 * slotSpacing; // 448
        float startX = -totalW / 2f + slotSize / 2f;   // left edge of first slot
        float row0Y  = -20f;   // center Y of top row
        float row1Y  = -136f;  // center Y of bottom row

        for (int i = 0; i < 8; i++)
        {
            int col = i % 4;
            int row = i / 4;
            float x = startX + col * (slotSize + slotSpacing);
            float y = row == 0 ? row0Y : row1Y;

            var slotGO = MakePanel(invRoot.transform, $"Slot{i}",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(x, y), new Vector2(slotSize, slotSize),
                new Color(0.12f, 0.12f, 0.18f, 0.9f));

            // Slot border
            var border = MakePanel(slotGO.transform, "Border",
                Vector2.zero, Vector2.one, Vector2.zero, new Vector2(4f, 4f),
                new Color(0f, 0f, 0f, 0f));
            border.GetComponent<Image>().color = new Color(0.4f, 0.4f, 0.6f, 0.6f);
            var borderRT = border.GetComponent<RectTransform>();
            borderRT.anchorMin = Vector2.zero;
            borderRT.anchorMax = Vector2.one;
            borderRT.offsetMin = new Vector2(-2f, -2f);
            borderRT.offsetMax = new Vector2(2f, 2f);

            // Slot number label (top-left corner)
            var numLabel = MakeText(slotGO.transform, "SlotNum",
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(6f, -4f), new Vector2(20f, 20f),
                (i + 1).ToString(), 12, TextAlignmentOptions.Left);
            numLabel.color = new Color(1f, 1f, 1f, 0.4f);

            // Item label (center)
            labels[i] = MakeText(slotGO.transform, "ItemLabel",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                "", 14, TextAlignmentOptions.Center);

            slots[i] = slotGO.GetComponent<Image>();
        }

        // ── InventoryUI script ────────────────────────────────────────────
        var invUI = invCanvasGO.AddComponent<InventoryUI>();
        invUI.inventoryRoot = invRoot;

        // ── Wire up GameHUD ───────────────────────────────────────────────
        var hudCanvas = GameObject.Find("HUDCanvas");
        if (hudCanvas != null)
        {
            var hud = hudCanvas.GetComponent<GameHUD>();
            if (hud != null)
            {
                hud.inventorySlots  = slots;
                hud.inventoryLabels = labels;
                EditorUtility.SetDirty(hud);
            }
        }

        // ── Save ──────────────────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, SCENE_PATH);
        Debug.Log("[RebuildInventory] 8-slot inventory rebuilt and saved.");
    }

    static GameObject MakePanel(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 anchoredPos, Vector2 sizeDelta, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = anchorMin;
        rt.anchorMax        = anchorMax;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta        = sizeDelta;
        rt.pivot            = new Vector2(0.5f, 0.5f);
        var img = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    static TextMeshProUGUI MakeText(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 anchoredPos, Vector2 sizeDelta,
        string text, int fontSize, TextAlignmentOptions align)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = anchorMin;
        rt.anchorMax        = anchorMax;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta        = sizeDelta;
        rt.pivot            = new Vector2(0.5f, 0.5f);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.alignment = align;
        tmp.color     = Color.white;

        string fontPath = AssetDatabase.GUIDToAssetPath(FONT_GUID);
        if (!string.IsNullOrEmpty(fontPath))
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
            if (font != null) tmp.font = font;
        }
        return tmp;
    }
}
