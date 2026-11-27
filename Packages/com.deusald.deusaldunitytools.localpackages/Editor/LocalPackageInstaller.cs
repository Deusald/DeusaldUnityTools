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
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;

namespace DeusaldUnityTools
{
    /// <summary>
    /// Installs OpenUPM packages into the Packages folder as local (embedded) packages.
    /// </summary>
    public static class LocalPackageInstaller
    {
        #region Types

        public class InstallResult
        {
            /// <summary>Installed packages as "name version".</summary>
            public readonly List<string> Installed = new List<string>();

            /// <summary>Things worth a warning: dependencies left out, manifest entries that are now overridden.</summary>
            public readonly List<string> Notes = new List<string>();
        }

        private class StagedPackage
        {
            public string                              Name         { get; }
            public string                              Version      { get; }
            public string                              Folder       { get; }
            public List<(string name, string version)> Dependencies { get; }

            public StagedPackage(string name, string version, string folder, List<(string name, string version)> dependencies)
            {
                Name         = name;
                Version      = version;
                Folder       = folder;
                Dependencies = dependencies;
            }
        }

        #endregion Types

        #region Variables

        private const string _STAGING_FOLDER = "Temp/DeusaldLocalPackages";
        private const string _UNITY_PREFIX   = "com.unity.";
        private const string _OPEN_UPM_PAGE  = "https://openupm.com/packages/";

        private static readonly Regex _Tarball         = new Regex("\"tarball\"\\s*:\\s*\"([^\"]+)\"",       RegexOptions.Compiled);
        private static readonly Regex _Shasum          = new Regex("\"shasum\"\\s*:\\s*\"([0-9a-fA-F]+)\"",  RegexOptions.Compiled);
        private static readonly Regex _Dependencies    = new Regex("\"dependencies\"\\s*:\\s*\\{([^}]*)\\}", RegexOptions.Compiled);
        private static readonly Regex _DependencyEntry = new Regex("\"([^\"]+)\"\\s*:\\s*\"([^\"]*)\"",      RegexOptions.Compiled);
        private static readonly Regex _PackageName     = new Regex("^[a-z0-9][a-z0-9._-]*$",                 RegexOptions.Compiled);

        #endregion Variables

        #region Public Methods

        public static bool IsValidPackageName(string packageName)
        {
            return _PackageName.IsMatch(packageName);
        }

        /// <summary>
        /// Downloads the package from OpenUPM into Packages/&lt;name&gt; and marks it as a local package, together with
        /// its dependencies that come from OpenUPM and are not in the Packages folder yet. An existing copy of the
        /// package is replaced; its marker settings are kept.
        /// </summary>
        /// <param name="version">Version to install; null or empty for the latest one.</param>
        /// <param name="silenceWarnings">Silence compiler warnings of newly added packages.</param>
        /// <param name="progress">Receives short progress messages.</param>
        public static async Task<InstallResult> InstallFromOpenUpmAsync(string packageName, string? version, bool silenceWarnings,
                                                                        Action<string>? progress = null)
        {
            InstallResult                                      result      = new InstallResult();
            List<StagedPackage>                                staged      = new List<StagedPackage>();
            HashSet<string>                                    seen        = new HashSet<string>();
            Queue<(string name, string? version, bool isMain)> queue       = new Queue<(string name, string? version, bool isMain)>();
            string                                             stagingRoot = Path.GetFullPath(_STAGING_FOLDER);
            bool                                               placedAny   = false;

            DeleteDirectory(stagingRoot);
            queue.Enqueue((packageName, version, true));

            try
            {
                // Everything is downloaded first, so a failure leaves the Packages folder untouched.
                while (queue.Count > 0)
                {
                    (string name, string? requestedVersion, bool isMain) = queue.Dequeue();
                    if (!seen.Add(name)) continue;

                    progress?.Invoke($"Downloading {name}...");
                    StagedPackage? package = await StageAsync(name, requestedVersion, isMain, stagingRoot);
                    if (package == null)
                    {
                        result.Notes.Add($"{name} is not on OpenUPM, so it was not installed. If it comes from another registry, add it there.");
                        continue;
                    }

                    staged.Add(package);
                    foreach ((string dependency, string dependencyVersion) in package.Dependencies)
                    {
                        if (dependency.StartsWith(_UNITY_PREFIX, StringComparison.Ordinal) || seen.Contains(dependency)) continue;
                        if (LocalPackagesRegistry.FindPackageByName(dependency) != null) continue;
                        queue.Enqueue((dependency, dependencyVersion, false));
                    }
                }

                progress?.Invoke("Installing...");
                string manifest = ReadProjectManifest();
                foreach (StagedPackage package in staged)
                {
                    Place(package, silenceWarnings, stagingRoot);
                    placedAny = true;
                    result.Installed.Add($"{package.Name} {package.Version}");

                    if (manifest.Contains($"\"{package.Name}\""))
                        result.Notes.Add($"{package.Name} is also listed in Packages/manifest.json. The local copy takes precedence; remove the manifest entry when you no longer need it.");
                }
            }
            finally
            {
                DeleteDirectory(stagingRoot);
                if (placedAny)
                {
                    AssetDatabase.Refresh();
                    Client.Resolve();
                }
            }

            return result;
        }

        #endregion Public Methods

        #region Private Methods

