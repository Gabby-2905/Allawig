using UnityEngine;
using UnityEditor;

/// <summary>
/// Creates a proper UI rain effect using a RenderTexture + dedicated camera approach.
/// A RainCamera renders only the particle system to a RenderTexture,
/// which is displayed as a RawImage on the RainCanvas between the background and UI.
/// </summary>
public class BuildUIRain : MonoBehaviour
{
    [MenuItem("Tools/Build UI Rain (RenderTexture)")]
    public static void Execute()
    {
        // ── Clean up old rain objects ──────────────────────────────────────
        DestroyIfExists("RainCanvas");
        DestroyIfExists("RainCamera");
        DestroyIfExists("RainParticles");

        // ── 1. Create RenderTexture asset ──────────────────────────────────
        string rtPath = "Assets/RainRenderTexture.renderTexture";
        RenderTexture rt = AssetDatabase.LoadAssetAtPath<RenderTexture>(rtPath);
        if (rt == null)
        {
            rt = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGB32);
            rt.name = "RainRenderTexture";
            rt.antiAliasing = 1;
            rt.filterMode = FilterMode.Bilinear;
            rt.wrapMode = TextureWrapMode.Clamp;
            rt.Create();
            AssetDatabase.CreateAsset(rt, rtPath);
            AssetDatabase.SaveAssets();
        }

        // ── 2. Create Rain Particle System in world space ──────────────────
        GameObject rainGO = new GameObject("RainParticles");
        rainGO.transform.position = new Vector3(0f, 5f, 10f);
        rainGO.transform.rotation = Quaternion.identity;
        rainGO.layer = LayerMask.NameToLayer("Default");

        // Create a dedicated layer for rain if possible, otherwise use Default
        int rainLayer = LayerMask.NameToLayer("Default");

        ParticleSystem ps = rainGO.AddComponent<ParticleSystem>();
        ParticleSystemRenderer psr = rainGO.GetComponent<ParticleSystemRenderer>();

        // Main module
        var main = ps.main;
        main.loop = true;
        main.prewarm = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.5f, 2.5f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.015f, 0.025f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.75f, 0.88f, 1f, 0.6f),
            new Color(0.6f, 0.78f, 0.95f, 0.4f));
        main.maxParticles = 3000;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0f;

        // Emission
        var emission = ps.emission;
        emission.rateOverTime = 500f;

        // Shape: wide box above camera view
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Rectangle;
        shape.scale = new Vector3(22f, 1f, 1f);
        shape.position = new Vector3(0f, 6f, 0f);
        shape.rotation = Vector3.zero;

        // Velocity over lifetime — rain falls down and slightly sideways
        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.World;
        vel.y = new ParticleSystem.MinMaxCurve(-18f, -14f);
        vel.x = new ParticleSystem.MinMaxCurve(-0.8f, -0.3f);

        // Renderer
        psr.renderMode = ParticleSystemRenderMode.Stretch;
        psr.velocityScale = 0.06f;
        psr.lengthScale = 3f;
        psr.sortingOrder = 0;

        // Use a transparent/additive material
        Shader rainShader = Shader.Find("Particles/Standard Unlit");
        if (rainShader == null) rainShader = Shader.Find("Legacy Shaders/Particles/Additive");
        if (rainShader == null) rainShader = Shader.Find("Sprites/Default");

        Material rainMat = new Material(rainShader);
        rainMat.color = new Color(0.75f, 0.88f, 1f, 0.5f);
        string matPath = "Assets/RainMaterial.mat";
        AssetDatabase.DeleteAsset(matPath);
        AssetDatabase.CreateAsset(rainMat, matPath);
        psr.material = AssetDatabase.LoadAssetAtPath<Material>(matPath);

        // ── 3. Create Rain Camera ──────────────────────────────────────────
        GameObject rainCamGO = new GameObject("RainCamera");
        rainCamGO.transform.position = new Vector3(0f, 0f, 0f);
        rainCamGO.transform.rotation = Quaternion.identity;

        Camera rainCam = rainCamGO.AddComponent<Camera>();
        rainCam.clearFlags = CameraClearFlags.SolidColor;
        rainCam.backgroundColor = new Color(0f, 0f, 0f, 0f); // transparent black
        rainCam.cullingMask = 1 << rainLayer; // only render rain layer
        rainCam.orthographic = false;
        rainCam.fieldOfView = 60f;
        rainCam.nearClipPlane = 0.1f;
        rainCam.farClipPlane = 50f;
        rainCam.targetTexture = rt;
        rainCam.depth = -2; // render before main camera
        rainCam.allowHDR = false;
        rainCam.allowMSAA = false;

        // ── 4. Create RainCanvas with RawImage showing the RenderTexture ───
        GameObject rainCanvasGO = new GameObject("RainCanvas");
        var rainCanvas = rainCanvasGO.AddComponent<Canvas>();
        rainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        rainCanvas.sortingOrder = 5; // above background (0), below main UI (10)

        var rainScaler = rainCanvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        rainScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        rainScaler.referenceResolution = new Vector2(1920, 1080);
        rainScaler.matchWidthOrHeight = 0.5f;
        rainCanvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // RawImage to display the render texture
        GameObject rainImageGO = new GameObject("RainImage");
        rainImageGO.transform.SetParent(rainCanvasGO.transform, false);
        var rainRT2 = rainImageGO.AddComponent<RectTransform>();
        rainRT2.anchorMin = Vector2.zero;
        rainRT2.anchorMax = Vector2.one;
        rainRT2.offsetMin = Vector2.zero;
        rainRT2.offsetMax = Vector2.zero;

        var rawImg = rainImageGO.AddComponent<UnityEngine.UI.RawImage>();
        rawImg.texture = rt;
        rawImg.color = new Color(1f, 1f, 1f, 0.85f); // slight transparency

        // ── 5. Add LightningFlash to RainCanvas ────────────────────────────
        // Check if it exists in old RainCanvas
        GameObject oldRainCanvas = GameObject.Find("RainCanvas");
        // Already created above as rainCanvasGO

        // Create lightning flash overlay on RainCanvas
        GameObject flashGO = new GameObject("LightningFlash");
        flashGO.transform.SetParent(rainCanvasGO.transform, false);
        var flashRT = flashGO.AddComponent<RectTransform>();
        flashRT.anchorMin = Vector2.zero;
        flashRT.anchorMax = Vector2.one;
        flashRT.offsetMin = Vector2.zero;
        flashRT.offsetMax = Vector2.zero;

        var flashImg = flashGO.AddComponent<UnityEngine.UI.Image>();
        flashImg.color = new Color(0.9f, 0.95f, 1f, 0f);

        var thunderAudio = flashGO.AddComponent<AudioSource>();
        thunderAudio.playOnAwake = false;
        thunderAudio.spatialBlend = 0f;
        thunderAudio.volume = 0.7f;

        var le = flashGO.AddComponent<LightningEffect>();
        le.flashImage = flashImg;
        le.thunderSource = thunderAudio;
        le.minInterval = 4f;
        le.maxInterval = 12f;
        le.fadeDuration = 0.18f;
        le.peakAlpha = 0.55f;
        le.thunderDelay = 0.5f;

        // ── 6. Save ────────────────────────────────────────────────────────
        AssetDatabase.SaveAssets();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("✅ UI Rain built with RenderTexture. RainCamera renders particles to RainCanvas RawImage.");
    }

    static void DestroyIfExists(string name)
    {
        GameObject go = GameObject.Find(name);
        if (go != null)
        {
            GameObject.DestroyImmediate(go);
            Debug.Log($"Destroyed existing {name}");
        }
    }
}
