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
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DeusaldUnityTools
{
    /// <summary>
    /// Finds the newest version of a local package's source.
    /// </summary>
    /// <description>
    /// OpenUPM is asked through its npm registry (dist-tags.latest). The Asset Store has no public
    /// API, so its product page is scraped: the page embeds the product data as JSON, including
    /// "currentVersion". That format is not a contract and may change without notice.
    /// </description>
    public static class LocalPackageUpdateChecker
    {
        #region Variables

        public const string OPEN_UPM_REGISTRY = "https://package.openupm.com/";

        private const string _USER_AGENT      = "DeusaldUnityTools-LocalPackages";
        private const int    _TIMEOUT_SECONDS = 30;

        private static readonly HttpClient _Client = CreateClient();

        private static readonly Regex _OpenUpmLatest    = new Regex("\"dist-tags\"\\s*:\\s*\\{[^}]*?\"latest\"\\s*:\\s*\"([^\"]+)\"", RegexOptions.Compiled);
        private static readonly Regex _ProductStart     = new Regex("\\{\"id\":\"\\d+\",\"productId\"",                                   RegexOptions.Compiled);
        private static readonly Regex _InlineVersion    = new Regex("\"currentVersion\":\\{\"id\":\"\\d+\",\"name\":\"([^\"]*)\"(?:,\"publishedDate\":\"([^\"]*)\")?", RegexOptions.Compiled);
        private static readonly Regex _VersionReference = new Regex("\"currentVersion\":\\{\"type\":\"id\",\"id\":\\[\"ProductVersion\",\"(\\d+)\"\\]", RegexOptions.Compiled);

        #endregion Variables

        #region Public Methods

        /// <summary>
        /// Checks the package's source. Never throws: failures are returned in <see cref="LocalPackageUpdateInfo.error"/>.
        /// </summary>
        public static async Task<LocalPackageUpdateInfo> CheckAsync(LocalPackage package)
        {
            LocalPackageUpdateInfo info = new LocalPackageUpdateInfo { source = package.Source };

            try
            {
                switch (package.SourceType)
                {
                    case LocalPackageSourceType.AssetStore:
                        (info.latestVersion, info.publishedDate) = await GetAssetStoreVersionAsync(package.Source);
                        break;
                    case LocalPackageSourceType.OpenUpm:
                        (info.latestVersion, info.publishedDate) = await GetOpenUpmVersionAsync(package);
                        break;
                    default:
                        info.error = "Unsupported source. Use an Asset Store page or an OpenUPM package URL.";
                        break;
                }
            }
            catch (TaskCanceledException)
            {
                info.error = $"The request timed out after {_TIMEOUT_SECONDS} s.";
            }
            catch (Exception e)
            {
                info.error = e.Message;
            }

            info.checkedAtTicks = DateTime.UtcNow.Ticks;
            return info;
        }

        /// <summary>
        /// Reads the current version of the product from the HTML of its Asset Store page.
        /// </summary>
        public static (string version, string publishedDate) ParseAssetStoreVersion(string html, string productId)
        {
            // The page holds the product's data more than once (inline and as a normalized cache entry)
            // next to related products, so each match is limited to its own product object.
            string productStart = "{\"id\":\"" + productId + "\",\"productId\"";
            int    index        = html.IndexOf(productStart, StringComparison.Ordinal);

            while (index >= 0)
            {
                int   contentStart = index + productStart.Length;
                Match next         = _ProductStart.Match(html, contentStart);
                int   length       = (next.Success ? next.Index : html.Length) - index;

                Match inline = _InlineVersion.Match(html, index, length);
                if (inline.Success) return (inline.Groups[1].Value, inline.Groups[2].Value);

                Match reference = _VersionReference.Match(html, index, length);
                if (reference.Success)
                {
                    Regex versionEntry = new Regex("\"id\":\"" + reference.Groups[1].Value
                                                 + "\",\"name\":\"([^\"]*)\"(?:,\"publishedDate\":\"([^\"]*)\")?");
                    Match resolved = versionEntry.Match(html);
                    if (resolved.Success) return (resolved.Groups[1].Value, resolved.Groups[2].Value);
                }

                index = html.IndexOf(productStart, contentStart, StringComparison.Ordinal);
            }

            throw new FormatException("Can't find the current version on the Asset Store page. The page layout may have changed.");
        }

        #endregion Public Methods

        #region Private Methods

        private static HttpClient CreateClient()
        {
            HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(_TIMEOUT_SECONDS) };
            client.DefaultRequestHeaders.UserAgent.ParseAdd(_USER_AGENT);
            return client;
        }

        private static async Task<(string version, string publishedDate)> GetAssetStoreVersionAsync(string source)
        {
            if (!LocalPackageSources.TryGetAssetStoreId(source, out string? productId))
                throw new FormatException("Can't find the product id in the Asset Store URL. It should end with -<id>, e.g. editor-console-pro-11889.");

            string html = await GetStringAsync(LocalPackageSources.GetPageUrl(source), "Asset Store page");
            return ParseAssetStoreVersion(html, productId);
        }

        private static Task<(string version, string publishedDate)> GetOpenUpmVersionAsync(LocalPackage package)
        {
            return GetOpenUpmLatestAsync(GetOpenUpmPackageName(package));
        }

        #endregion Private Methods

        #region Internal Methods

        /// <summary>Package name on OpenUPM: from the source URL, or the package.json name.</summary>
        internal static string GetOpenUpmPackageName(LocalPackage package)
        {
            return LocalPackageSources.TryGetOpenUpmPackageName(package.Source, out string? parsedName)
                       ? parsedName
                       : package.PackageName;
        }

        internal static async Task<(string version, string publishedDate)> GetOpenUpmLatestAsync(string packageName)
        {
            string json   = await GetStringAsync(OPEN_UPM_REGISTRY + Uri.EscapeDataString(packageName), $"OpenUPM package {packageName}");
            Match  latest = _OpenUpmLatest.Match(json);
            if (!latest.Success) throw new FormatException($"OpenUPM returned no latest version for {packageName}.");

            string version = latest.Groups[1].Value;
            Match  time    = Regex.Match(json, "\"time\"\\s*:\\s*\\{[^}]*?\"" + Regex.Escape(version) + "\"\\s*:\\s*\"([^\"]+)\"");
            return (version, time.Success ? time.Groups[1].Value : string.Empty);
        }

        /// <summary>Latest version of the package on OpenUPM, or null when OpenUPM doesn't have the package.</summary>
        internal static async Task<string?> TryGetOpenUpmLatestVersionAsync(string packageName)
        {
            string? json = await TryGetStringAsync(OPEN_UPM_REGISTRY + Uri.EscapeDataString(packageName), $"OpenUPM package {packageName}");
            if (json == null) return null;

            Match latest = _OpenUpmLatest.Match(json);
            return latest.Success ? latest.Groups[1].Value : null;
        }

        internal static async Task<string> GetStringAsync(string url, string what)
        {
            return await TryGetStringAsync(url, what) ?? throw new Exception($"{what} not found (404).");
        }

        /// <summary>Like <see cref="GetStringAsync"/>, but null when the server answers 404.</summary>
        internal static async Task<string?> TryGetStringAsync(string url, string what)
        {
            using HttpResponseMessage response = await _Client.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.NotFound) return null;
            if (!response.IsSuccessStatusCode) throw new Exception($"{what} returned {(int)response.StatusCode} {response.ReasonPhrase}.");
            return await response.Content.ReadAsStringAsync();
        }

        internal static async Task<byte[]> GetBytesAsync(string url, string what)
        {
            using HttpResponseMessage response = await _Client.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.NotFound) throw new Exception($"{what} not found (404).");
            if (!response.IsSuccessStatusCode) throw new Exception($"{what} returned {(int)response.StatusCode} {response.ReasonPhrase}.");
            return await response.Content.ReadAsByteArrayAsync();
        }

        #endregion Internal Methods
    }
}
