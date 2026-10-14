using UnityEditor;
using UnityEngine;

public class FixMuddyGround6
{
    public static void Execute()
    {
        // Load the world-space shader
        Shader wsShader = Shader.Find("Custom/MuddyGroundWorldSpace");
        if (wsShader == null)
        {
            Debug.LogError("Could not find Custom/MuddyGroundWorldSpace shader. Make sure it compiled.");
            return;
        }

        Material muddyMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/MuddyGround.mat");
        if (muddyMat == null)
        {
            Debug.LogError("Could not find MuddyGround.mat");
            return;
        }

        // Load textures
        Texture2D dirtAlbedo = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Flooded_Grounds/Content/Textures/GR_Dirt1_AS.tif");
        Texture2D dirtNormal = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Flooded_Grounds/Content/Textures/GR_Dirt1_N.tif");

        // Switch to world-space shader
        muddyMat.shader = wsShader;

        muddyMat.SetTexture("_BaseMap", dirtAlbedo);
        muddyMat.SetTexture("_BumpMap", dirtNormal);

        // World tiling: 0.25 = texture repeats every 4 world units (nice scale for 5-unit tiles)
        muddyMat.SetFloat("_Tiling", 0.25f);

        // Warm muddy brown tint
        muddyMat.SetColor("_BaseColor", new Color(0.70f, 0.54f, 0.36f, 1f));

        // Matte mud
        muddyMat.SetFloat("_Smoothness", 0.08f);
        muddyMat.SetFloat("_BumpScale", 1.0f);

        EditorUtility.SetDirty(muddyMat);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("MuddyGround switched to world-space shader — no more UV stripes!");
    }
}
