﻿// Tweens module based on unity-tweens by Jeffrey Lanters - https://github.com/jeffreylanters/unity-tweens
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
    public sealed class AudioSourcePriorityTween : Tween<AudioSource, int>
    {
        protected override int CurrentFromComponent()
        {
            return Component!.priority;
        }

        protected override int Lerp(int from, int to, float time)
        {
            return Mathf.RoundToInt(Mathf.LerpUnclamped(from, to, time));
        }

        protected override void ApplyToComponent(int value)
        {
            Component!.priority = value;
        }
    }

    public static partial class TweenExtensions
    {
        public static Tween<AudioSource, int> TweenAudioSourcePriority(this AudioSource audioSource, int from, int to)
        {
            return new AudioSourcePriorityTween().SetTarget(audioSource, from, to);
        }
        
        public static Tween<AudioSource, int> TweenAudioSourcePriority(this AudioSource audioSource, int to)
        {
            return new AudioSourcePriorityTween().SetTarget(audioSource, to);
        }
    }
}