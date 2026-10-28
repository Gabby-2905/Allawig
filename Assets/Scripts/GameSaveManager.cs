using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Placed in any gameplay scene. On Awake it writes a save record so
/// MainMenuManager.ContinueGame() knows a save exists and which scene to load.
/// </summary>
public class GameSaveManager : MonoBehaviour
{
    // ── Inventory persistence keys ─────────────────────────────────────────
    public const string INV_ITEM_KEY   = "Inv_Item_";    // + slot index
    public const string INV_COLOR_R    = "Inv_R_";
    public const string INV_COLOR_G    = "Inv_G_";
    public const string INV_COLOR_B    = "Inv_B_";
    public const int    INV_SLOTS      = 4;

    private void Awake()
    {
        // Record that a save exists and store the current scene name
        PlayerPrefs.SetInt(MainMenuManager.SAVE_EXISTS_KEY, 1);
        PlayerPrefs.SetString(MainMenuManager.SAVE_SCENE_KEY, SceneManager.GetActiveScene().name);
        PlayerPrefs.Save();
    }

    // ── Static helpers called by GameHUD ───────────────────────────────────

    /// <summary>Persist a single inventory slot to PlayerPrefs.</summary>
    public static void SaveInventorySlot(int slot, string itemName, Color color)
    {
        PlayerPrefs.SetString(INV_ITEM_KEY + slot, itemName);
        PlayerPrefs.SetFloat(INV_COLOR_R   + slot, color.r);
        PlayerPrefs.SetFloat(INV_COLOR_G   + slot, color.g);
        PlayerPrefs.SetFloat(INV_COLOR_B   + slot, color.b);
        PlayerPrefs.Save();
    }

    /// <summary>Load a single inventory slot from PlayerPrefs.</summary>
    public static (string name, Color color) LoadInventorySlot(int slot)
    {
        string name = PlayerPrefs.GetString(INV_ITEM_KEY + slot, "");
        float r = PlayerPrefs.GetFloat(INV_COLOR_R + slot, 0.1f);
        float g = PlayerPrefs.GetFloat(INV_COLOR_G + slot, 0.1f);
        float b = PlayerPrefs.GetFloat(INV_COLOR_B + slot, 0.1f);
        return (name, new Color(r, g, b, 0.85f));
    }

    /// <summary>Clear all saved inventory data.</summary>
    public static void ClearInventory()
    {
        for (int i = 0; i < INV_SLOTS; i++)
        {
            PlayerPrefs.DeleteKey(INV_ITEM_KEY + i);
            PlayerPrefs.DeleteKey(INV_COLOR_R  + i);
            PlayerPrefs.DeleteKey(INV_COLOR_G  + i);
            PlayerPrefs.DeleteKey(INV_COLOR_B  + i);
        }
        PlayerPrefs.Save();
    }
}
