using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Attach to Main Camera to diagnose black screen issues at runtime.
/// </summary>
public class CameraDiagnostic : MonoBehaviour
{
    void Start()
    {
        var cam = GetComponent<Camera>();
        Debug.Log($"[CamDiag] clearFlags={cam.clearFlags}, bgColor={cam.backgroundColor}, enabled={cam.enabled}");
        Debug.Log($"[CamDiag] Camera pos={transform.position}, rot={transform.eulerAngles}");
        Debug.Log($"[CamDiag] Active pipeline: {GraphicsSettings.defaultRenderPipeline?.name}");

        var urpData = GetComponent<UniversalAdditionalCameraData>();
        if (urpData != null)
            Debug.Log($"[CamDiag] URP postProcessing={urpData.renderPostProcessing}");

        // Force fix at runtime
        cam.clearFlags = CameraClearFlags.Skybox;
        cam.backgroundColor = new Color(0.49f, 0.71f, 0.91f, 1f);
        cam.enabled = true;

        if (urpData != null)
        {
            urpData.renderPostProcessing = false;
        }

        Debug.Log($"[CamDiag] After fix: clearFlags={cam.clearFlags}");
    }
}
