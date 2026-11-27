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
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace DeusaldUnityTools
{
    /// <summary>
    /// Finds local packages in the project's Packages folder and reads or writes their marker files.
    /// </summary>
    public static class LocalPackagesRegistry
    {
        #region Types

        [Serializable]
        private class PackageManifest
        {
            public string? name        = null;
            public string? displayName = null;
            public string? version     = null;
        }

        #endregion Types

        #region Variables

        private const string _PACKAGES_FOLDER = "Packages";
        private const string _MANIFEST_FILE   = "package.json";
        private const string _RESPONSE_FILE   = "csc.rsp";
        private const string _SILENCE_OPTION  = "-warn:0";

        private static readonly Regex _ManifestVersion = new Regex("(\"version\"\\s*:\\s*\")[^\"]*(\")", RegexOptions.Compiled);

        #endregion Variables

        #region Public Methods

        /// <summary>Packages in the Packages folder that have a marker file.</summary>
        public static List<LocalPackage> FindLocalPackages()
        {
            return FindPackages(true);
        }

        /// <summary>Embedded packages in the Packages folder that have no marker file yet.</summary>
        public static List<LocalPackage> FindUnmarkedPackages()
        {
            return FindPackages(false);
        }

        /// <summary>The embedded package with this package.json name, marked or not.</summary>
        public static LocalPackage? FindPackageByName(string packageName)
        {
            return FindPackages(null).Find(package => package.PackageName == packageName);
        }

        /// <summary>Reads the package in a folder of the Packages folder, e.g. Packages/com.vendor.package.</summary>
        public static LocalPackage? LoadPackage(string directory)
        {
            return LoadPackageFromDirectory(Path.GetFullPath(directory));
        }

        public static string GetPackagesFolder()
        {
            return Path.GetFullPath(_PACKAGES_FOLDER);
        }

        public static void SaveMarker(LocalPackage package, LocalPackageMarker marker, bool refresh = true)
        {
            File.WriteAllText(GetMarkerPath(package), JsonUtility.ToJson(marker, true));
            if (refresh) AssetDatabase.Refresh();
        }

        public static void RemoveMarker(LocalPackage package)
        {
            string markerPath = GetMarkerPath(package);
            DeleteIfExists(markerPath);
            DeleteIfExists(markerPath + ".meta");
            AssetDatabase.Refresh();
        }

        public static string GetFullPath(LocalPackage package)
        {
            return Path.GetFullPath(package.FolderPath);
        }

        /// <summary>
        /// Writes the version into the package's package.json. False when it has no package.json or version field.
        /// </summary>
        public static bool SetManifestVersion(LocalPackage package, string version)
        {
            string manifestPath = Path.Combine(GetFullPath(package), _MANIFEST_FILE);
            if (!File.Exists(manifestPath)) return false;

            string json = File.ReadAllText(manifestPath);
            if (!_ManifestVersion.IsMatch(json)) return false;

            File.WriteAllText(manifestPath, _ManifestVersion.Replace(json, "${1}" + version + "${2}", 1));
            AssetDatabase.Refresh();
            return true;
        }

        /// <summary>Folders of the package's assembly definitions: a csc.rsp file there applies to that assembly.</summary>
        public static List<string> FindAssemblyFolders(LocalPackage package)
        {
            string root = GetFullPath(package);
            if (!Directory.Exists(root)) return new List<string>();

            return Directory.GetFiles(root, "*.asmdef", SearchOption.AllDirectories)
                            .Select(file => Path.GetDirectoryName(file))
                            .OfType<string>()
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToList();
        }

        /// <summary>How many of the package's assemblies have their warnings silenced.</summary>
        public static int CountSilencedAssemblies(LocalPackage package, out int assemblyCount)
        {
            List<string> folders = FindAssemblyFolders(package);
            assemblyCount = folders.Count;
            return folders.Count(folder => HasSilenceOption(Path.Combine(folder, _RESPONSE_FILE)));
        }

        /// <summary>
        /// Adds or removes the -warn:0 option in a csc.rsp next to every assembly definition of the package.
        /// Other options already in those files are kept; a file left empty is deleted.
        /// </summary>
        public static void SetWarningsSilenced(LocalPackage package, bool silenced, bool refresh = true)
        {
            bool changed = false;

            foreach (string folder in FindAssemblyFolders(package))
            {
                string       responseFile = Path.Combine(folder, _RESPONSE_FILE);
                List<string> lines        = File.Exists(responseFile) ? File.ReadAllLines(responseFile).ToList() : new List<string>();
                if (lines.Any(IsSilenceOption) == silenced) continue;

                changed = true;
                if (silenced)
                {
                    lines.Add(_SILENCE_OPTION);
                    File.WriteAllLines(responseFile, lines);
                    continue;
                }

                lines.RemoveAll(IsSilenceOption);
                if (lines.All(string.IsNullOrWhiteSpace))
                {
                    DeleteIfExists(responseFile);
                    DeleteIfExists(responseFile + ".meta");
                }
                else
                {
                    File.WriteAllLines(responseFile, lines);
                }
            }

            if (!changed || !refresh) return;
            AssetDatabase.Refresh();
            CompilationPipeline.RequestScriptCompilation();
        }

        #endregion Public Methods

        #region Private Methods

        /// <param name="marked">True for marked packages only, false for unmarked ones only, null for all.</param>
        private static List<LocalPackage> FindPackages(bool? marked)
        {
            List<LocalPackage> packages = new List<LocalPackage>();
            string             root     = Path.GetFullPath(_PACKAGES_FOLDER);
            if (!Directory.Exists(root)) return packages;

            foreach (string directory in Directory.GetDirectories(root))
            {
                LocalPackage? package = LoadPackageFromDirectory(directory);
                if (package != null && (marked == null || package.IsMarked == marked)) packages.Add(package);
            }

            packages.Sort((a, b) => string.Compare(a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase));
            return packages;
        }

        private static LocalPackage? LoadPackageFromDirectory(string directory)
        {
            string manifestPath = Path.Combine(directory, _MANIFEST_FILE);
            string markerPath   = Path.Combine(directory, LocalPackageMarker.FILE_NAME);
            bool   hasManifest  = File.Exists(manifestPath);
            bool   hasMarker    = File.Exists(markerPath);
            if (!hasManifest && !hasMarker) return null;

            PackageManifest?    manifest = hasManifest ? ReadJson<PackageManifest>(manifestPath) : null;
            LocalPackageMarker? marker   = hasMarker ? ReadJson<LocalPackageMarker>(markerPath) ?? new LocalPackageMarker() : null;
            string              folder   = Path.GetFileName(directory) ?? directory;
            string              name     = manifest?.name is { Length: > 0 } manifestName ? manifestName : folder;
            string              display  = manifest?.displayName is { Length: > 0 } manifestDisplay ? manifestDisplay : name;

            return new LocalPackage($"{_PACKAGES_FOLDER}/{folder}", folder, name, display, manifest?.version, marker);
        }

        private static T? ReadJson<T>(string path) where T : class
        {
            try
            {
                return JsonUtility.FromJson<T>(File.ReadAllText(path));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Local Packages: can't read {path}: {e.Message}");
                return null;
            }
        }

        private static string GetMarkerPath(LocalPackage package)
        {
            return Path.Combine(GetFullPath(package), LocalPackageMarker.FILE_NAME);
        }

        private static void DeleteIfExists(string path)
        {
            if (File.Exists(path)) File.Delete(path);
        }

        private static bool HasSilenceOption(string responseFile)
        {
            return File.Exists(responseFile) && File.ReadAllLines(responseFile).Any(IsSilenceOption);
        }

        private static bool IsSilenceOption(string line)
        {
            return line.Trim() == _SILENCE_OPTION;
        }

        #endregion Private Methods
    }
}
