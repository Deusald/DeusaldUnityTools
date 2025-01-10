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
using UnityEngine;
using UnityEngine.UI;

namespace DeusaldUnityTools
{
    public static class UnityExtensions
    {
        public static int MathMod(this int a, int b)
        {
            return (Math.Abs(a * b) + a) % b;
        }

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

        public static Vector3 ClipPlaneScreenToWorld(this Camera camera, Vector3 position)
        {
            position.z = 0f;
            return camera.ScreenToWorldPoint(position);
        }

        public static void ChangeLocalX(this Transform transform, float target)
        {
            Vector3 pos = transform.localPosition;
            pos.x                   = target;
            transform.localPosition = pos;
        }
        
        public static void ChangeLocalY(this Transform transform, float target)
        {
            Vector3 pos = transform.localPosition;
            pos.y                   = target;
            transform.localPosition = pos;
        }
        
        public static void ChangeLocalZ(this Transform transform, float target)
        {
            Vector3 pos = transform.localPosition;
            pos.z                   = target;
            transform.localPosition = pos;
        }

        public static void ChangeSizeDeltaX(this RectTransform rectTransform, float target)
        {
            Vector2 sizeDelta = rectTransform.sizeDelta;
            sizeDelta.x             = target;
            rectTransform.sizeDelta = sizeDelta;
        }

        public static void ChangeSizeDeltaY(this RectTransform rectTransform, float target)
        {
            Vector2 sizeDelta = rectTransform.sizeDelta;
            sizeDelta.y             = target;
            rectTransform.sizeDelta = sizeDelta;
        }
    }
}