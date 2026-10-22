using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class PlayFromBoot
{
    private const string BootPath = "Assets/00.Scenes/Boot.unity";

    static PlayFromBoot()
    {
        EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(BootPath);
    }
}