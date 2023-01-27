// MIT License

// DeusaldUnityTools:
// Copyright (c) 2020 Adam "Deusald" Orliński

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DeusaldUnityTools
{
    /// <summary>
    /// Scene auto loader.
    /// </summary>
    /// <description>
    /// This class adds a File > Scene Autoload menu containing options to select
    /// a "master scene" enable it to be auto-loaded when the user presses play
    /// in the editor. When enabled, the selected scene will be loaded on play,
    /// then the original scene will be reloaded on stop.
    /// </description>
    [InitializeOnLoad]
    public static class SceneAutoLoader
    {
        #region Variables

        // Properties are remembered as editor preferences.
        private const string _EditorPrefLoadMasterOnPlay = "SceneAutoLoader.LoadMasterOnPlay";
        private const string _EditorPrefMasterScene      = "SceneAutoLoader.MasterScene";
        private const string _EditorPrefPreviousScene    = "SceneAutoLoader.PreviousScene";

        #endregion Variables

        #region Properties

        private static bool LoadMasterOnPlay
        {
            get => EditorPrefs.GetBool(_EditorPrefLoadMasterOnPlay, false);
            set => EditorPrefs.SetBool(_EditorPrefLoadMasterOnPlay, value);
        }

        private static string MasterScene
        {
            get => EditorPrefs.GetString(_EditorPrefMasterScene, "Master.unity");
            set => EditorPrefs.SetString(_EditorPrefMasterScene, value);
        }

        private static string PreviousScene
        {
            get => EditorPrefs.GetString(_EditorPrefPreviousScene, SceneManager.GetActiveScene().path);
            set => EditorPrefs.SetString(_EditorPrefPreviousScene, value);
        }

        #endregion Properties
        
        #region Init Methods

        // Static constructor binds a playmode-changed callback.
        // [InitializeOnLoad] above makes sure this gets executed.
        static SceneAutoLoader()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        #endregion Init Methods

        #region Public Methods

        public static void SetManually(string relativeMasterScenePath, bool loadMasterOnPlay = true)
        {
            MasterScene      = relativeMasterScenePath;
            LoadMasterOnPlay = loadMasterOnPlay;
        }

        #endregion Public Methods
        
        #region Private Methods

        // Menu items to select the "master" scene and control whether or not to load it.
        [MenuItem("Tools/Scene Autoload/Select Master Scene...")]
        private static void SelectMasterScene()
        {
            string masterScene = EditorUtility.OpenFilePanel("Select Master Scene", Application.dataPath, "unity");
            masterScene = masterScene.Replace(Application.dataPath, "Assets"); //project relative instead of absolute path
            if (!string.IsNullOrEmpty(masterScene))
            {
                MasterScene      = masterScene;
                LoadMasterOnPlay = true;
            }
        }

        [MenuItem("Tools/Scene Autoload/Load Master On Play", true)]
        private static bool ShowLoadMasterOnPlay()
        {
            return !LoadMasterOnPlay;
        }

        [MenuItem("Tools/Scene Autoload/Load Master On Play")]
        private static void EnableLoadMasterOnPlay()
        {
            LoadMasterOnPlay = true;
        }

        [MenuItem("Tools/Scene Autoload/Don't Load Master On Play", true)]
        private static bool ShowDontLoadMasterOnPlay()
        {
            return LoadMasterOnPlay;
        }

        [MenuItem("Tools/Scene Autoload/Don't Load Master On Play")]
        private static void DisableLoadMasterOnPlay()
        {
            LoadMasterOnPlay = false;
        }

        // Play mode change callback handles the scene load/reload.
        private static void OnPlayModeChanged(PlayModeStateChange playModeStateChange)
        {
            if (!LoadMasterOnPlay)
            {
                return;
            }

            if (!EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
            {
                // User pressed play -- autoload master scene.
                PreviousScene = SceneManager.GetActiveScene().path;
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    try
                    {
                        EditorSceneManager.OpenScene(MasterScene);
                    }
                    catch
                    {
                        Debug.LogError($"error: scene not found: {MasterScene}");
                        EditorApplication.isPlaying = false;
                    }
                }
                else
                {
                    // User cancelled the save operation -- cancel play as well.
                    EditorApplication.isPlaying = false;
                }
            }

            // isPlaying check required because cannot OpenScene while playing
            if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                // User pressed stop -- reload previous scene.
                try
                {
                    EditorSceneManager.OpenScene(PreviousScene);
                }
                catch
                {
                    Debug.LogError($"error: scene not found: {PreviousScene}");
                }
            }
        }

        #endregion Private Methods
    }
}