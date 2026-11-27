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

namespace DeusaldUnityTools
{
    /// <summary>
    /// Contents of the marker file that makes a package in the Packages folder a local package.
    /// </summary>
    [Serializable]
    public class LocalPackageMarker
    {
        public const string FILE_NAME = "localpackage.json";

        /// <summary>Asset Store page or OpenUPM package URL the package comes from.</summary>
        public string source = string.Empty;

        /// <summary>Upstream version kept locally. When empty, the package.json version is used.</summary>
        public string version = string.Empty;

        /// <summary>Whether compiler warnings of the package's assemblies are silenced with csc.rsp files.</summary>
        public bool silenceWarnings;
    }
}
