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
    public sealed class VisualElementTranslateTween : Tween<VisualElement, Vector2>
    {
        protected override Vector2 CurrentFromComponent()
        {
            return new Vector2(VisualElementTranslateUtils.GetPixelX(Component!), VisualElementTranslateUtils.GetPixelY(Component!));
        }

        protected override Vector2 Lerp(Vector2 from, Vector2 to, float time)
        {
            return Vector2.LerpUnclamped(from, to, time);
        }

        protected override void ApplyToComponent(Vector2 value)
        {
            Component!.style.translate = new Translate(value.x, value.y);
        }
    }

    public static partial class TweenExtensions
    {
        public static Tween<VisualElement, Vector2> TweenTranslate(this VisualElement visualElement, Vector2 from, Vector2 to)
        {
            return new VisualElementTranslateTween().SetTarget(visualElement, from, to);
        }

        public static Tween<VisualElement, Vector2> TweenTranslate(this VisualElement visualElement, Vector2 to)
        {
            return new VisualElementTranslateTween().SetTarget(visualElement, to);
        }
    }

    internal static class VisualElementTranslateUtils
    {
        /// <summary>Returns inline translate X keeping its original unit (e.g. percent) or resolved pixel value when inline style is not set.</summary>
        public static Length GetX(VisualElement visualElement)
        {
            StyleTranslate translate = visualElement.style.translate;
            return translate.keyword == StyleKeyword.Undefined ? translate.value.x : new Length(visualElement.resolvedStyle.translate.x);
        }

        /// <summary>Returns inline translate Y keeping its original unit (e.g. percent) or resolved pixel value when inline style is not set.</summary>
        public static Length GetY(VisualElement visualElement)
        {
            StyleTranslate translate = visualElement.style.translate;
            return translate.keyword == StyleKeyword.Undefined ? translate.value.y : new Length(visualElement.resolvedStyle.translate.y);
        }

        /// <summary>Returns translate X in pixels, preferring inline pixel value as resolved style is updated with one frame delay.</summary>
        public static float GetPixelX(VisualElement visualElement)
        {
            StyleTranslate translate = visualElement.style.translate;
            bool           isPixel   = translate.keyword == StyleKeyword.Undefined && translate.value.x.unit == LengthUnit.Pixel;
            return isPixel ? translate.value.x.value : visualElement.resolvedStyle.translate.x;
        }

        /// <summary>Returns translate Y in pixels, preferring inline pixel value as resolved style is updated with one frame delay.</summary>
        public static float GetPixelY(VisualElement visualElement)
        {
            StyleTranslate translate = visualElement.style.translate;
            bool           isPixel   = translate.keyword == StyleKeyword.Undefined && translate.value.y.unit == LengthUnit.Pixel;
            return isPixel ? translate.value.y.value : visualElement.resolvedStyle.translate.y;
        }
    }
}