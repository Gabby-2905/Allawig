using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

/// <summary>
/// Fixes the Main Menu:
///  1. Wires slider OnValueChanged → SettingsManager methods (persistent listeners)
///  2. Creates an Inventory panel that shows saved items
///  3. Assigns the inventory panel to MainMenuManager
/// </summary>
public class FixMainMenuFunctionality
{
    [MenuItem("Tools/Fix Main Menu Functionality")]
    public static void Execute()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainMenuScene.unity",
                                                  OpenSceneMode.Single);

        GameObject canvasGO = GameObject.Find("Canvas");
        if (canvasGO == null) { Debug.LogError("Canvas not found!"); return; }

        MainMenuManager mmm = canvasGO.GetComponent<MainMenuManager>();
        if (mmm == null) { Debug.LogError("MainMenuManager not found on Canvas!"); return; }

        // ── 1. Wire Sliders ────────────────────────────────────────────────
        GameObject settingsPanelGO = canvasGO.transform.Find("SettingsPanel")?.gameObject;
        if (settingsPanelGO != null)
        {
            SettingsManager sm = settingsPanelGO.GetComponent<SettingsManager>();
            if (sm != null)
            {
                WireSliderPersistent(sm.sensitivitySlider,  sm, "OnSensitivityChanged");
                WireSliderPersistent(sm.musicVolumeSlider,  sm, "OnMusicVolumeChanged");
                WireSliderPersistent(sm.effectsVolumeSlider, sm, "OnEffectsVolumeChanged");
                Debug.Log("Sliders wired to SettingsManager.");
            }
        }

        // ── 2. Create or find Inventory Panel ─────────────────────────────
        GameObject invPanel = canvasGO.transform.Find("InventoryPanel")?.gameObject;
        if (invPanel == null)
        {
            invPanel = BuildInventoryPanel(canvasGO, mmm);
            Debug.Log("Inventory panel created.");
        }

        // ── 3. Assign to MainMenuManager via SerializedObject ──────────────
        SerializedObject so = new SerializedObject(mmm);
        so.FindProperty("inventoryPanel").objectReferenceValue = invPanel;

        // Assign slot images
        SerializedProperty imgArr = so.FindProperty("invSlotImages");
        imgArr.arraySize = 4;
        SerializedProperty lblArr = so.FindProperty("invSlotLabels");
        lblArr.arraySize = 4;

        for (int i = 0; i < 4; i++)
        {
            Transform slot = invPanel.transform.Find($"Slot{i}");
            if (slot != null)
            {
                imgArr.GetArrayElementAtIndex(i).objectReferenceValue = slot.GetComponent<Image>();
                Transform itemLbl = slot.Find("ItemLabel");
                if (itemLbl != null)
                    lblArr.GetArrayElementAtIndex(i).objectReferenceValue =
                        itemLbl.GetComponent<TextMeshProUGUI>();
            }
        }

        Transform emptyLbl = invPanel.transform.Find("EmptyLabel");
        if (emptyLbl != null)
            so.FindProperty("invEmptyLabel").objectReferenceValue =
                emptyLbl.GetComponent<TextMeshProUGUI>();

        so.ApplyModifiedProperties();

        // ── 4. Wire STASH/INVENTORY button ────────────────────────────────
        Transform stashBtn = canvasGO.transform.Find("ButtonPanel/STASH_/_INVENTORY_Btn");
        if (stashBtn != null)
        {
            Button btn = stashBtn.GetComponent<Button>();
            if (btn != null)
            {
                AddPersistentVoidListener(btn.gameObject, "m_OnClick", mmm, "OpenInventory");
                Debug.Log("STASH/INVENTORY button wired to OpenInventory.");
            }
        }

        // ── 5. Save scene ──────────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Main menu functionality fixed and scene saved.");
    }

    // ── Wire a Slider's onValueChanged with a persistent float listener ────

    static void WireSliderPersistent(Slider slider, SettingsManager sm, string methodName)
    {
        if (slider == null || sm == null) return;

        SerializedObject sliderSO = new SerializedObject(slider);
        SerializedProperty onValueChanged = sliderSO.FindProperty("m_OnValueChanged");

        // Clear existing persistent calls
        SerializedProperty calls = onValueChanged.FindPropertyRelative("m_PersistentCalls.m_Calls");
        calls.ClearArray();

        // Add one persistent call
        calls.arraySize = 1;
        SerializedProperty call = calls.GetArrayElementAtIndex(0);

        call.FindPropertyRelative("m_Target").objectReferenceValue = sm;
        call.FindPropertyRelative("m_TargetAssemblyTypeName").stringValue =
            $"SettingsManager, Assembly-CSharp";
        call.FindPropertyRelative("m_MethodName").stringValue = methodName;
        call.FindPropertyRelative("m_Mode").intValue = 4; // Float
        call.FindPropertyRelative("m_CallState").intValue = 2; // RuntimeOnly

        sliderSO.ApplyModifiedProperties();
    }

    // ── Add a persistent void listener to a Button's onClick ──────────────

    static void AddPersistentVoidListener(GameObject go, string eventPropName,
                                           Object target, string methodName)
    {
        // Find the component that owns the event
        Component[] comps = go.GetComponents<Component>();
        foreach (var comp in comps)
        {
            SerializedObject compSO = new SerializedObject(comp);
            SerializedProperty evtProp = compSO.FindProperty(eventPropName);
            if (evtProp == null) continue;

            SerializedProperty calls = evtProp.FindPropertyRelative("m_PersistentCalls.m_Calls");
            if (calls == null) continue;

            // Check if already wired
            for (int i = 0; i < calls.arraySize; i++)
            {
                var existing = calls.GetArrayElementAtIndex(i);
                if (existing.FindPropertyRelative("m_MethodName").stringValue == methodName)
                    return; // already wired
            }

            int idx = calls.arraySize;
            calls.arraySize = idx + 1;
            SerializedProperty call = calls.GetArrayElementAtIndex(idx);

            call.FindPropertyRelative("m_Target").objectReferenceValue = target;
            call.FindPropertyRelative("m_TargetAssemblyTypeName").stringValue =
                $"{target.GetType().Name}, Assembly-CSharp";
            call.FindPropertyRelative("m_MethodName").stringValue = methodName;
            call.FindPropertyRelative("m_Mode").intValue = 1; // Void
            call.FindPropertyRelative("m_CallState").intValue = 2; // RuntimeOnly

            compSO.ApplyModifiedProperties();
            return;
        }
    }

    // ── Build Inventory Panel ──────────────────────────────────────────────

    static GameObject BuildInventoryPanel(GameObject canvas, MainMenuManager mmm)
    {
        TMP_FontAsset font = null;
        var existingTMP = canvas.GetComponentInChildren<TextMeshProUGUI>(true);
        if (existingTMP != null) font = existingTMP.font;

        // Panel root
        GameObject panel = new GameObject("InventoryPanel");
        panel.transform.SetParent(canvas.transform, false);
        RectTransform panelRT = panel.AddComponent<RectTransform>();
        panelRT.anchorMin        = new Vector2(0.5f, 0.5f);
        panelRT.anchorMax        = new Vector2(0.5f, 0.5f);
        panelRT.sizeDelta        = new Vector2(700f, 520f);
        panelRT.anchoredPosition = Vector2.zero;
        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.05f, 0.05f, 0.08f, 0.93f);

        // Title
        CreateTMP(panel, "Title", "INVENTORY", 28f, FontStyles.Bold,
            new Vector2(0f, -20f), new Vector2(0f, 50f),
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
            new Color(0.9f, 0.85f, 0.6f, 1f), TextAlignmentOptions.Center, font);

        // Divider
        GameObject divGO = new GameObject("Divider");
        divGO.transform.SetParent(panel.transform, false);
        RectTransform divRT = divGO.AddComponent<RectTransform>();
        divRT.anchorMin = new Vector2(0.05f, 1f); divRT.anchorMax = new Vector2(0.95f, 1f);
        divRT.pivot = new Vector2(0.5f, 1f);
        divRT.anchoredPosition = new Vector2(0f, -75f); divRT.sizeDelta = new Vector2(0f, 2f);
        divGO.AddComponent<Image>().color = new Color(0.9f, 0.85f, 0.6f, 0.4f);

        // 4 inventory slots
        float slotW = 110f, slotH = 140f, spacing = 20f;
        float totalW = 4 * slotW + 3 * spacing;
        float startX = -totalW / 2f + slotW / 2f;

        for (int i = 0; i < 4; i++)
        {
            GameObject slotGO = new GameObject($"Slot{i}");
            slotGO.transform.SetParent(panel.transform, false);
            RectTransform slotRT = slotGO.AddComponent<RectTransform>();
            slotRT.anchorMin = new Vector2(0.5f, 0.5f);
            slotRT.anchorMax = new Vector2(0.5f, 0.5f);
            slotRT.sizeDelta = new Vector2(slotW, slotH);
            slotRT.anchoredPosition = new Vector2(startX + i * (slotW + spacing), 20f);
            slotGO.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.5f);

            // Slot number
            CreateTMP(slotGO, "SlotNum", $"Slot {i + 1}", 12f, FontStyles.Normal,
                new Vector2(0f, 0f), new Vector2(0f, 22f),
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Color(0.7f, 0.7f, 0.7f, 1f), TextAlignmentOptions.Center, font);

            // Item label
            CreateTMP(slotGO, "ItemLabel", "Empty", 13f, FontStyles.Normal,
                new Vector2(0f, 0f), new Vector2(0f, 28f),
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f),
                new Color(0.85f, 0.85f, 0.85f, 1f), TextAlignmentOptions.Center, font);
        }

        // "No items" label
        CreateTMP(panel, "EmptyLabel", "No items in inventory", 18f, FontStyles.Italic,
            new Vector2(0f, 0f), new Vector2(0f, 40f),
            new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f),
            new Color(0.6f, 0.6f, 0.6f, 1f), TextAlignmentOptions.Center, font);

        // Close button
        GameObject closeBtnGO = new GameObject("CloseBtn");
        closeBtnGO.transform.SetParent(panel.transform, false);
        RectTransform closeBtnRT = closeBtnGO.AddComponent<RectTransform>();
        closeBtnRT.anchorMin = new Vector2(0.5f, 0f); closeBtnRT.anchorMax = new Vector2(0.5f, 0f);
        closeBtnRT.pivot = new Vector2(0.5f, 0f);
        closeBtnRT.anchoredPosition = new Vector2(0f, 20f); closeBtnRT.sizeDelta = new Vector2(160f, 44f);
        closeBtnGO.AddComponent<Image>().color = new Color(0.18f, 0.45f, 0.5f, 1f);
        closeBtnGO.AddComponent<Button>();

        CreateTMP(closeBtnGO, "Label", "CLOSE", 16f, FontStyles.Bold,
            Vector2.zero, Vector2.zero,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Color.white, TextAlignmentOptions.Center, font);

        // Wire close button
        if (mmm != null)
            AddPersistentVoidListener(closeBtnGO, "m_OnClick", mmm, "CloseAllPanels");

        panel.SetActive(false);
        return panel;
    }

    // ── TMP helper ─────────────────────────────────────────────────────────

    static TextMeshProUGUI CreateTMP(GameObject parent, string name, string text,
        float fontSize, FontStyles style,
        Vector2 anchoredPos, Vector2 sizeDelta,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Color color, TextAlignmentOptions alignment, TMP_FontAsset font)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos; rt.sizeDelta = sizeDelta;
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = fontSize; tmp.fontStyle = style;
        tmp.alignment = alignment; tmp.color = color;
        if (font != null) tmp.font = font;
        return tmp;
    }
}
