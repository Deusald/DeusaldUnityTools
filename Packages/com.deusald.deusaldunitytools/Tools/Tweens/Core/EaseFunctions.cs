// Tweens module based on unity-tweens by Jeffrey Lanters - https://github.com/jeffreylanters/unity-tweens
// MIT License
// Copyright (c) 2020 Jeffrey Lanters

// DeusaldUnityTools:
// Copyright (c) 2020 Adam "Deusald" Orliński

// MIT License

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

using System;
using UnityEngine;
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace DeusaldUnityTools
{
    public static class EaseFunctions
    {
        private const float _CONSTANT_A = 1.70158f;
        private const float _CONSTANT_B = _CONSTANT_A * 1.525f;
        private const float _CONSTANT_C = _CONSTANT_A + 1f;
        private const float _CONSTANT_D = 2f * Mathf.PI / 3f;
        private const float _CONSTANT_E = 2f * Mathf.PI / 4.5f;
        private const float _CONSTANT_F = 7.5625f;
        private const float _CONSTANT_G = 2.75f;

        public static EaseFunctionDelegate GetFunction(EaseType easeType)
        {
            return easeType switch
            {
                EaseType.Linear       => Linear,
                EaseType.SineIn       => SineIn,
                EaseType.SineOut      => SineOut,
                EaseType.SineInOut    => SineInOut,
                EaseType.QuadIn       => QuadIn,
                EaseType.QuadOut      => QuadOut,
                EaseType.QuadInOut    => QuadInOut,
                EaseType.CubicIn      => CubicIn,
                EaseType.CubicOut     => CubicOut,
                EaseType.CubicInOut   => CubicInOut,
                EaseType.QuartIn      => QuartIn,
                EaseType.QuartOut     => QuartOut,
                EaseType.QuartInOut   => QuartInOut,
                EaseType.QuintIn      => QuintIn,
                EaseType.QuintOut     => QuintOut,
                EaseType.QuintInOut   => QuintInOut,
                EaseType.ExpoIn       => ExpoIn,
                EaseType.ExpoOut      => ExpoOut,
                EaseType.ExpoInOut    => ExpoInOut,
                EaseType.CircIn       => CircIn,
                EaseType.CircOut      => CircOut,
                EaseType.CircInOut    => CircInOut,
                EaseType.BackIn       => BackIn,
                EaseType.BackOut      => BackOut,
                EaseType.BackInOut    => BackInOut,
                EaseType.ElasticIn    => ElasticIn,
                EaseType.ElasticOut   => ElasticOut,
                EaseType.ElasticInOut => ElasticInOut,
                EaseType.BounceIn     => BounceIn,
                EaseType.BounceOut    => BounceOut,
                EaseType.BounceInOut  => BounceInOut,
                _                     => throw new NotImplementedException($"EaseType {easeType} not implemented"),
            };
        }

        private static float Linear(float time)
        {
            return time;
        }

        private static float SineIn(float time)
        {
            return 1f - Mathf.Cos((time * Mathf.PI) / 2f);
        }

        private static float SineOut(float time)
        {
            return Mathf.Sin((time * Mathf.PI) / 2f);
        }

        private static float SineInOut(float time)
        {
            return -(Mathf.Cos(Mathf.PI * time) - 1f) / 2f;
        }

        private static float QuadIn(float time)
        {
            return time * time;
        }

        private static float QuadOut(float time)
        {
            return 1 - (1 - time) * (1 - time);
        }

        private static float QuadInOut(float time)
        {
            return time < 0.5f ? 2 * time * time : 1 - Mathf.Pow(-2 * time + 2, 2) / 2;
        }

        private static float CubicIn(float time)
        {
            return time * time * time;
        }

        private static float CubicOut(float time)
        {
            return 1 - Mathf.Pow(1 - time, 3);
        }

        private static float CubicInOut(float time)
        {
            return time < 0.5f ? 4 * time * time * time : 1 - Mathf.Pow(-2 * time + 2, 3) / 2;
        }

        private static float QuartIn(float time)
        {
            return time * time * time * time;
        }

        private static float QuartOut(float time)
        {
            return 1 - Mathf.Pow(1 - time, 4);
        }

        private static float QuartInOut(float time)
        {
            return time < 0.5 ? 8 * time * time * time * time : 1 - Mathf.Pow(-2 * time + 2, 4) / 2;
        }

        private static float QuintIn(float time)
        {
            return time * time * time * time * time;
        }

        private static float QuintOut(float time)
        {
            return 1 - Mathf.Pow(1 - time, 5);
        }

        private static float QuintInOut(float time)
        {
            return time < 0.5f ? 16 * time * time * time * time * time : 1 - Mathf.Pow(-2 * time + 2, 5) / 2;
        }

        private static float ExpoIn(float time)
        {
            return time == 0 ? 0 : Mathf.Pow(2, 10 * time - 10);
        }

        private static float ExpoOut(float time)
        {
            return time == 1 ? 1 : 1 - Mathf.Pow(2, -10 * time);
        }

        private static float ExpoInOut(float time)
        {
            return time == 0 ? 0 : time == 1 ? 1 : time < 0.5 ? Mathf.Pow(2, 20 * time - 10) / 2 : (2 - Mathf.Pow(2, -20 * time + 10)) / 2;
        }

        private static float CircIn(float time)
        {
            return 1 - Mathf.Sqrt(1 - Mathf.Pow(time, 2));
        }

        private static float CircOut(float time)
        {
            return Mathf.Sqrt(1 - Mathf.Pow(time - 1, 2));
        }

        private static float CircInOut(float time)
        {
            return time < 0.5 ? (1 - Mathf.Sqrt(1 - Mathf.Pow(2 * time, 2))) / 2 : (Mathf.Sqrt(1 - Mathf.Pow(-2 * time + 2, 2)) + 1) / 2;
        }

        private static float BackIn(float time)
        {
            return _CONSTANT_C * time * time * time - _CONSTANT_A * time * time;
        }

        private static float BackOut(float time)
        {
            return 1f + _CONSTANT_C * Mathf.Pow(time - 1, 3) + _CONSTANT_A * Mathf.Pow(time - 1, 2);
        }

        private static float BackInOut(float time)
        {
            return time < 0.5 ? Mathf.Pow(2 * time, 2) * ((_CONSTANT_B + 1) * 2 * time - _CONSTANT_B) / 2 : (Mathf.Pow(2 * time - 2, 2) * ((_CONSTANT_B + 1) * (time * 2 - 2) + _CONSTANT_B) + 2) / 2;
        }

        private static float ElasticIn(float time)
        {
            return time == 0 ? 0 : time == 1 ? 1 : -Mathf.Pow(2, 10 * time - 10) * Mathf.Sin((time * 10f - 10.75f) * _CONSTANT_D);
        }

        private static float ElasticOut(float time)
        {
            return time == 0 ? 0 : time == 1 ? 1 : Mathf.Pow(2, -10 * time) * Mathf.Sin((time * 10 - 0.75f) * _CONSTANT_D) + 1;
        }

        private static float ElasticInOut(float time)
        {
            return time == 0 ? 0 : time == 1 ? 1 : time < 0.5 ? -(Mathf.Pow(2, 20 * time - 10) * Mathf.Sin((20 * time - 11.125f) * _CONSTANT_E)) / 2 : Mathf.Pow(2, -20 * time + 10) * Mathf.Sin((20 * time - 11.125f) * _CONSTANT_E) / 2 + 1;
        }

        private static float BounceIn(float time)
        {
            return 1 - BounceOut(1 - time);
        }

        private static float BounceOut(float time)
        {
            if (time < 1 / _CONSTANT_G)
                return _CONSTANT_F * time * time;
            else if (time < 2 / _CONSTANT_G)
                return _CONSTANT_F * (time -= 1.5f / _CONSTANT_G) * time + 0.75f;
            else if (time < 2.5f / _CONSTANT_G)
                return _CONSTANT_F * (time -= 2.25f / _CONSTANT_G) * time + 0.9375f;
            else
                return _CONSTANT_F * (time -= 2.625f / _CONSTANT_G) * time + 0.984375f;
        }

        private static float BounceInOut(float time)
        {
            return time < 0.5f ? (1 - BounceOut(1 - 2 * time)) / 2 : (1 + BounceOut(2 * time - 1)) / 2;
        }
    }
}