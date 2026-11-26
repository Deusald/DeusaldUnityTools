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
#define TEST_SCRIPT
using JetBrains.Annotations;
using UnityEngine;

namespace DeusaldUnityTools
{
    [PublicAPI]
    public static class AndroidTools
    {
        private static AndroidJavaObject _CurrentActivity;

        private static readonly AndroidJavaClass  _ToolsClass = new("com.deusald.deusaldjavatools.Tools");
        private static readonly AndroidJavaObject _ShareClass = new("com.deusald.deusaldjavatools.Share");
        
        public static void RefreshCurrentActivity()
        {
            using AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            _CurrentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        }
        
        /// <summary> Use to get keyboard height to place elements above the keyboard. Use as a ratio:
        /// float rate = referenceCanvasHeight / Screen.height;
        /// positionY = AndroidAppLauncher.GetKeyboardHeight() * rate + margin
        /// </summary>
        public static float GetKeyboardHeight()
        {
            #if TEST_SCRIPT || (UNITY_ANDROID && !UNITY_EDITOR)
            try
            {
                if (_CurrentActivity == null) RefreshCurrentActivity();
                return _ToolsClass.CallStatic<int>("getKeyboardHeight", _CurrentActivity);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Failed to get Android keyboard height: " + e);
            }
            #endif
            return TouchScreenKeyboard.area.height;
        }

        /// <summary>
        /// Opens app and if that app is not installed, then opens a Play Store page for a specific app, with fallback to browser if Play Store isn't available.
        /// </summary>
        public static void LaunchOrOpenPlayStore(string packageName)
        {
            #if TEST_SCRIPT || (UNITY_ANDROID && !UNITY_EDITOR)
            try
            {
                if (_CurrentActivity == null) RefreshCurrentActivity();
                _ToolsClass.CallStatic("launchOrOpenPlayStore", _CurrentActivity, packageName);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error launching or opening Play Store: " + ex.Message);
            }
            #else
            Debug.Log("LaunchOrOpenPlayStore is Android-only functionality.");
            #endif
        }

        /// <summary> Opens a Play Store page for a specific app, with fallback to browser if Play Store isn't available.
        /// </summary>
        public static void TryOpenPlayStorePage(string packageName)
        {
            #if TEST_SCRIPT || (UNITY_ANDROID && !UNITY_EDITOR)
            try
            {
                if (_CurrentActivity == null) RefreshCurrentActivity();
                _ToolsClass.CallStatic("tryOpenPlayStorePage", _CurrentActivity, packageName);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Failed to open Play Store or browser: " + ex.Message);
            }
            #else
            Debug.Log("TryOpenPlayStorePage is Android-only functionality.");
            #endif
        }

        /// <summary> Opens app settings. </summary>
        public static void OpenAppSettings()
        {
            #if TEST_SCRIPT || (UNITY_ANDROID && !UNITY_EDITOR)
            try
            {
                if (_CurrentActivity == null) RefreshCurrentActivity();
                _ToolsClass.CallStatic("openAppSettings", _CurrentActivity);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to open Android app settings: " + e.Message);
            }
            #else
            Debug.Log("OpenAppSettings is Android-only functionality.");
            #endif
        }

        /// <summary> This method has to be executed at the start of the application to init the Share module. </summary>
        public static void InitShare(string providerName)
        {
            #if TEST_SCRIPT || (UNITY_ANDROID && !UNITY_EDITOR)
            try
            {
                if (_CurrentActivity == null) RefreshCurrentActivity();
                _ShareClass.Call("init", _CurrentActivity, providerName);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to init Android Share module: " + e.Message);
            }
            #else
            Debug.Log("InitShare is Android-only functionality.");
            #endif
        }
        
        /// <summary> Uses native Android methods to share text via another app. </summary>
        public static void ShareText(string message)
        {
            #if TEST_SCRIPT || (UNITY_ANDROID && !UNITY_EDITOR)
            try
            {
                _ShareClass.Call("shareText", message);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to share text using native android module: " + e.Message);
            }
            #else
            Debug.Log("ShareText is Android-only functionality.");
            #endif
        }
        
        /// <summary> Uses native Android methods to share the file via another app. </summary>
        public static void ShareFile(string filePath, string message)
        {
            #if TEST_SCRIPT || (UNITY_ANDROID && !UNITY_EDITOR)
            try
            {
                if (_CurrentActivity == null) RefreshCurrentActivity();
                _ShareClass.Call("shareSingleFile", _CurrentActivity, filePath, message);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to share file using native android module: " + e.Message);
            }
            #else
            Debug.Log("ShareSingleFile is Android-only functionality.");
            #endif
        }
        
        /// <summary> Uses native Android methods to share the files via another app. </summary>
        public static void ShareFiles(string[] filesPaths, string message)
        {
            #if TEST_SCRIPT || (UNITY_ANDROID && !UNITY_EDITOR)
            try
            {
                if (_CurrentActivity == null) RefreshCurrentActivity();
                _ShareClass.Call("shareMultipleFiles", _CurrentActivity, filesPaths, message);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to share files using native android module: " + e.Message);
            }
            #else
            Debug.Log("ShareMultipleFiles is Android-only functionality.");
            #endif
        }
    }
}