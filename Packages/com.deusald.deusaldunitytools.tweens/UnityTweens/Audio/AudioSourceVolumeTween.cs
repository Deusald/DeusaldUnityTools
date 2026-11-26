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

using JetBrains.Annotations;
using UnityEngine;

namespace DeusaldUnityTools
{
    [PublicAPI]
    public sealed class AudioSourceVolumeTween : Tween<AudioSource, float>
    {
        protected override float CurrentFromComponent()
        {
            return Component!.volume;
        }

        protected override float Lerp(float from, float to, float time)
        {
            return Mathf.LerpUnclamped(from, to, time);
        }

        protected override void ApplyToComponent(float value)
        {
            Component!.volume = value;
        }
    }

    public static partial class TweenExtensions
    {
        public static Tween<AudioSource, float> TweenAudioSourceVolume(this AudioSource audioSource, float from, float to)
        {
            return new AudioSourceVolumeTween().SetTarget(audioSource, from, to);
        }
        
        public static Tween<AudioSource, float> TweenAudioSourceVolume(this AudioSource audioSource, float to)
        {
            return new AudioSourceVolumeTween().SetTarget(audioSource, to);
        }
    }
}