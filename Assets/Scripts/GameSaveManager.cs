using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Placed in the GameScene. On Awake it writes a save record so
/// MainMenuManager.ContinueGame() knows a save exists and which scene to load.
/// </summary>
public class GameSaveManager : MonoBehaviour
{
    private void Awake()
    {
        // Record that a save exists and store the current scene name
        PlayerPrefs.SetInt(MainMenuManager.SAVE_EXISTS_KEY, 1);
        PlayerPrefs.SetString(MainMenuManager.SAVE_SCENE_KEY, SceneManager.GetActiveScene().name);
        PlayerPrefs.Save();
    }
}
