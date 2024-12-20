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