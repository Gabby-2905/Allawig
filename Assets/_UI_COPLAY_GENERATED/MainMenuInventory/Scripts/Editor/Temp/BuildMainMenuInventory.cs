using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using UnityEditor.SceneManagement;

public class BuildMainMenuInventory
{
    public static void Execute()
    {
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/BebasNeue-Regular SDF.asset");

        // Fix & load slot sprite
        string spritePath = "Assets/_UI_COPLAY_GENERATED/MainMenuInventory/Sprites/SlotBg.png";
        var imp = AssetImporter.GetAtPath(spritePath) as TextureImporter;
        if (imp != null)
        {
            imp.textureType      = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.mipmapEnabled    = false;
            AssetDatabase.ImportAsset(spritePath, ImportAssetOptions.ForceUpdate);
        }
        AssetDatabase.Refresh();
        Sprite slotSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

        // Colors matching reference
        Color panelBg    = new Color(0.05f, 0.05f, 0.10f, 0.88f);
        Color slotBg     = new Color(0.32f, 0.32f, 0.46f, 0.85f);
        Color titleColor = new Color(1f, 1f, 1f, 1f);
        Color numColor   = new Color(0.75f, 0.75f, 0.85f, 0.65f);

        // Find the Canvas
        var canvasGO = GameObject.Find("Canvas");
        if (canvasGO == null) { Debug.LogError("[BuildMainMenuInventory] Canvas not found!"); return; }

        // Remove existing InventoryPanel
        var existing = canvasGO.transform.Find("InventoryPanel");
        if (existing != null) Undo.DestroyObjectImmediate(existing.gameObject);

        // ── InventoryPanel ─────────────────────────────────────────────────
        GameObject panel = new GameObject("InventoryPanel");
        Undo.RegisterCreatedObjectUndo(panel, "Create InventoryPanel");
        panel.transform.SetParent(canvasGO.transform, false);

        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = panelBg;
        panelImg.type  = Image.Type.Simple;

        RectTransform panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin = panelRT.anchorMax = new Vector2(0.5f, 0.5f);
        panelRT.pivot     = new Vector2(0.5f, 0.5f);
        panelRT.sizeDelta = new Vector2(700, 420);
        panelRT.anchoredPosition = new Vector2(0, 0);

        // ── Title ──────────────────────────────────────────────────────────
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(panel.transform, false);
        TextMeshProUGUI titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text      = "INVENTORY";
        if (font != null) titleTMP.font = font;
        titleTMP.fontSize  = 42;
        titleTMP.color     = titleColor;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.characterSpacing = 4f;
        RectTransform titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = titleRT.anchorMax = new Vector2(0.5f, 1f);
        titleRT.pivot     = new Vector2(0.5f, 1f);
        titleRT.sizeDelta = new Vector2(640, 55);
        titleRT.anchoredPosition = new Vector2(0, -12);

        // ── 8 Slots: 4 cols x 2 rows ───────────────────────────────────────
        int   cols     = 4;
        int   rows     = 2;
        float slotW    = 148f;
        float slotH    = 148f;
        float gapX     = 10f;
        float gapY     = 10f;
        float gridW    = cols * slotW + (cols - 1) * gapX;  // 4*148 + 3*10 = 622
        float gridH    = rows * slotH + (rows - 1) * gapY;  // 2*148 + 1*10 = 306
        float startX   = -(gridW / 2f) + slotW / 2f;        // -311 + 74 = -237
        float startY   = -70f;                               // below title

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                int idx = r * cols + c + 1;
                float x = startX + c * (slotW + gapX);
                float y = startY - r * (slotH + gapY);

                GameObject slot = new GameObject("Slot" + (idx - 1));
                slot.transform.SetParent(panel.transform, false);
                Image slotImg = slot.AddComponent<Image>();
                if (slotSprite != null)
                {
                    slotImg.sprite = slotSprite;
                    slotImg.color  = Color.white;
                }
                else
                {
                    slotImg.color = slotBg;
                }
                slotImg.type = Image.Type.Simple;
                RectTransform slotRT = slot.GetComponent<RectTransform>();
                slotRT.anchorMin = slotRT.anchorMax = new Vector2(0.5f, 0.5f);
                slotRT.pivot     = new Vector2(0.5f, 0.5f);
                slotRT.sizeDelta = new Vector2(slotW, slotH);
                slotRT.anchoredPosition = new Vector2(x, y);

                // Slot number (top-left corner)
                GameObject numGO = new GameObject("SlotNum");
                numGO.transform.SetParent(slot.transform, false);
                TextMeshProUGUI numTMP = numGO.AddComponent<TextMeshProUGUI>();
                numTMP.text = idx.ToString();
                if (font != null) numTMP.font = font;
                numTMP.fontSize  = 16;
                numTMP.color     = numColor;
                numTMP.alignment = TextAlignmentOptions.TopLeft;
                RectTransform numRT = numGO.GetComponent<RectTransform>();
                numRT.anchorMin = Vector2.zero;
                numRT.anchorMax = Vector2.one;
                numRT.offsetMin = new Vector2(8, 8);
                numRT.offsetMax = new Vector2(-8, -8);

                // Item label (center, hidden by default)
                GameObject itemGO = new GameObject("ItemLabel");
                itemGO.transform.SetParent(slot.transform, false);
                TextMeshProUGUI itemTMP = itemGO.AddComponent<TextMeshProUGUI>();
                itemTMP.text = "";
                if (font != null) itemTMP.font = font;
                itemTMP.fontSize  = 14;
                itemTMP.color     = Color.white;
                itemTMP.alignment = TextAlignmentOptions.Center;
                RectTransform itemRT = itemGO.GetComponent<RectTransform>();
                itemRT.anchorMin = Vector2.zero;
                itemRT.anchorMax = Vector2.one;
                itemRT.offsetMin = itemRT.offsetMax = Vector2.zero;
            }
        }

        // Start hidden (toggled by the STASH button)
        panel.SetActive(false);

        EditorUtility.SetDirty(canvasGO);
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[BuildMainMenuInventory] InventoryPanel built in MainMenuScene!");
    }
}
