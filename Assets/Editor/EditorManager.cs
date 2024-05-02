using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class NewBehaviourScript : MonoBehaviour
{
    [MenuItem("MyEditor/Start from Scene 0 %h")]
    public static void PlaySceneFromZero()
    {
        var path_sceneZero = EditorBuildSettings.scenes[0].path;
        var sceneZeroAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path_sceneZero);
        EditorSceneManager.playModeStartScene = sceneZeroAsset;
        UnityEditor.EditorApplication.isPlaying = true;
    }

    [MenuItem("MyEditor/Start from Current Scene %j")]
    public static void PlaySceneFromCurrent()
    {
        EditorSceneManager.playModeStartScene = null;
        UnityEditor.EditorApplication.isPlaying = true;
    }
}
