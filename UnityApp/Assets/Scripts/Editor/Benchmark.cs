#if UNITY_EDITOR
using Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class Benchmark
{
    private const string NativeScene = "Assets/Scenes/SampleScene.unity";
    private const string SocketScene = "Assets/Scenes/SampleScene 1.unity";

    public static void Native()
    {
        Debug.Log("SHELL: Rebuilding Native Library...");
        NativeLibBuilder.RebuildNativeLibrary();
        EditorSceneManager.OpenScene(NativeScene);
        EditorApplication.isPlaying = true;
        Debug.Log("SHELL: Native Simulation Started...");
    }

    public static void Socket()
    {
        EditorSceneManager.OpenScene(SocketScene);
        EditorApplication.isPlaying = true;
        Debug.Log("SHELL: Socket Simulation Started...");
    }
}
#endif
