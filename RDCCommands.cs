/*
 *      MIT License
 *      # Copyright (c) 2025 OnistDerFalke
 *      # This software is provided "as is", without any warranty. You can use, modify, and share it freely.
 */

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Template class containing list of debug commands available for game.
/// </summary>
public class RDCCommands : MonoBehaviour
{
    #region References

    //Paste your references here
    //..

    private void Awake()
    {
        //Load dynamic references
    }

    #endregion

    /// <summary>
    /// Sets timescale to proper value. Default value: 1.
    /// </summary>
    [DebugCommand("set_timescale", "Set global timescale to value.")]
    private void SetTimeScale(float value)
    {
        Time.timeScale = value;
        Debug.Log($"Time scale set to {value}.");
    }

    /// <summary>
    /// Resets timescale.
    /// </summary>
    [DebugCommand("reset_timescale", "Resets global timescale to default value.")]
    private void ResetTimeScale()
    {
        Time.timeScale = 1f;
        Debug.Log($"Time scale has been reset.");
    }

    /// <summary>
    /// Reloads current scene.
    /// </summary>
    [DebugCommand("reload_scene", "Reloads current scene.")]
    private void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Debug.Log($"Scene {SceneManager.GetActiveScene().name} has been reloaded.");
    }

    /// <summary>
    /// Lists available scenes.
    /// </summary>
    [DebugCommand("list_scenes", "Lists all available scenes.")]
    private void ListScenes()
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        Debug.Log($"Scenes in build settings ({sceneCount}):");

        for (int i = 0; i < sceneCount; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
            Debug.Log($"{i}: {sceneName}");
        }
    }

    /// <summary>
    /// Loads scene by name.
    /// </summary>
    [DebugCommand("set_scene", "Loads scene by name.")]
    private void SetScene(string sceneName)
    {
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
            Debug.Log($"Scene '{sceneName}' has been loaded.");
        }
        else
        {
            Debug.LogWarning($"Scene '{sceneName}' cannot be loaded. Check scene name or build settings.");
        }
    }
}
