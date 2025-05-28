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

using JetBrains.Annotations;
using UnityEngine;
#if TEST_SCRIPT || (UNITY_IOS && !UNITY_EDITOR)
using System.Runtime.InteropServices;
#endif

namespace DeusaldUnityTools
{
    // ReSharper disable once InconsistentNaming
    [PublicAPI]
    public static class iOSTools
    {
        #if TEST_SCRIPT || (UNITY_IOS && !UNITY_EDITOR)
        [DllImport("__Internal")]
        private static extern void _OpenAppStorePage(string appId);

        [DllImport("__Internal")]
        private static extern void _OpenAppSettings();
        #endif

        /// <summary>
        /// Opens the App Store page for the specified App ID.
        /// </summary>
        /// <param name="appId">The app’s numeric Apple ID (not the bundle ID)</param>
        public static void OpenAppStore(string appId)
        {
            #if TEST_SCRIPT || (UNITY_IOS && !UNITY_EDITOR)
            _OpenAppStorePage(appId);
            #else
            Debug.Log($"Would open App Store page for appId: {appId}");
            #endif
        }

        /// <summary>
        /// Opens the iOS settings screen from the app.
        /// </summary>
        public static void OpenAppSettings()
        {
            #if TEST_SCRIPT || (UNITY_IOS && !UNITY_EDITOR)
            _OpenAppSettings();
            #else
            Debug.Log("This feature only works on iOS devices.");
            #endif
        }
    }
}