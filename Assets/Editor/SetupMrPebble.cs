using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine.AI;

public class SetupMrPebble
{
    [MenuItem("Tools/Setup Mr Pebble Boss")]
    public static void Execute()
    {
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/BebasNeue-Regular SDF.asset");

        // ── 1. Find Mr Pebble ──────────────────────────────────────────────
        var boss = GameObject.Find("Mr Pebble");
        if (boss == null) { Debug.LogError("[SetupMrPebble] 'Mr Pebble' not found!"); return; }

        // Fix rotation (was 270 on X — reset to upright)
        boss.transform.rotation = Quaternion.identity;
        boss.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

        // ── 2. Add Collider ────────────────────────────────────────────────
        var col = boss.GetComponent<CapsuleCollider>();
        if (col == null) col = boss.AddComponent<CapsuleCollider>();
        col.height = 4f;
        col.radius = 1f;
        col.center = new Vector3(0, 2f, 0);
        col.isTrigger = false;

        // ── 3. Add NavMeshAgent ────────────────────────────────────────────
        var agent = boss.GetComponent<NavMeshAgent>();
        if (agent == null) agent = boss.AddComponent<NavMeshAgent>();
        agent.height        = 4f;
        agent.radius        = 1f;
        agent.speed         = 4f;
        agent.angularSpeed  = 180f;
        agent.acceleration  = 8f;
        agent.stoppingDistance = 3f;

        // ── 4. Add MrPebbleBoss script ─────────────────────────────────────
        var bossScript = boss.GetComponent<MrPebbleBoss>();
        if (bossScript == null) bossScript = boss.AddComponent<MrPebbleBoss>();
        bossScript.maxHealth       = 500f;
        bossScript.detectionRange  = 25f;
        bossScript.attackRange     = 5f;
        bossScript.slamDamage      = 35f;
        bossScript.slamRadius      = 5f;
        bossScript.slamCooldown    = 5f;
        bossScript.chargeDamage    = 25f;
        bossScript.chargeSpeed     = 18f;
        bossScript.chargeCooldown  = 7f;
        bossScript.chargeDistance  = 15f;
        bossScript.vulnerableDuration = 3f;
        bossScript.walkSpeed       = 3.5f;
        bossScript.chaseSpeed      = 6f;

        // ── 5. Boss Health Bar UI on HUDCanvas ─────────────────────────────
        var hudCanvas = GameObject.Find("HUDCanvas");
        if (hudCanvas != null)
        {
            // Remove old if exists
            var oldBar = hudCanvas.transform.Find("BossHealthBar");
            if (oldBar != null) Undo.DestroyObjectImmediate(oldBar.gameObject);

            // Panel
            var panelGO = new GameObject("BossHealthBar");
            panelGO.transform.SetParent(hudCanvas.transform, false);
            var panelImg = panelGO.AddComponent<Image>();
            panelImg.color = new Color(0.05f, 0.05f, 0.05f, 0.85f);
            var panelRT = panelGO.GetComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.2f, 0f);
            panelRT.anchorMax = new Vector2(0.8f, 0f);
            panelRT.pivot     = new Vector2(0.5f, 0f);
            panelRT.sizeDelta = new Vector2(0, 80);
            panelRT.anchoredPosition = new Vector2(0, 20);

            // Boss name
            var nameGO = new GameObject("BossName");
            nameGO.transform.SetParent(panelGO.transform, false);
            var nameTMP = nameGO.AddComponent<TextMeshProUGUI>();
            nameTMP.text      = "MR. PEBBLE";
            if (font != null) nameTMP.font = font;
            nameTMP.fontSize  = 22;
            nameTMP.color     = new Color(1f, 0.6f, 0.1f, 1f);
            nameTMP.fontStyle = FontStyles.Bold;
            nameTMP.alignment = TextAlignmentOptions.Center;
            var nameRT = nameGO.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 0.6f);
            nameRT.anchorMax = new Vector2(1f, 1f);
            nameRT.offsetMin = new Vector2(10, 0);
            nameRT.offsetMax = new Vector2(-10, 0);

