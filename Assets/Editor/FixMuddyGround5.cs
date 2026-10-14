using UnityEditor;
using UnityEngine;

public class FixMuddyGround5
{
    public static void Execute()
    {
        Material muddyMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/MuddyGround.mat");
        if (muddyMat == null)
        {
            Debug.LogError("Could not find MuddyGround.mat");
            return;
        }

        // Load moss texture to use as detail map to break up the stripe pattern
        Texture2D mossAlbedo = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Flooded_Grounds/Content/Textures/GR_Moss1_AS.tif");
        Texture2D mossNormal = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Flooded_Grounds/Content/Textures/GR_Moss1_N.tif");

        // Use non-integer tiling to break the per-tile UV alignment stripes
        // 1.7 doesn't divide evenly into the tile count so stripes won't repeat uniformly
        muddyMat.SetTextureScale("_BaseMap", new Vector2(1.7f, 1.7f));
        muddyMat.SetTextureScale("_BumpMap", new Vector2(1.7f, 1.7f));

        // Slightly offset to further break symmetry
        muddyMat.SetTextureOffset("_BaseMap", new Vector2(0.15f, 0.15f));
        muddyMat.SetTextureOffset("_BumpMap", new Vector2(0.15f, 0.15f));

        // Use detail albedo map (moss) to add variation and break up the uniform look
        if (mossAlbedo != null)
        {
            muddyMat.SetTexture("_DetailAlbedoMap", mossAlbedo);
            muddyMat.SetTextureScale("_DetailAlbedoMap", new Vector2(4f, 4f));
            muddyMat.EnableKeyword("_DETAIL_MULX2");
            muddyMat.SetFloat("_DetailAlbedoMapScale", 0.4f);
            Debug.Log("Moss detail texture applied.");
        }

        if (mossNormal != null)
        {
            muddyMat.SetTexture("_DetailNormalMap", mossNormal);
            muddyMat.SetTextureScale("_DetailNormalMap", new Vector2(4f, 4f));
            muddyMat.SetFloat("_DetailNormalMapScale", 0.5f);
        }

        // Warm muddy brown
        muddyMat.SetColor("_BaseColor", new Color(0.68f, 0.52f, 0.34f, 1f));

        // Matte mud
        muddyMat.SetFloat("_Smoothness", 0.08f);
        muddyMat.SetFloat("_Metallic", 0.0f);
        muddyMat.SetFloat("_BumpScale", 1.0f);

        EditorUtility.SetDirty(muddyMat);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("MuddyGround material updated - non-integer tiling + moss detail to break stripes.");
    }
}
