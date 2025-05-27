using UnityEngine;
#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace DeusaldUnityTools
{
    // ReSharper disable once InconsistentNaming
    public static class iOSTools
    {
        #if UNITY_IOS && !UNITY_EDITOR
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
            #if UNITY_IOS && !UNITY_EDITOR
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
            #if UNITY_IOS && !UNITY_EDITOR
            _OpenAppSettings();
            #else
            Debug.Log("This feature only works on iOS devices.");
            #endif
        }
    }
}