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

using UnityEngine;

namespace DeusaldUnityTools
{
    public static class AndroidTools
    {
        /// <summary> Use to get keyboard height to place elements above the keyboard. Use as a ratio:
        /// float rate = referenceCanvasHeight / Screen.height;
        /// positionY = AndroidAppLauncher.GetKeyboardHeight() * rate + margin
        /// </summary>
        public static float GetKeyboardHeight()
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                AndroidJavaClass  unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject decorView = activity.Call<AndroidJavaObject>("getWindow").Call<AndroidJavaObject>("getDecorView");
                AndroidJavaObject rect = new AndroidJavaObject("android.graphics.Rect");

                decorView.Call("getWindowVisibleDisplayFrame", rect);
                int visibleHeight = rect.Call<int>("height");

                AndroidJavaObject rootView = decorView.Call<AndroidJavaObject>("getRootView");
                int               screenHeight = rootView.Call<int>("getHeight");

                int keyboardHeight = screenHeight - visibleHeight;

                // If height is reasonable (to avoid false positives)
                if (keyboardHeight > screenHeight / 5) return keyboardHeight;
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
            #if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (AndroidJavaObject packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager"))
                {
                    AndroidJavaObject launchIntent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", packageName);

                    if (launchIntent != null)
                    {
                        // App is installed, launch it
                        Debug.Log($"{packageName} is installed, launching it...");
                        currentActivity.Call("startActivity", launchIntent);
                    }
                    else
                    {
                        // App isn't installed, try to open Play Store
                        Debug.Log($"{packageName} is not installed, opening Play Store...");
                        TryOpenPlayStorePage(packageName);
                    }
                }
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
            #if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                string marketUri = $"market://details?id={packageName}";
                string webUri = $"https://play.google.com/store/apps/details?id={packageName}";

                using (AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", "android.intent.action.VIEW"))
                using (AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri"))
                {
                    AndroidJavaObject uri = uriClass.CallStatic<AndroidJavaObject>("parse", marketUri);
                    intent.Call<AndroidJavaObject>("setData", uri);

                    // Check if Play Store is available
                    using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                    using (AndroidJavaObject packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager"))
                    using (AndroidJavaObject resolveInfo = packageManager.Call<AndroidJavaObject>("resolveActivity", intent, 0))
                    {
                        if (resolveInfo != null)
                        {
                            currentActivity.Call("startActivity", intent);
                        }
                        else
                        {
                            // Play Store not available, fallback to browser
                            AndroidJavaObject webIntent = new AndroidJavaObject("android.content.Intent", "android.intent.action.VIEW");
                            AndroidJavaObject webUriObject = uriClass.CallStatic<AndroidJavaObject>("parse", webUri);
                            webIntent.Call<AndroidJavaObject>("setData", webUriObject);
                            currentActivity.Call("startActivity", webIntent);
                        }
                    }
                }
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
            #if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var uriClass = new AndroidJavaClass("android.net.Uri"))
                using (var intent = new AndroidJavaObject("android.content.Intent",
                           "android.settings.APPLICATION_DETAILS_SETTINGS"))
                {
                    string            packageName = context.Call<string>("getPackageName");
                    AndroidJavaObject uri = uriClass.CallStatic<AndroidJavaObject>("fromParts", "package", packageName, null);
                    intent.Call<AndroidJavaObject>("setData", uri);
                    context.Call("startActivity", intent);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to open Android app settings: " + e.Message);
            }
            #else
            Debug.Log("OpenAppSettings is Android-only functionality.");
            #endif
        }
    }
}