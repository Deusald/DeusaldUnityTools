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
        public static bool TurnedOn { get; set; } = true;

        private static SupportedHapticType _SupportedHapticType = SupportedHapticType.NotChecked;

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
        
        private const long _LIGHT_DURATION   = 20;
        private const long _MEDIUM_DURATION  = 40;
        private const long _HEAVY_DURATION   = 80;
        private const int  _LIGHT_AMPLITUDE  = 40;
        private const int  _MEDIUM_AMPLITUDE = 120;
        private const int  _HEAVY_AMPLITUDE  = 255;

        private static readonly Dictionary<HapticType, long[]> _Patterns = new()
        {
            { HapticType.Light, new[] { _LIGHT_DURATION } },
            { HapticType.Medium, new[] { _MEDIUM_DURATION } },
            { HapticType.Heavy, new[] { _HEAVY_DURATION } },
            { HapticType.Selection, new[] { _LIGHT_DURATION } },
            { HapticType.Success, new[] { 0, _LIGHT_DURATION, _LIGHT_DURATION, _HEAVY_DURATION } },
            { HapticType.Warning, new[] { 0, _HEAVY_DURATION, _LIGHT_DURATION, _MEDIUM_DURATION } },
            { HapticType.Error, new[] { 0, _MEDIUM_DURATION, _LIGHT_DURATION, _MEDIUM_DURATION, _LIGHT_DURATION, _HEAVY_DURATION, _LIGHT_DURATION, _LIGHT_DURATION } }
        };

        private static readonly Dictionary<HapticType, int[]> _Amplitudes = new()
        {
            { HapticType.Light, new[] { _LIGHT_AMPLITUDE } },
            { HapticType.Medium, new[] { _MEDIUM_AMPLITUDE } },
            { HapticType.Heavy, new[] { _HEAVY_AMPLITUDE } },
            { HapticType.Selection, new[] { _LIGHT_AMPLITUDE } },
            { HapticType.Success, new[] { 0, _LIGHT_AMPLITUDE, 0, _HEAVY_AMPLITUDE } },
            { HapticType.Warning, new[] { 0, _HEAVY_AMPLITUDE, 0, _MEDIUM_AMPLITUDE } },
            { HapticType.Error, new[] { 0, _MEDIUM_AMPLITUDE, 0, _MEDIUM_AMPLITUDE, 0, _HEAVY_AMPLITUDE, 0, _LIGHT_AMPLITUDE } },
        };

        private static AndroidJavaClass  _UnityPlayer     = new("com.unity3d.player.UnityPlayer");
        private static AndroidJavaObject _CurrentActivity = _UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        private static AndroidJavaObject _AndroidVibrator = _CurrentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
        private static AndroidJavaClass  _VibrationEffectClass;

        private static void PerformInternalAndroid(HapticType type)
        {
            if (!TurnedOn) return;
            if (_SupportedHapticType == SupportedHapticType.NotChecked) SetHapticSupportedAndroid();

            if (type != HapticType.Default && _SupportedHapticType == SupportedHapticType.Base) _AndroidVibrator?.Call("vibrate", _Patterns[type]);
            else if (type != HapticType.Default && _SupportedHapticType == SupportedHapticType.Advanced)
            {
                if (_Patterns[type].Length == 1)
                {
                    AndroidJavaObject effect = _VibrationEffectClass.CallStatic<AndroidJavaObject>("createOneShot", _Patterns[type][0], _Amplitudes[type][0]);
                    _AndroidVibrator?.Call("vibrate", effect);
                }
                else
                {
                    AndroidJavaObject effect = _VibrationEffectClass.CallStatic<AndroidJavaObject>("createWaveform", _Patterns[type], _Amplitudes[type], -1);
                    _AndroidVibrator?.Call("vibrate", effect);
                }
            }
            else Handheld.Vibrate();
        }

        private static void SetHapticSupportedAndroid()
        {
            int apiLevel = int.Parse(SystemInfo.operatingSystem.Substring(SystemInfo.operatingSystem.IndexOf("-", StringComparison.Ordinal) + 1, 3));
            _SupportedHapticType = apiLevel >= 26 ? SupportedHapticType.Advanced : SupportedHapticType.Base;
            if (_SupportedHapticType != SupportedHapticType.Advanced) return;
            _VibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect");
            if (_VibrationEffectClass == null) _SupportedHapticType = SupportedHapticType.Base;
        }
        
        #endif
    }
}