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
    public sealed class LocalScaleTween : Tween<Transform, Vector3>
    {
        protected override Vector3 CurrentFromComponent()
        {
            return Component!.lossyScale;
        }

        protected override Vector3 Lerp(Vector3 from, Vector3 to, float time)
        {
            return Vector3.LerpUnclamped(from, to, time);
        }

        protected override void ApplyToComponent(Vector3 value)
        {
            Component!.localScale = value;
        }
    }

    public static partial class TweenExtensions
    {
        public static Tween<Transform, Vector3> TweenLocalScale(this Transform transform, Vector3 from, Vector3 to)
        {
            return new LocalScaleTween().SetTarget(transform, from, to);
        }
        
        public static Tween<Transform, Vector3> TweenLocalScale(this Transform transform, Vector3 to)
        {
            return new LocalScaleTween().SetTarget(transform, to);
        }
    }
}