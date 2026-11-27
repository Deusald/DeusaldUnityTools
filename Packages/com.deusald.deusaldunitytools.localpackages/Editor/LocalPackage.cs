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

namespace DeusaldUnityTools
{
    /// <summary>
    /// A package folder inside the Packages folder, with or without a local package marker.
    /// </summary>
    public class LocalPackage
    {
        #region Properties

        /// <summary>Project relative folder, e.g. Packages/com.vendor.package.</summary>
        public string FolderPath { get; }

        public string              FolderName      { get; }
        public string              PackageName     { get; }
        public string              DisplayName     { get; }
        public string?             ManifestVersion { get; }
        public LocalPackageMarker? Marker          { get; }

        public bool                   IsMarked   => Marker != null;
        public string                 Source     => Marker?.source?.Trim() ?? string.Empty;
        public LocalPackageSourceType SourceType => LocalPackageSources.GetSourceType(Source);

        public string InstalledVersion => Marker?.version?.Trim() is { Length: > 0 } markerVersion
                                              ? markerVersion
                                              : ManifestVersion ?? string.Empty;

        #endregion Properties

        #region Init Methods

        public LocalPackage(string folderPath, string folderName, string packageName, string displayName,
                            string? manifestVersion, LocalPackageMarker? marker)
        {
            FolderPath      = folderPath;
            FolderName      = folderName;
            PackageName     = packageName;
            DisplayName     = displayName;
            ManifestVersion = manifestVersion;
            Marker          = marker;
        }

        #endregion Init Methods
    }
}
