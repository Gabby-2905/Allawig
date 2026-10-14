using UnityEngine;
using UnityEditor;

/// <summary>
/// Fixes rain visibility by splitting the UI into layered canvases:
/// - BackgroundCanvas (sortOrder 0): background image only
/// - RainCanvas (sortOrder 5): rain particles + lightning flash as UI RawImage
/// - MainCanvas (sortOrder 10): all buttons and panels
/// </summary>
public class FixRainSortingOrder : MonoBehaviour
{
    [MenuItem("Tools/Fix Rain Sorting Order")]
    public static void Execute()
    {
        GameObject mainCanvasGO = GameObject.Find("Canvas");
        if (mainCanvasGO == null) { Debug.LogError("Canvas not found!"); return; }

        UnityEngine.Canvas mainCanvas = mainCanvasGO.GetComponent<UnityEngine.Canvas>();
        UnityEngine.UI.CanvasScaler mainScaler = mainCanvasGO.GetComponent<UnityEngine.UI.CanvasScaler>();

        // ── 1. Bump main Canvas to sort order 10 ──────────────────────────
        mainCanvas.sortingOrder = 10;

        // ── 2. Create BackgroundCanvas (sort order 0) ─────────────────────
        GameObject bgCanvasGO = GameObject.Find("BackgroundCanvas");
        if (bgCanvasGO == null)
        {
            bgCanvasGO = new GameObject("BackgroundCanvas");
            var bgC = bgCanvasGO.AddComponent<UnityEngine.Canvas>();
            bgC.renderMode = UnityEngine.RenderMode.ScreenSpaceOverlay;
            bgC.sortingOrder = 0;
            var bgScaler = bgCanvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            bgScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            bgScaler.referenceResolution = new Vector2(1920, 1080);
            bgScaler.matchWidthOrHeight = 0.5f;
            bgCanvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        // Move Background image into BackgroundCanvas
        Transform bgT = mainCanvasGO.transform.Find("Background");
        if (bgT != null)
        {
            bgT.SetParent(bgCanvasGO.transform, false);
            var rt = bgT.GetComponent<UnityEngine.RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            Debug.Log("Background moved to BackgroundCanvas.");
        }

        // ── 3. Create RainCanvas (sort order 5) ───────────────────────────
        GameObject rainCanvasGO = GameObject.Find("RainCanvas");
        if (rainCanvasGO == null)
        {
            rainCanvasGO = new GameObject("RainCanvas");
            var rc = rainCanvasGO.AddComponent<UnityEngine.Canvas>();
            rc.renderMode = UnityEngine.RenderMode.ScreenSpaceOverlay;
            rc.sortingOrder = 5;
            var rcScaler = rainCanvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            rcScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            rcScaler.referenceResolution = new Vector2(1920, 1080);
            rcScaler.matchWidthOrHeight = 0.5f;
            rainCanvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        // Move RainParticles into RainCanvas
        Transform rainT = mainCanvasGO.transform.Find("RainParticles");
        if (rainT != null)
        {
            rainT.SetParent(rainCanvasGO.transform, false);
            var rt = rainT.GetComponent<UnityEngine.RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }

            // Fix particle system settings for proper screen coverage
            ParticleSystem ps = rainT.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
                main.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 1.8f);
                main.startSpeed = new ParticleSystem.MinMaxCurve(1200f, 1600f);
                main.startSize = new ParticleSystem.MinMaxCurve(1.5f, 3f);
                main.startColor = new ParticleSystem.MinMaxGradient(
                    new Color(0.75f, 0.88f, 1f, 0.55f),
                    new Color(0.6f, 0.78f, 0.95f, 0.35f));
                main.maxParticles = 1500;
                main.gravityModifier = 0f;

                var emission = ps.emission;
                emission.rateOverTime = 350f;

                var shape = ps.shape;
                shape.enabled = true;
                shape.shapeType = ParticleSystemShapeType.Rectangle;
                shape.scale = new Vector3(2200f, 1f, 1f);
                shape.position = new Vector3(0f, 700f, 0f);
                shape.rotation = Vector3.zero;

                var vel = ps.velocityOverLifetime;
                vel.enabled = true;
                vel.space = ParticleSystemSimulationSpace.Local;
                vel.y = new ParticleSystem.MinMaxCurve(-1400f, -1100f);
                vel.x = new ParticleSystem.MinMaxCurve(-60f, -30f);

                ParticleSystemRenderer psr = rainT.GetComponent<ParticleSystemRenderer>();
                if (psr != null)
                {
                    psr.renderMode = ParticleSystemRenderMode.Stretch;
                    psr.velocityScale = 0.04f;
                    psr.lengthScale = 3.5f;
                    psr.sortingOrder = 0;
                }
            }
            Debug.Log("RainParticles moved to RainCanvas.");
        }

        // Move LightningFlash into RainCanvas too
        Transform lightT = mainCanvasGO.transform.Find("LightningFlash");
        if (lightT != null)
        {
            lightT.SetParent(rainCanvasGO.transform, false);
            var rt = lightT.GetComponent<UnityEngine.RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
            Debug.Log("LightningFlash moved to RainCanvas.");
        }

        // ── 4. Save ────────────────────────────────────────────────────────
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("✅ Rain canvas layering complete. BackgroundCanvas=0, RainCanvas=5, MainCanvas=10.");
    }
}
