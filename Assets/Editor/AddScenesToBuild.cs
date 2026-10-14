using UnityEditor;
using UnityEngine;

public class AddScenesToBuild
{
    public static void Execute()
    {
        var scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/MainMenuScene.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/GameScene.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/SampleScene.unity", true),
        };
        EditorBuildSettings.scenes = scenes;
        Debug.Log("[AddScenesToBuild] Build scenes set: MainMenuScene(0), GameScene(1), SampleScene(2)");
    }
}