            // Health bar background
            var barBgGO = new GameObject("HealthBarBg");
            barBgGO.transform.SetParent(panelGO.transform, false);
            var barBgImg = barBgGO.AddComponent<Image>();
            barBgImg.color = new Color(0.15f, 0.05f, 0.05f, 1f);
            var barBgRT = barBgGO.GetComponent<RectTransform>();
            barBgRT.anchorMin = new Vector2(0.02f, 0.1f);
            barBgRT.anchorMax = new Vector2(0.98f, 0.55f);
            barBgRT.offsetMin = barBgRT.offsetMax = Vector2.zero;

            // Health bar fill
            var barFillGO = new GameObject("HealthBarFill");
            barFillGO.transform.SetParent(barBgGO.transform, false);
            var barFillImg = barFillGO.AddComponent<Image>();
            barFillImg.color    = new Color(0.2f, 0.9f, 0.2f, 1f);
            barFillImg.type     = Image.Type.Filled;
            barFillImg.fillMethod = Image.FillMethod.Horizontal;
            barFillImg.fillAmount = 1f;
            var barFillRT = barFillGO.GetComponent<RectTransform>();
            barFillRT.anchorMin = Vector2.zero;
            barFillRT.anchorMax = Vector2.one;
            barFillRT.offsetMin = barFillRT.offsetMax = Vector2.zero;

            // Health value text
            var hpTextGO = new GameObject("HealthText");
            hpTextGO.transform.SetParent(barBgGO.transform, false);
            var hpTMP = hpTextGO.AddComponent<TextMeshProUGUI>();
            hpTMP.text      = "500 / 500";
            if (font != null) hpTMP.font = font;
            hpTMP.fontSize  = 14;
            hpTMP.color     = Color.white;
            hpTMP.alignment = TextAlignmentOptions.Center;
            var hpRT = hpTextGO.GetComponent<RectTransform>();
            hpRT.anchorMin = Vector2.zero;
            hpRT.anchorMax = Vector2.one;
            hpRT.offsetMin = hpRT.offsetMax = Vector2.zero;

            // State text
            var stateGO = new GameObject("StateText");
            stateGO.transform.SetParent(panelGO.transform, false);
            var stateTMP = stateGO.AddComponent<TextMeshProUGUI>();
            stateTMP.text      = "";
            if (font != null) stateTMP.font = font;
            stateTMP.fontSize  = 16;
            stateTMP.color     = new Color(1f, 0.4f, 0.1f, 1f);
            stateTMP.fontStyle = FontStyles.Bold;
            stateTMP.alignment = TextAlignmentOptions.Center;
            var stateRT = stateGO.GetComponent<RectTransform>();
            stateRT.anchorMin = new Vector2(0, 0f);
            stateRT.anchorMax = new Vector2(1f, 0.15f);
            stateRT.offsetMin = new Vector2(10, 2);
            stateRT.offsetMax = new Vector2(-10, -2);

            // Add BossHealthBar component
            var healthBarComp = panelGO.AddComponent<BossHealthBar>();
            healthBarComp.panel         = panelGO;
            healthBarComp.healthFill    = barFillImg;
            healthBarComp.bossNameText  = nameTMP;
            healthBarComp.healthValueText = hpTMP;
            healthBarComp.stateText     = stateTMP;

            // Link to boss
            bossScript.healthBar = healthBarComp;
            EditorUtility.SetDirty(bossScript);

            Debug.Log("[SetupMrPebble] Boss health bar added to HUDCanvas.");
        }

        // ── 6. Tag Mr Pebble ───────────────────────────────────────────────
        if (!System.Array.Exists(UnityEditorInternal.InternalEditorUtility.tags, t => t == "Boss"))
            Debug.LogWarning("[SetupMrPebble] 'Boss' tag not found — skipping tag assignment.");
        else
            boss.tag = "Boss";

        EditorUtility.SetDirty(boss);
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[SetupMrPebble] Mr Pebble boss setup complete!");
    }
}
