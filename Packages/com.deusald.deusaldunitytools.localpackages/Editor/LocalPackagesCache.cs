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
using System.IO;
using UnityEngine;

namespace DeusaldUnityTools
{
    /// <summary>
    /// Update check results, kept per project in the Library folder.
    /// </summary>
    [Serializable]
    public class LocalPackagesCache
    {
        #region Variables

        private const string _CACHE_PATH = "Library/DeusaldUnityTools/LocalPackagesCache.json";

        public List<LocalPackageUpdateInfo> entries = new List<LocalPackageUpdateInfo>();

        #endregion Variables

        #region Public Methods

        public static LocalPackagesCache Load()
        {
            try
            {
                if (File.Exists(_CACHE_PATH))
                    return JsonUtility.FromJson<LocalPackagesCache>(File.ReadAllText(_CACHE_PATH)) ?? new LocalPackagesCache();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Local Packages: can't read the update cache: {e.Message}");
            }

            return new LocalPackagesCache();
        }

        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_CACHE_PATH) ?? "Library");
            File.WriteAllText(_CACHE_PATH, JsonUtility.ToJson(this, true));
        }

        public LocalPackageUpdateInfo? Get(string? source)
        {
            return string.IsNullOrEmpty(source) ? null : entries.Find(entry => entry.source == source);
        }

        public void Set(LocalPackageUpdateInfo info)
        {
            entries.RemoveAll(entry => entry.source == info.source);
            entries.Add(info);
        }

        #endregion Public Methods
    }
}
