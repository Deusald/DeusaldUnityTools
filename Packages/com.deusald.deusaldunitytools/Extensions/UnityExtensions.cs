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

using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace DeusaldUnityTools
{
    [PublicAPI]
    public static class UnityExtensions
    {
        #region Color

        public static Color SetAlpha(this Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        public static void SetAlpha(this Image image, float alpha)
        {
            Color color = image.color;
            color.a     = alpha;
            image.color = color;
        }

        public static string ToHex(this Color color) => $"#{ColorUtility.ToHtmlStringRGBA(color)}";

        public static Color FromHex(this string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color color)) return color;
            throw new ArgumentException("Invalid hex string", nameof(hex));
        }

        public static Color Invert(this Color color) => new(1 - color.r, 1 - color.g, 1 - color.b, color.a);

        #endregion Color

        #region Camera

        public static Vector3 ClipPlaneScreenToWorld(this Camera camera, Vector3 position)
        {
            position.z = 0f;
            return camera.ScreenToWorldPoint(position);
        }

        #endregion Camera

        #region Vector 2

        public static Vector2 Add(this Vector2 vector2, float x = 0, float y = 0)
        {
            return new Vector2(vector2.x + x, vector2.y + y);
        }

        public static Vector2 With(this Vector2 vector2, float? x = null, float? y = null)
        {
            return new Vector2(x ?? vector2.x, y ?? vector2.y);
        }

        #endregion Vector 2

        #region Vector 3

        public static Vector3 With(this Vector3 vector, float? x = null, float? y = null, float? z = null)
        {
            return new Vector3(x ?? vector.x, y ?? vector.y, z ?? vector.z);
        }

        public static Vector3 Add(this Vector3 vector, float x = 0, float y = 0, float z = 0)
        {
            return new Vector3(vector.x + x, vector.y + y, vector.z + z);
        }

        #endregion Vector 3

        #region Transform

        public static void SetLocalX(this Transform transform, float target)
        {
            Vector3 pos = transform.localPosition;
            pos.x                   = target;
            transform.localPosition = pos;
        }

        public static void SetLocalY(this Transform transform, float target)
        {
            Vector3 pos = transform.localPosition;
            pos.y                   = target;
            transform.localPosition = pos;
        }

        public static void SetLocalZ(this Transform transform, float target)
        {
            Vector3 pos = transform.localPosition;
            pos.z                   = target;
            transform.localPosition = pos;
        }

        public static void SetSizeDeltaX(this RectTransform rectTransform, float target)
        {
            Vector2 sizeDelta = rectTransform.sizeDelta;
            sizeDelta.x             = target;
            rectTransform.sizeDelta = sizeDelta;
        }

        public static void SetSizeDeltaY(this RectTransform rectTransform, float target)
        {
            Vector2 sizeDelta = rectTransform.sizeDelta;
            sizeDelta.y             = target;
            rectTransform.sizeDelta = sizeDelta;
        }

        #endregion Transform
    }
}