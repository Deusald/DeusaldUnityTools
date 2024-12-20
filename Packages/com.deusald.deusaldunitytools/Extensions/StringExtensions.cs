using UnityEngine;

namespace DeusaldUnityTools
{
    public static class StringExtensions
    {
        public static void SaveToClipboard(this string text)
        {
            GUIUtility.systemCopyBuffer = text;
        }
    }
}