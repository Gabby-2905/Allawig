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
            new EditorBuildSettingsScene("Assets/Toon Gas Station/Scenes/Level1MapGas.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Level2Scene.unity", true),
        };
        EditorBuildSettings.scenes = scenes;
        Debug.Log("[AddScenesToBuild] Build scenes set: MainMenuScene(0), GameScene(1), Level1MapGas(2), Level2Scene(3)");
    }
}
