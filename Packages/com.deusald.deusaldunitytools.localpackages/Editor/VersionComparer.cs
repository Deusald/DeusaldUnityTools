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

#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

namespace DeusaldUnityTools
{
    /// <summary>
    /// Compares loosely formatted versions: semver (1.2.3-preview.1) and Asset Store style (3.984, v2.1b).
    /// </summary>
    public static class VersionComparer
    {
        #region Variables

        private static readonly char[] _Separators     = { '.', '-', '+', '_', ' ' };
        private static readonly Regex  _NumericVersion = new Regex(@"^\d+(\.\d+){0,2}$",                          RegexOptions.Compiled);
        private static readonly Regex  _SemVer         = new Regex(@"^\d+\.\d+\.\d+(?:[-+][0-9A-Za-z.\-+]+)?$", RegexOptions.Compiled);

        #endregion Variables

        #region Public Methods

        /// <summary>
        /// Less than zero when left is older than right, zero when equal, more than zero when newer.
        /// </summary>
        public static int Compare(string? left, string? right)
        {
            string[] leftParts  = Split(left);
            string[] rightParts = Split(right);
            int      count      = Math.Max(leftParts.Length, rightParts.Length);

            for (int x = 0; x < count; ++x)
            {
                string? leftPart  = x < leftParts.Length ? leftParts[x] : null;
                string? rightPart = x < rightParts.Length ? rightParts[x] : null;
                int     result    = ComparePart(leftPart, rightPart);
                if (result != 0) return result;
            }

            return 0;
        }

        /// <summary>
        /// Turns a version into a valid package.json (semver) version, e.g. Asset Store's 3.984 into 3.984.0.
        /// </summary>
        public static bool TryNormalizeToSemVer(string? version, [NotNullWhen(true)] out string? semVer)
        {
            semVer = null;
            string trimmed = (version ?? string.Empty).Trim().TrimStart('v', 'V');

            if (_SemVer.IsMatch(trimmed))
            {
                semVer = trimmed;
                return true;
            }

            if (!_NumericVersion.IsMatch(trimmed)) return false;

            List<string> parts = trimmed.Split('.').ToList();
            while (parts.Count < 3) parts.Add("0");
            semVer = string.Join(".", parts);
            return true;
        }

        #endregion Public Methods

        #region Private Methods

        private static string[] Split(string? version)
        {
            string trimmed = (version ?? string.Empty).Trim().TrimStart('v', 'V');
            return trimmed.Split(_Separators, StringSplitOptions.RemoveEmptyEntries);
        }

        private static int ComparePart(string? left, string? right)
        {
            bool leftNumeric  = long.TryParse(left,  out long leftNumber);
            bool rightNumeric = long.TryParse(right, out long rightNumber);

            // A missing part counts as 0, but a release (1.0) is newer than its pre-release (1.0-preview).
            if (left == null) return rightNumeric ? 0L.CompareTo(rightNumber) : 1;
            if (right == null) return leftNumeric ? leftNumber.CompareTo(0L) : -1;

            if (leftNumeric && rightNumeric) return leftNumber.CompareTo(rightNumber);
            if (leftNumeric) return 1;
            if (rightNumeric) return -1;
            return string.Compare(left, right, StringComparison.OrdinalIgnoreCase);
        }

        #endregion Private Methods
    }
}
