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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DeusaldUnityTools
{
    /// <summary>
    /// Tools > Local Packages window: lists local packages, checks them for updates and edits their markers.
    /// </summary>
    public class LocalPackagesWindow : EditorWindow
    {
        #region Types

        private enum Status
        {
            NotChecked,
            Checking,
            UpToDate,
            UpdateAvailable,
            Error
        }

        #endregion Types

        #region Variables

        private const string _UXML_PATH          = "Packages/com.deusald.deusaldunitytools.localpackages/Editor/LocalPackagesWindow.uxml";
        private const string _HIDDEN_CLASS       = "lp-hidden";
        private const string _UNSUPPORTED_SOURCE = "Unsupported source. Use an Asset Store page or an OpenUPM package URL.";

        private static readonly TimeSpan _AutoCheckInterval = TimeSpan.FromHours(24);
        private static readonly string[] _StatusClasses     = { "lp-status-ok", "lp-status-update", "lp-status-error", "lp-status-checking" };

        [SerializeField] private string? _SelectedFolder;

        private readonly List<LocalPackage> _Packages         = new List<LocalPackage>();
        private readonly List<LocalPackage> _UnmarkedPackages = new List<LocalPackage>();
        private readonly HashSet<string>    _Checking         = new HashSet<string>();

        private LocalPackagesCache _Cache = new LocalPackagesCache();
        private bool               _AddMode;
        private bool               _Installing;

        private ListView      _PackageList              = null!;
        private Label         _SummaryLabel             = null!;
        private Label         _LastCheckedLabel         = null!;
        private Label         _EmptyLabel               = null!;
        private VisualElement _DetailsPanel             = null!;
        private Label         _DisplayNameLabel         = null!;
        private Label         _PackageNameLabel         = null!;
        private Label         _SourceTypeLabel          = null!;
        private Label         _StatusLabel              = null!;
        private Label         _FolderValue              = null!;
        private Label         _InstalledValue           = null!;
        private Label         _LatestValue              = null!;
        private Label         _PublishedValue           = null!;
        private Label         _CheckedValue             = null!;
        private Label         _CheckErrorLabel          = null!;
        private Button        _CheckButton              = null!;
        private Button        _OpenSourceButton         = null!;
        private TextField     _SourceField              = null!;
        private TextField     _VersionField             = null!;
        private Label         _MarkerErrorLabel         = null!;
        private Button        _InstallLatestButton      = null!;
        private Label         _InstallLatestStatusLabel = null!;
        private Label         _InstallLatestErrorLabel  = null!;
        private Button        _MarkUpdatedButton        = null!;
        private Toggle        _SilenceToggle            = null!;
        private Label         _SilenceStatusLabel       = null!;
        private Label         _SilenceErrorLabel        = null!;
        private VisualElement _ReapplySilenceRow        = null!;
        private VisualElement _AddPanel                 = null!;
        private TextField     _InstallNameField         = null!;
        private TextField     _InstallVersionField      = null!;
        private Toggle        _InstallSilenceToggle     = null!;
        private Label         _InstallErrorLabel        = null!;
        private Label         _InstallStatusLabel       = null!;
        private Button        _InstallButton            = null!;
        private DropdownField _AddPackageField          = null!;
        private TextField     _AddSourceField           = null!;
        private TextField     _AddVersionField          = null!;
        private Toggle        _AddSilenceToggle         = null!;
        private Label         _AddErrorLabel            = null!;
        private Button        _AddConfirmButton         = null!;

        #endregion Variables

        #region Properties

        private LocalPackage? _Selected => _Packages.Find(package => package.FolderPath == _SelectedFolder);

        #endregion Properties

        #region Init Methods

        [MenuItem("Tools/Local Packages")]
        public static void ShowWindow()
        {
            LocalPackagesWindow window = GetWindow<LocalPackagesWindow>();
            window.titleContent = new GUIContent("Local Packages");
            window.minSize      = new Vector2(600, 320);
        }

        public void CreateGUI()
        {
            VisualTreeAsset tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(_UXML_PATH);
            if (tree == null)
            {
                rootVisualElement.Add(new Label($"Can't load {_UXML_PATH}"));
                return;
            }

            tree.CloneTree(rootVisualElement);
            QueryElements();
            RegisterCallbacks();

            _PackageList.makeItem      = MakeItem;
            _PackageList.bindItem      = BindItem;
            _PackageList.itemsSource   = _Packages;
            _PackageList.selectionType = SelectionType.Single;

            SetStatus(_InstallStatusLabel,       null);
            SetStatus(_InstallLatestStatusLabel, null);
            SetError(_InstallErrorLabel,         null);
            SetError(_InstallLatestErrorLabel,   null);

            _Cache = LocalPackagesCache.Load();
            ReloadPackages(true);
            CheckPackages(_Packages.Where(NeedsAutoCheck).ToList());
        }

        private void QueryElements()
        {
            VisualElement root = rootVisualElement;

            _PackageList              = root.Q<ListView>("packageList");
            _SummaryLabel             = root.Q<Label>("summaryLabel");
            _LastCheckedLabel         = root.Q<Label>("lastCheckedLabel");
            _EmptyLabel               = root.Q<Label>("emptyLabel");
            _DetailsPanel             = root.Q<VisualElement>("detailsPanel");
            _DisplayNameLabel         = root.Q<Label>("displayNameLabel");
            _PackageNameLabel         = root.Q<Label>("packageNameLabel");
            _SourceTypeLabel          = root.Q<Label>("sourceTypeLabel");
            _StatusLabel              = root.Q<Label>("statusLabel");
            _FolderValue              = root.Q<Label>("folderValue");
            _InstalledValue           = root.Q<Label>("installedValue");
            _LatestValue              = root.Q<Label>("latestValue");
            _PublishedValue           = root.Q<Label>("publishedValue");
            _CheckedValue             = root.Q<Label>("checkedValue");
            _CheckErrorLabel          = root.Q<Label>("checkErrorLabel");
            _CheckButton              = root.Q<Button>("checkButton");
            _OpenSourceButton         = root.Q<Button>("openSourceButton");
            _SourceField              = root.Q<TextField>("sourceField");
            _VersionField             = root.Q<TextField>("versionField");
            _MarkerErrorLabel         = root.Q<Label>("markerErrorLabel");
            _InstallLatestButton      = root.Q<Button>("installLatestButton");
            _InstallLatestStatusLabel = root.Q<Label>("installLatestStatusLabel");
            _InstallLatestErrorLabel  = root.Q<Label>("installLatestErrorLabel");
            _MarkUpdatedButton        = root.Q<Button>("markUpdatedButton");
            _SilenceToggle            = root.Q<Toggle>("silenceToggle");
            _SilenceStatusLabel       = root.Q<Label>("silenceStatusLabel");
            _SilenceErrorLabel        = root.Q<Label>("silenceErrorLabel");
            _ReapplySilenceRow        = root.Q<VisualElement>("reapplySilenceRow");
            _AddPanel                 = root.Q<VisualElement>("addPanel");
            _InstallNameField         = root.Q<TextField>("installNameField");
            _InstallVersionField      = root.Q<TextField>("installVersionField");
            _InstallSilenceToggle     = root.Q<Toggle>("installSilenceToggle");
            _InstallErrorLabel        = root.Q<Label>("installErrorLabel");
            _InstallStatusLabel       = root.Q<Label>("installStatusLabel");
            _InstallButton            = root.Q<Button>("installButton");
            _AddPackageField          = root.Q<DropdownField>("addPackageField");
            _AddSourceField           = root.Q<TextField>("addSourceField");
            _AddVersionField          = root.Q<TextField>("addVersionField");
            _AddSilenceToggle         = root.Q<Toggle>("addSilenceToggle");
            _AddErrorLabel            = root.Q<Label>("addErrorLabel");
            _AddConfirmButton         = root.Q<Button>("addConfirmButton");
        }

        private void RegisterCallbacks()
        {
            VisualElement root = rootVisualElement;

            root.Q<Button>("refreshButton").clicked        += () => ReloadPackages(true);
            root.Q<Button>("checkAllButton").clicked       += () => CheckPackages(_Packages.ToList());
            root.Q<Button>("addButton").clicked            += ShowAddPanel;
            root.Q<Button>("showFolderButton").clicked     += ShowSelectedFolder;
            root.Q<Button>("saveButton").clicked           += SaveSelected;
            root.Q<Button>("removeButton").clicked         += RemoveSelected;
            root.Q<Button>("addCancelButton").clicked      += HideAddPanel;
            root.Q<Button>("reapplySilenceButton").clicked += ReapplySelectedSilence;
            _CheckButton.clicked                           += () => CheckPackages(new[] { _Selected });
            _OpenSourceButton.clicked                      += OpenSelectedSource;
            _MarkUpdatedButton.clicked                     += MarkSelectedAsUpdated;
            _InstallButton.clicked                         += InstallFromAddPanel;
            _InstallLatestButton.clicked                   += InstallLatestSelected;
            _AddConfirmButton.clicked                      += AddPackage;

            _PackageList.selectionChanged += OnSelectionChanged;
            _AddPackageField.RegisterValueChangedCallback(_ => UpdateAddVersionPlaceholder());
            _SilenceToggle.RegisterValueChangedCallback(evt => SetSelectedSilenced(evt.newValue));
        }

        #endregion Init Methods

        #region Private Methods

        private void ReloadPackages(bool resetFields)
        {
            _Packages.Clear();
            _Packages.AddRange(LocalPackagesRegistry.FindLocalPackages());
            _PackageList.RefreshItems();

            int index = _Packages.FindIndex(package => package.FolderPath == _SelectedFolder);
            if (index < 0) _SelectedFolder = null;
            _PackageList.SetSelectionWithoutNotify(index >= 0 ? new[] { index } : Array.Empty<int>());

            ShowCurrentPanel(resetFields);
        }

        private void OnSelectionChanged(IEnumerable<object> selection)
        {
            LocalPackage? selected = selection.OfType<LocalPackage>().FirstOrDefault();
            if (selected == null && _AddMode) return;

            _AddMode        = false;
            _SelectedFolder = selected?.FolderPath;
            ShowCurrentPanel(true);
        }

        private void ShowCurrentPanel(bool resetFields)
        {
            LocalPackage? selected = _Selected;

            SetVisible(_AddPanel,     _AddMode);
            SetVisible(_DetailsPanel, !_AddMode && selected != null);
            SetVisible(_EmptyLabel,   !_AddMode && selected == null);

            _EmptyLabel.text = _Packages.Count == 0
                                   ? "No local packages yet.\n\nPut an Asset Store or OpenUPM package into the Packages folder, then click + to mark it as a local package."
                                   : "Select a package.";

            if (resetFields && selected != null)
            {
                _SourceField.SetValueWithoutNotify(selected.Source);
                _VersionField.SetValueWithoutNotify(selected.Marker?.version?.Trim() ?? string.Empty);
                _VersionField.textEdition.placeholder = selected.ManifestVersion ?? string.Empty;
                SetError(_MarkerErrorLabel,        null);
                SetError(_InstallLatestErrorLabel, null);
            }

            RefreshViews();
        }

        private void RefreshViews()
        {
            _PackageList.RefreshItems();

            int  updates     = _Packages.Count(package => GetStatus(package) == Status.UpdateAvailable);
            long lastChecked = _Packages.Select(package => GetInfo(package)?.checkedAtTicks ?? 0).DefaultIfEmpty(0).Max();

            _SummaryLabel.text     = $"{Plural(_Packages.Count, "package")}" + (updates > 0 ? $" · {Plural(updates, "update")}" : string.Empty);
            _LastCheckedLabel.text = lastChecked > 0 ? $"Last checked {FormatLocal(new DateTime(lastChecked, DateTimeKind.Utc))}" : string.Empty;

            LocalPackage? selected = _Selected;
            if (selected != null) UpdateDetails(selected);
        }

        private void UpdateDetails(LocalPackage package)
        {
            LocalPackageUpdateInfo? info   = GetInfo(package);
            Status                  status = GetStatus(package);
            bool                    known  = package.SourceType != LocalPackageSourceType.Unknown;

            _DisplayNameLabel.text = package.DisplayName;
            _PackageNameLabel.text = package.PackageName;
            _SourceTypeLabel.text  = GetSourceTypeName(package.SourceType);
            _FolderValue.text      = package.FolderPath;
            _InstalledValue.text   = OrDash(package.InstalledVersion);
            _LatestValue.text      = OrDash(info?.latestVersion);
            _PublishedValue.text   = FormatDate(info?.publishedDate);
            _CheckedValue.text     = info != null ? FormatLocal(info.CheckedAtUtc) : "Never";
            ApplyStatus(_StatusLabel, status, false);

            string? error = status != Status.Error ? null : known ? info?.error : _UNSUPPORTED_SOURCE;
            SetError(_CheckErrorLabel, error);

            _CheckButton.SetEnabled(known && status != Status.Checking);
            _OpenSourceButton.SetEnabled(known);
            _MarkUpdatedButton.SetEnabled(status == Status.UpdateAvailable);
            _InstallLatestButton.SetEnabled(!_Installing && status == Status.UpdateAvailable);
            SetVisible(_InstallLatestButton, package.SourceType == LocalPackageSourceType.OpenUpm);

            UpdateSilenceDetails(package);
        }

        private void UpdateSilenceDetails(LocalPackage package)
        {
            bool silenced      = package.Marker?.silenceWarnings ?? false;
            int  silencedCount = LocalPackagesRegistry.CountSilencedAssemblies(package, out int assemblyCount);

            _SilenceToggle.SetValueWithoutNotify(silenced);
            _SilenceToggle.SetEnabled(assemblyCount > 0 || silenced);

            _SilenceStatusLabel.text = assemblyCount == 0 ? "The package has no assembly definitions, so there is nothing to silence."
                                     : silenced           ? $"Silenced with csc.rsp files in {Assemblies(assemblyCount)}."
                                                          : $"Warnings are shown for {Assemblies(assemblyCount)}.";

            string? error = silenced && silencedCount < assemblyCount
                                ? $"{assemblyCount - silencedCount} of {Assemblies(assemblyCount)} have no csc.rsp with -warn:0, e.g. after replacing the package files."
                                : null;
            SetError(_SilenceErrorLabel, error);
            SetVisible(_ReapplySilenceRow, error != null);
        }

        private VisualElement MakeItem()
        {
            VisualElement item    = new VisualElement();
            VisualElement texts   = new VisualElement();
            Label         name    = new Label { name = "itemName" };
            Label         version = new Label { name = "itemVersion" };
            Label         status  = new Label { name = "itemStatus" };

            item.AddToClassList("lp-item");
            texts.AddToClassList("lp-item-texts");
            name.AddToClassList("lp-item-name");
            version.AddToClassList("lp-item-version");
            status.AddToClassList("lp-badge");

            texts.Add(name);
            texts.Add(version);
            item.Add(texts);
            item.Add(status);
            return item;
        }

        private void BindItem(VisualElement item, int index)
        {
            if (index < 0 || index >= _Packages.Count) return;

            LocalPackage            package = _Packages[index];
            LocalPackageUpdateInfo? info    = GetInfo(package);
            Status                  status  = GetStatus(package);
            string                  version = OrDash(package.InstalledVersion);

            if (status == Status.UpdateAvailable) version += $" → {info?.latestVersion}";

            item.Q<Label>("itemName").text    = package.DisplayName;
            item.Q<Label>("itemVersion").text = $"{GetSourceTypeName(package.SourceType)} · {version}";
            ApplyStatus(item.Q<Label>("itemStatus"), status, true);
        }

        private async void CheckPackages(IEnumerable<LocalPackage?> packages)
        {
            List<Task> tasks = new List<Task>();
            foreach (LocalPackage? package in packages)
            {
                if (package == null || package.SourceType == LocalPackageSourceType.Unknown) continue;
                if (!_Checking.Add(package.FolderPath)) continue;
                tasks.Add(CheckPackageAsync(package));
            }

            if (tasks.Count == 0) return;
            RefreshViews();

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private async Task CheckPackageAsync(LocalPackage package)
        {
            LocalPackageUpdateInfo info = await LocalPackageUpdateChecker.CheckAsync(package);

            _Checking.Remove(package.FolderPath);
            _Cache.Set(info);
            _Cache.Save();

            // The window may have been closed while the request was running.
            if (this == null || _PackageList == null) return;
            RefreshViews();
        }

        private bool NeedsAutoCheck(LocalPackage package)
        {
            if (package.SourceType == LocalPackageSourceType.Unknown) return false;
            LocalPackageUpdateInfo? info = GetInfo(package);
            return info == null || DateTime.UtcNow - info.CheckedAtUtc > _AutoCheckInterval;
        }

        private LocalPackageUpdateInfo? GetInfo(LocalPackage package)
        {
            return _Cache.Get(package.Source);
        }

        private Status GetStatus(LocalPackage package)
        {
            if (_Checking.Contains(package.FolderPath)) return Status.Checking;
            if (package.SourceType == LocalPackageSourceType.Unknown) return Status.Error;

            LocalPackageUpdateInfo? info = GetInfo(package);
            if (info == null) return Status.NotChecked;
            if (info.HasError) return Status.Error;

            return VersionComparer.Compare(info.latestVersion, package.InstalledVersion) > 0
                       ? Status.UpdateAvailable
                       : Status.UpToDate;
        }

        private void SaveSelected()
        {
            LocalPackage? package = _Selected;
            if (package == null) return;

            LocalPackageMarker marker = CreateMarker(_SourceField.value, _VersionField.value, package.Marker?.silenceWarnings ?? false);
            if (!ValidateMarker(marker, package, _MarkerErrorLabel)) return;

            bool sourceChanged = marker.source != package.Source;
            LocalPackagesRegistry.SaveMarker(package, marker);
            ReloadPackages(true);
            if (sourceChanged) CheckPackages(new[] { _Selected });
        }

        private void MarkSelectedAsUpdated()
        {
            LocalPackage?       package = _Selected;
            LocalPackageMarker? marker  = package?.Marker;
            if (package == null || marker == null) return;

            LocalPackageUpdateInfo? info = GetInfo(package);
            if (info == null || info.latestVersion.Length == 0) return;

            // Without a version in the marker the package.json version is the installed one, and it has to stay
            // a 3 part version, so Asset Store's 3.985 is written as 3.985.0.
            bool usesManifest = string.IsNullOrWhiteSpace(marker.version);
            if (!usesManifest
             || !VersionComparer.TryNormalizeToSemVer(info.latestVersion, out string? semVer)
             || !LocalPackagesRegistry.SetManifestVersion(package, semVer))
            {
                LocalPackagesRegistry.SaveMarker(package, CreateMarker(marker.source, info.latestVersion, marker.silenceWarnings));
            }

            if (marker.silenceWarnings) LocalPackagesRegistry.SetWarningsSilenced(package, true);
            ReloadPackages(true);
        }

        private void SetSelectedSilenced(bool silenced)
        {
            LocalPackage?       package = _Selected;
            LocalPackageMarker? marker  = package?.Marker;
            if (package == null || marker == null) return;

            LocalPackagesRegistry.SaveMarker(package, CreateMarker(marker.source, marker.version, silenced));
            LocalPackagesRegistry.SetWarningsSilenced(package, silenced);
            ReloadPackages(false);
        }

        private void ReapplySelectedSilence()
        {
            LocalPackage? package = _Selected;
            if (package == null) return;

            LocalPackagesRegistry.SetWarningsSilenced(package, true);
            RefreshViews();
        }

        private void RemoveSelected()
        {
            LocalPackage? package = _Selected;
            if (package == null) return;

            bool confirmed = EditorUtility.DisplayDialog("Remove Marker",
                                                         $"Delete the {LocalPackageMarker.FILE_NAME} marker of {package.DisplayName}?\n\nThe package itself stays in the Packages folder; it is just no longer tracked and its warnings are no longer silenced.",
                                                         "Remove", "Cancel");
            if (!confirmed) return;

            if (package.Marker?.silenceWarnings == true) LocalPackagesRegistry.SetWarningsSilenced(package, false);
            LocalPackagesRegistry.RemoveMarker(package);
            _SelectedFolder = null;
            ReloadPackages(true);
        }

        private void OpenSelectedSource()
        {
            LocalPackage? package = _Selected;
            if (package != null) Application.OpenURL(package.Source);
        }

        private void ShowSelectedFolder()
        {
            LocalPackage? package = _Selected;
            if (package != null) EditorUtility.RevealInFinder(Path.Combine(LocalPackagesRegistry.GetFullPath(package), LocalPackageMarker.FILE_NAME));
        }

        private void ShowAddPanel()
        {
            _AddMode        = true;
            _SelectedFolder = null;
            _PackageList.SetSelectionWithoutNotify(Array.Empty<int>());

            _UnmarkedPackages.Clear();
            _UnmarkedPackages.AddRange(LocalPackagesRegistry.FindUnmarkedPackages());

            bool hasPackages = _UnmarkedPackages.Count > 0;
            _AddPackageField.choices = _UnmarkedPackages.Select(package => $"{package.DisplayName} ({package.FolderName})").ToList();
            _AddPackageField.index   = hasPackages ? 0 : -1;
            _AddPackageField.SetEnabled(hasPackages);
            _AddConfirmButton.SetEnabled(hasPackages);
            _AddSourceField.SetValueWithoutNotify(string.Empty);
            _AddVersionField.SetValueWithoutNotify(string.Empty);
            _AddSilenceToggle.SetValueWithoutNotify(false);
            _InstallNameField.SetValueWithoutNotify(string.Empty);
            _InstallVersionField.SetValueWithoutNotify(string.Empty);
            _InstallSilenceToggle.SetValueWithoutNotify(false);
            _InstallButton.SetEnabled(!_Installing);
            SetError(_InstallErrorLabel, null);
            UpdateAddVersionPlaceholder();
            SetError(_AddErrorLabel, hasPackages ? null : "Every package in the Packages folder is already marked, or none has a package.json.");

            ShowCurrentPanel(false);
        }

        private void HideAddPanel()
        {
            _AddMode = false;
            ShowCurrentPanel(false);
        }

        private void UpdateAddVersionPlaceholder()
        {
            int index = _AddPackageField.index;
            _AddVersionField.textEdition.placeholder = index >= 0 && index < _UnmarkedPackages.Count
                                                           ? _UnmarkedPackages[index].ManifestVersion ?? string.Empty
                                                           : string.Empty;
        }

        private void AddPackage()
        {
            int index = _AddPackageField.index;
            if (index < 0 || index >= _UnmarkedPackages.Count) return;

            LocalPackage       package = _UnmarkedPackages[index];
            LocalPackageMarker marker  = CreateMarker(_AddSourceField.value, _AddVersionField.value, _AddSilenceToggle.value);
            if (!ValidateMarker(marker, package, _AddErrorLabel)) return;

            LocalPackagesRegistry.SaveMarker(package, marker);
            if (marker.silenceWarnings) LocalPackagesRegistry.SetWarningsSilenced(package, true);
            _AddMode        = false;
            _SelectedFolder = package.FolderPath;
            ReloadPackages(true);
            CheckPackages(new[] { _Selected });
        }

        private void InstallFromAddPanel()
        {
            string input       = _InstallNameField.value?.Trim() ?? string.Empty;
            string packageName = LocalPackageSources.TryGetOpenUpmPackageName(input, out string? parsedName) ? parsedName : input;

            if (!LocalPackageInstaller.IsValidPackageName(packageName))
            {
                SetError(_InstallErrorLabel, "Enter a package name like com.vendor.package, or its OpenUPM page URL.");
                return;
            }

            LocalPackage? existing = LocalPackagesRegistry.FindPackageByName(packageName);
            if (existing != null)
            {
                bool replace = EditorUtility.DisplayDialog("Replace Package",
                                                           $"{packageName} is already in {existing.FolderPath}. Replace its files with the version from OpenUPM?\n\nChanges you made inside the package folder are lost.",
                                                           "Replace", "Cancel");
                if (!replace) return;
            }

            RunInstall(packageName, _InstallVersionField.value, _InstallSilenceToggle.value, _InstallStatusLabel, _InstallErrorLabel);
        }

        private void InstallLatestSelected()
        {
            LocalPackage? package = _Selected;
            if (package == null) return;

            LocalPackageUpdateInfo? info = GetInfo(package);
            if (info == null || info.latestVersion.Length == 0) return;

            bool confirmed = EditorUtility.DisplayDialog("Install Latest",
                                                         $"Replace the files of {package.DisplayName} in {package.FolderPath} with version {info.latestVersion} from OpenUPM?\n\nChanges you made inside the package folder are lost. The marker settings are kept.",
                                                         "Install", "Cancel");
            if (!confirmed) return;

            RunInstall(LocalPackageUpdateChecker.GetOpenUpmPackageName(package), info.latestVersion, package.Marker?.silenceWarnings ?? false,
                       _InstallLatestStatusLabel, _InstallLatestErrorLabel);
        }

        private async void RunInstall(string packageName, string? version, bool silenceWarnings, Label statusLabel, Label errorLabel)
        {
            if (_Installing) return;

            _Installing = true;
            SetError(errorLabel, null);
            SetStatus(statusLabel, $"Installing {packageName}...");
            RefreshInstallButtons();

            try
            {
                LocalPackageInstaller.InstallResult result = await LocalPackageInstaller.InstallFromOpenUpmAsync(packageName, version, silenceWarnings, message =>
                {
                    if (this != null) SetStatus(statusLabel, message);
                });

                Debug.Log($"Local Packages: installed {string.Join(", ", result.Installed)}.");
                foreach (string note in result.Notes) Debug.LogWarning($"Local Packages: {note}");
                if (this == null) return;

                _AddMode        = false;
                _SelectedFolder = LocalPackagesRegistry.FindPackageByName(packageName)?.FolderPath;
                ReloadPackages(true);
            }
            catch (Exception e)
            {
                Debug.LogError($"Local Packages: installing {packageName} failed: {e.Message}");
                if (this != null) SetError(errorLabel, e.Message);
            }
            finally
            {
                _Installing = false;
                if (this != null)
                {
                    SetStatus(statusLabel, null);
                    RefreshInstallButtons();
                }
            }
        }

        private void RefreshInstallButtons()
        {
            _InstallButton.SetEnabled(!_Installing);
            RefreshViews();
        }

        private static LocalPackageMarker CreateMarker(string? source, string? version, bool silenceWarnings)
        {
            return new LocalPackageMarker
            {
                source          = source?.Trim() ?? string.Empty,
                version         = version?.Trim() ?? string.Empty,
                silenceWarnings = silenceWarnings
            };
        }

        private static bool ValidateMarker(LocalPackageMarker marker, LocalPackage package, Label errorLabel)
        {
            LocalPackageSourceType type  = LocalPackageSources.GetSourceType(marker.source);
            string?                error = null;

            if (type == LocalPackageSourceType.Unknown)
                error = "Source must be an Asset Store page or an OpenUPM package URL.";
            else if (type == LocalPackageSourceType.AssetStore && !LocalPackageSources.TryGetAssetStoreId(marker.source, out _))
                error = "The Asset Store URL must end with the product id, e.g. .../editor-console-pro-11889.";
            else if (string.IsNullOrEmpty(marker.version) && string.IsNullOrEmpty(package.ManifestVersion))
                error = "Set the installed version: the package.json has none.";

            SetError(errorLabel, error);
            return error == null;
        }

        private static void ApplyStatus(Label label, Status status, bool compact)
        {
            label.text = status switch
            {
                Status.Checking        => "Checking...",
                Status.UpToDate        => "Up to date",
                Status.UpdateAvailable => compact ? "Update" : "Update available",
                Status.Error           => "Error",
                _                      => "Not checked"
            };

            string? statusClass = status switch
            {
                Status.Checking        => "lp-status-checking",
                Status.UpToDate        => "lp-status-ok",
                Status.UpdateAvailable => "lp-status-update",
                Status.Error           => "lp-status-error",
                _                      => null
            };

            foreach (string candidate in _StatusClasses) label.EnableInClassList(candidate, candidate == statusClass);
        }

        private static string GetSourceTypeName(LocalPackageSourceType type)
        {
            return type switch
            {
                LocalPackageSourceType.AssetStore => "Asset Store",
                LocalPackageSourceType.OpenUpm    => "OpenUPM",
                _                                 => "Unknown source"
            };
        }

        private static void SetVisible(VisualElement element, bool visible)
        {
            element.EnableInClassList(_HIDDEN_CLASS, !visible);
        }

        private static void SetStatus(Label label, string? status)
        {
            label.text = status ?? string.Empty;
            SetVisible(label, !string.IsNullOrEmpty(status));
        }

        private static void SetError(Label label, string? error)
        {
            label.text = error ?? string.Empty;
            SetVisible(label, !string.IsNullOrEmpty(error));
        }

        private static string FormatDate(string? isoDate)
        {
            if (isoDate is not { Length: > 0 }) return "-";
            return DateTime.TryParse(isoDate, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime date)
                       ? date.ToLocalTime().ToString("yyyy-MM-dd")
                       : isoDate;
        }

        private static string FormatLocal(DateTime utc)
        {
            return utc.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
        }

        private static string OrDash(string? value)
        {
            return value is { Length: > 0 } ? value : "-";
        }

        private static string Plural(int count, string noun)
        {
            return count == 1 ? $"1 {noun}" : $"{count} {noun}s";
        }

        private static string Assemblies(int count)
        {
            return count == 1 ? "1 assembly" : $"{count} assemblies";
        }

        #endregion Private Methods
    }
}
