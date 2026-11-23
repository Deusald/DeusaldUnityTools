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
using UnityEngine.UIElements;

namespace DeusaldUnityTools
{
    [PublicAPI]
    public sealed class VisualElementBackgroundColorTween : Tween<VisualElement, Color>
    {
        protected override Color CurrentFromComponent()
        {
            return VisualElementColorUtils.GetBackgroundColor(Component!);
        }

        protected override Color Lerp(Color from, Color to, float time)
        {
            return Color.LerpUnclamped(from, to, time);
        }

        protected override void ApplyToComponent(Color value)
        {
            Component!.style.backgroundColor = value;
        }
    }

    public static partial class TweenExtensions
    {
        public static Tween<VisualElement, Color> TweenBackgroundColor(this VisualElement visualElement, Color from, Color to)
        {
            return new VisualElementBackgroundColorTween().SetTarget(visualElement, from, to);
        }

        public static Tween<VisualElement, Color> TweenBackgroundColor(this VisualElement visualElement, Color to)
        {
            return new VisualElementBackgroundColorTween().SetTarget(visualElement, to);
        }
    }

    internal static class VisualElementColorUtils
    {
        /// <summary>Returns inline background color when set (resolved style is updated with one frame delay) or resolved background color otherwise.</summary>
        public static Color GetBackgroundColor(VisualElement visualElement)
        {
            StyleColor backgroundColor = visualElement.style.backgroundColor;
            return backgroundColor.keyword == StyleKeyword.Undefined ? backgroundColor.value : visualElement.resolvedStyle.backgroundColor;
        }
    }
}