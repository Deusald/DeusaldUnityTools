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

using JetBrains.Annotations;
using UnityEngine;

namespace DeusaldUnityTools
{
    [PublicAPI]
    public static class StringExtensions
    {
        public static void SaveToClipboard(this string text)
        {
            GUIUtility.systemCopyBuffer = text;
        }
        
        public static string RichColor(this string text, string color)                    => $"<color={color}>{text}</color>";
        public static string RichSize(this string text, int size)                         => $"<size={size}>{text}</size>";
        public static string RichBold(this string text)                                   => $"<b>{text}</b>";
        public static string RichItalic(this string text)                                 => $"<i>{text}</i>";
        public static string RichUnderline(this string text)                              => $"<u>{text}</u>";
        public static string RichStrikethrough(this string text)                          => $"<s>{text}</s>";
        public static string RichFont(this string text, string font)                      => $"<font={font}>{text}</font>";
        public static string RichAlign(this string text, string align)                    => $"<align={align}>{text}</align>";
        public static string RichGradient(this string text, string color1, string color2) => $"<gradient={color1},{color2}>{text}</gradient>";
        public static string RichRotation(this string text, float angle)                  => $"<rotate={angle}>{text}</rotate>";
        public static string RichSpace(this string text, float space)                     => $"<space={space}>{text}</space>";
    }
}