        private static async Task<StagedPackage?> StageAsync(string name, string? requestedVersion, bool isMain, string stagingRoot)
        {
            string  registryUrl  = LocalPackageUpdateChecker.OPEN_UPM_REGISTRY + Uri.EscapeDataString(name) + "/";
            string? version      = requestedVersion?.Trim() is { Length: > 0 } trimmed ? trimmed : null;
            string? manifestJson = null;

            if (version != null) manifestJson = await LocalPackageUpdateChecker.TryGetStringAsync(registryUrl + Uri.EscapeDataString(version), $"OpenUPM package {name}");

            if (manifestJson == null || version == null)
            {
                if (isMain && version != null) throw new Exception($"Version {version} of {name} is not on OpenUPM.");

                // A dependency whose exact version is gone falls back to the latest one.
                string? latest = await LocalPackageUpdateChecker.TryGetOpenUpmLatestVersionAsync(name);
                if (latest == null)
                {
                    if (isMain) throw new Exception($"{name} is not on OpenUPM.");
                    return null;
                }

                version      = latest;
                manifestJson = await LocalPackageUpdateChecker.GetStringAsync(registryUrl + Uri.EscapeDataString(latest), $"OpenUPM package {name}");
            }

            Match tarball = _Tarball.Match(manifestJson);
            if (!tarball.Success) throw new FormatException($"OpenUPM returned no download for {name} {version}.");

            byte[] archive = await LocalPackageUpdateChecker.GetBytesAsync(tarball.Groups[1].Value, $"Download of {name} {version}");

            Match shasum = _Shasum.Match(manifestJson);
            if (shasum.Success && !string.Equals(Sha1(archive), shasum.Groups[1].Value, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"The download of {name} {version} is corrupted (checksum mismatch).");

            string folder = Path.Combine(stagingRoot, name);
            TarArchive.ExtractTarGz(archive, folder, 1);

            string manifestPath = Path.Combine(folder, "package.json");
            if (!File.Exists(manifestPath)) throw new InvalidDataException($"The download of {name} {version} has no package.json.");

            return new StagedPackage(name, version, folder, ReadDependencies(File.ReadAllText(manifestPath)));
        }

        /// <summary>
        /// Moves a staged package into the Packages folder. An existing folder is moved away as a whole first, so a
        /// locked file (e.g. a loaded native plugin) fails the move without leaving a half replaced package.
        /// </summary>
        private static void Place(StagedPackage package, bool silenceWarnings, string stagingRoot)
        {
            LocalPackage?       existing = LocalPackagesRegistry.FindPackageByName(package.Name);
            LocalPackageMarker? previous = existing?.Marker;
            string              target   = existing != null
                                               ? LocalPackagesRegistry.GetFullPath(existing)
                                               : Path.Combine(LocalPackagesRegistry.GetPackagesFolder(), package.Name);

            LocalPackageMarker marker = new LocalPackageMarker
            {
                source          = previous != null && previous.source.Length > 0 && LocalPackageSources.GetSourceType(previous.source) == LocalPackageSourceType.OpenUpm
                                      ? previous.source
                                      : _OPEN_UPM_PAGE + package.Name + "/",
                version         = string.Empty,
                silenceWarnings = previous?.silenceWarnings ?? silenceWarnings
            };

            string? backup = null;
            if (Directory.Exists(target))
            {
                backup = Path.Combine(stagingRoot, "_previous_" + package.Name);
                try
                {
                    Directory.Move(target, backup);
                }
                catch (IOException e)
                {
                    throw new IOException($"Can't replace {package.Name}: a file in its folder is in use ({e.Message}). Close what uses it, or restart Unity, and try again.", e);
                }
            }

            try
            {
                Directory.Move(package.Folder, target);
            }
            catch
            {
                if (backup != null && !Directory.Exists(target)) Directory.Move(backup, target);
                throw;
            }

            if (backup != null) DeleteDirectory(backup);

            LocalPackage? placed = LocalPackagesRegistry.LoadPackage(target);
            if (placed == null) return;

            LocalPackagesRegistry.SaveMarker(placed, marker, false);
            if (marker.silenceWarnings) LocalPackagesRegistry.SetWarningsSilenced(placed, true, false);
        }

        private static List<(string name, string version)> ReadDependencies(string packageJson)
        {
            Match block = _Dependencies.Match(packageJson);
            if (!block.Success) return new List<(string name, string version)>();

            return _DependencyEntry.Matches(block.Groups[1].Value)
                                   .Cast<Match>()
                                   .Select(match => (match.Groups[1].Value, match.Groups[2].Value))
                                   .ToList();
        }

        private static string ReadProjectManifest()
        {
            string path = Path.Combine(LocalPackagesRegistry.GetPackagesFolder(), "manifest.json");
            return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
        }

        private static string Sha1(byte[] data)
        {
            using SHA1 sha1 = SHA1.Create();
            return string.Concat(sha1.ComputeHash(data).Select(value => value.ToString("x2")));
        }

        private static void DeleteDirectory(string path)
        {
            if (!Directory.Exists(path)) return;

            try
            {
                foreach (string file in Directory.GetFiles(path, "*", SearchOption.AllDirectories)) File.SetAttributes(file, FileAttributes.Normal);
                Directory.Delete(path, true);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning($"Local Packages: can't delete {path}: {e.Message}");
            }
        }

        #endregion Private Methods
    }
}
