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
#if TEST_SCRIPT_IOS || (UNITY_IOS && !UNITY_EDITOR)
using System.Runtime.InteropServices;
#endif
#if TEST_SCRIPT_ANDROID || TEST_SCRIPT_IOS || (UNITY_ANDROID && !UNITY_EDITOR) || (UNITY_IOS && !UNITY_EDITOR)
using System;
using System.Collections.Generic;
using UnityEngine;
#endif

namespace DeusaldUnityTools
{
    [PublicAPI]
    public static class Haptic
    {
        #if TEST_SCRIPT_ANDROID || TEST_SCRIPT_IOS || (UNITY_ANDROID && !UNITY_EDITOR) || (UNITY_IOS && !UNITY_EDITOR)

        private enum SupportedHapticType
        {
            NotChecked = 0,
            None       = 1,
            Base       = 2,
            Advanced   = 3
        }

        private static SupportedHapticType _SupportedHapticType = SupportedHapticType.NotChecked;

        #endif

        public static bool TurnedOn { get; set; } = true;

        public static void Perform(HapticType type)
        {
            #if TEST_SCRIPT_IOS || (UNITY_IOS && !UNITY_EDITOR)
            PerformInternalIOS(type);
            #endif

            #if TEST_SCRIPT_ANDROID || (UNITY_ANDROID && !UNITY_EDITOR)
            PerformInternalAndroid(type);
            #endif
        }

        #if TEST_SCRIPT_IOS || (UNITY_IOS && !UNITY_EDITOR)
        [DllImport("__Internal")]
        private static extern int _IsHapticSupported();

        [DllImport("__Internal")]
        private static extern void _TapticEngine(int type);

        [DllImport("__Internal")]
        private static extern void _Haptic(int type);

        private static void PerformInternalIOS(HapticType type)
        {
            if (!TurnedOn) return;
            if (_SupportedHapticType == SupportedHapticType.NotChecked) _SupportedHapticType = (SupportedHapticType)_IsHapticSupported();

            if (type != HapticType.Default && _SupportedHapticType == SupportedHapticType.Base) _TapticEngine((int)type);
            else if (type != HapticType.Default && _SupportedHapticType == SupportedHapticType.Advanced) _Haptic((int)type);
            else Handheld.Vibrate();
        }

        #endif

        #if TEST_SCRIPT_ANDROID || (UNITY_ANDROID && !UNITY_EDITOR)

        private const string _UNITY_PLAYER                = "com.unity3d.player.UnityPlayer";
        private const string _CURRENT_ACTIVITY            = "currentActivity";
        private const string _PLUGIN_CLASS_NAME           = "com.deusald.deusaldjavatools.Haptics";
        private const string _IS_HAPTICS_SUPPORTED_METHOD = "isHapticSupported";
        private const string _BASE_METHOD                 = "baseVibrate";
        private const string _ADVANCED_METHOD             = "advancedVibrate";

        private static readonly AndroidJavaClass _HapticsClass = new(_PLUGIN_CLASS_NAME);
        
        private static void PerformInternalAndroid(HapticType type)
        {
            if (!TurnedOn) return;
            if (_SupportedHapticType == SupportedHapticType.NotChecked) _SupportedHapticType = IsHapticSupported();

            if (type != HapticType.Default && _SupportedHapticType == SupportedHapticType.Base) _HapticsClass.CallStatic(_BASE_METHOD,              type.ToString());
            else if (type != HapticType.Default && _SupportedHapticType == SupportedHapticType.Advanced) _HapticsClass.CallStatic(_ADVANCED_METHOD, type.ToString());
            else Handheld.Vibrate();
        }

        private static SupportedHapticType IsHapticSupported()
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass(_UNITY_PLAYER))
            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>(_CURRENT_ACTIVITY))
            {
                return (SupportedHapticType)_HapticsClass.CallStatic<int>(_IS_HAPTICS_SUPPORTED_METHOD, activity);
            }
        }

        #endif
    }
}