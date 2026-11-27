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
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace DeusaldUnityTools
{
    /// <summary>
    /// Recognises and parses local package source URLs.
    /// </summary>
    public static class LocalPackageSources
    {
        #region Variables

        private const string _ASSET_STORE_HOST       = "assetstore.unity.com";
        private const string _OPEN_UPM_HOST          = "openupm.com";
        private const string _OPEN_UPM_REGISTRY_HOST = "package.openupm.com";

        private static readonly Regex _AssetStoreId = new Regex(@"-(\d+)$", RegexOptions.Compiled);
        private static readonly Regex _NumericId    = new Regex(@"^\d+$",   RegexOptions.Compiled);

        #endregion Variables

        #region Public Methods

        public static LocalPackageSourceType GetSourceType(string? source)
        {
            if (!TryParse(source, out Uri? uri)) return LocalPackageSourceType.Unknown;

            string host = uri.Host.ToLowerInvariant();
            if (host == _ASSET_STORE_HOST) return LocalPackageSourceType.AssetStore;
            if (host == _OPEN_UPM_HOST || host.EndsWith("." + _OPEN_UPM_HOST)) return LocalPackageSourceType.OpenUpm;
            return LocalPackageSourceType.Unknown;
        }

        /// <summary>
        /// Reads the product id from an Asset Store URL, e.g. .../editor-console-pro-11889 or .../content/11889.
        /// </summary>
        public static bool TryGetAssetStoreId(string? source, [NotNullWhen(true)] out string? productId)
        {
            productId = null;
            if (!TryParse(source, out Uri? uri)) return false;

            string[] segments = GetSegments(uri);
            if (segments.Length == 0) return false;

            string last  = segments[segments.Length - 1];
            Match  match = _AssetStoreId.Match(last);
            if (match.Success)
            {
                productId = match.Groups[1].Value;
                return true;
            }

            if (!_NumericId.IsMatch(last)) return false;
            productId = last;
            return true;
        }

        /// <summary>
        /// Reads the package name from an OpenUPM URL, e.g. https://openupm.com/packages/com.vendor.package/.
        /// </summary>
        public static bool TryGetOpenUpmPackageName(string? source, [NotNullWhen(true)] out string? packageName)
        {
            packageName = null;
            if (!TryParse(source, out Uri? uri)) return false;

            string[] segments = GetSegments(uri);
            if (uri.Host.Equals(_OPEN_UPM_REGISTRY_HOST, StringComparison.OrdinalIgnoreCase))
            {
                if (segments.Length == 0) return false;
                packageName = segments[0];
                return true;
            }

            for (int x = 0; x < segments.Length - 1; ++x)
            {
                if (!segments[x].Equals("packages", StringComparison.OrdinalIgnoreCase)) continue;
                packageName = segments[x + 1];
                return true;
            }

            return false;
        }

        /// <summary>
        /// Source URL without the query and fragment, e.g. without #releases.
        /// </summary>
        public static string GetPageUrl(string source)
        {
            return TryParse(source, out Uri? uri) ? uri.GetLeftPart(UriPartial.Path) : source;
        }

        #endregion Public Methods

        #region Private Methods

        private static bool TryParse(string? source, [NotNullWhen(true)] out Uri? uri)
        {
            uri = null;
            string trimmed = source?.Trim() ?? string.Empty;
            if (trimmed.Length == 0 || !Uri.TryCreate(trimmed, UriKind.Absolute, out Uri? parsed) || parsed == null) return false;
            if (parsed.Scheme != Uri.UriSchemeHttps && parsed.Scheme != Uri.UriSchemeHttp) return false;
            uri = parsed;
            return true;
        }

        private static string[] GetSegments(Uri uri)
        {
            return uri.AbsolutePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        }

        #endregion Private Methods
    }
}
