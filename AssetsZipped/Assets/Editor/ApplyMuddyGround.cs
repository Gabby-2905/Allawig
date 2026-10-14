using UnityEditor;
using UnityEngine;
using System.IO;

public class ApplyMuddyGround
{
    public static void Execute()
    {
        // Load the dirt textures from Flooded_Grounds
        Texture2D dirtAlbedo = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Flooded_Grounds/Content/Textures/GR_Dirt1_AS.tif");
        Texture2D dirtNormal = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Flooded_Grounds/Content/Textures/GR_Dirt1_N.tif");

        if (dirtAlbedo == null)
        {
            Debug.LogError("Could not load GR_Dirt1_AS.tif");
            return;
        }

        // Create the muddy material
        string matPath = "Assets/MuddyGround.mat";
        Material muddyMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        muddyMat.name = "MuddyGround";

        // Set albedo texture
        muddyMat.SetTexture("_BaseMap", dirtAlbedo);
        muddyMat.SetTexture("_MainTex", dirtAlbedo);

        // Set normal map
        if (dirtNormal != null)
        {
            muddyMat.SetTexture("_BumpMap", dirtNormal);
            muddyMat.EnableKeyword("_NORMALMAP");
        }

        // Mud properties: dark brownish tint, low smoothness, slightly wet look
        muddyMat.SetColor("_BaseColor", new Color(0.42f, 0.30f, 0.18f, 1f));
        muddyMat.SetFloat("_Smoothness", 0.25f);
        muddyMat.SetFloat("_Metallic", 0.0f);
        muddyMat.SetFloat("_BumpScale", 1.2f);

        // Tile the texture to look natural across the ground tiles
        muddyMat.SetTextureScale("_BaseMap", new Vector2(2f, 2f));
        muddyMat.SetTextureScale("_BumpMap", new Vector2(2f, 2f));

        AssetDatabase.CreateAsset(muddyMat, matPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Created muddy material at {matPath}");

        // Find all Ground child objects and apply the material
        GameObject groundParent = GameObject.Find("Ground");
        if (groundParent == null)
        {
            Debug.LogError("Could not find 'Ground' GameObject in scene.");
            return;
        }

        int count = 0;
        MeshRenderer[] renderers = groundParent.GetComponentsInChildren<MeshRenderer>(true);
        foreach (MeshRenderer mr in renderers)
        {
            mr.sharedMaterial = muddyMat;
            count++;
            EditorUtility.SetDirty(mr.gameObject);
        }

        Debug.Log($"Applied MuddyGround material to {count} ground tiles.");

        // Mark scene dirty and save
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("Scene saved.");
    }
}
