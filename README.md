# DeusaldUnityTools

DeusaldUnityTools is a set of small, independent Unity modules: a tweening engine, extension methods, an object pool,
encrypted player prefs, native Android/iOS tools (share, store pages, haptics, Game Center auth), inspector attributes
and editor tools, including one that keeps Asset Store and OpenUPM packages locally in the `Packages` folder.

Every module is its own UPM package with its own assembly, so you install only what you need. All modules use the
single `DeusaldUnityTools` namespace.

- [Installation](#installation)
- [Modules](#modules)
- [Tweens](#tweens)
- [Extensions](#extensions)
- [Pooling](#pooling)
- [Secure Player Prefs](#secure-player-prefs)
- [Mobile](#mobile): [Android Tools](#android-tools) · [iOS Tools](#ios-tools) · [Haptic](#haptic) · [Game Center Auth](#game-center-auth)
- [Fps Counter](#fps-counter)
- [Inspector](#inspector)
- [Scene Autoloader](#scene-autoloader)
- [Local Packages](#local-packages): [Installing from OpenUPM](#installing-from-openupm) · [Asset Store packages](#adding-asset-store-packages) · [Updates](#checking-for-updates) · [Silencing warnings](#silencing-compiler-warnings)
- [Upgrading to 4.0](#upgrading-to-40)
- [Native Swift/Java projects](#native-swiftjava-projects)

---

## Installation

The `com.deusald.deusaldunitytools` meta-package has no code of its own and installs every module. Install it to get
everything, or install only the module packages you use.

### OpenUPM (recommended)

Packages from OpenUPM are signed by Deusald, so the Package Manager shows them as signed (Unity 6.3+) and offers updates.

Add the OpenUPM scoped registry for `com.deusald` once, then add the meta-package or just the modules you use to
`Packages/manifest.json`:

```json
"scopedRegistries": [
  {
    "name": "package.openupm.com",
    "url": "https://package.openupm.com",
    "scopes": [ "com.deusald" ]
  }
],
"dependencies": {
  "com.deusald.deusaldunitytools.tweens": "4.1.0",
  "com.deusald.deusaldunitytools.extensions": "4.1.0"
}
```

or with the OpenUPM CLI:

```
openupm add com.deusald.deusaldunitytools            # everything
openupm add com.deusald.deusaldunitytools.tweens     # or just the modules you use
```

### Git URL

In the Package Manager choose **Install package from git URL** and add each module from its own folder:

```
https://github.com/Deusald/DeusaldUnityTools.git?path=/Packages/com.deusald.deusaldunitytools.tweens#vX.X.X
```

Unity does not resolve git dependencies of a package, so the meta-package installs from git only when every module it
depends on is added too, or resolves from the OpenUPM registry. With git URLs, add the modules one by one.

---

## Modules

| Package                                           | Assembly                                    | Contents                                                                  |
|---------------------------------------------------|---------------------------------------------|---------------------------------------------------------------------------|
| `com.deusald.deusaldunitytools`                   | -                                           | Meta-package: every module below                                          |
| `com.deusald.deusaldunitytools.tweens`            | `DeusaldUnityTools.Tweens`                  | [Tweens](#tweens) for Transforms, uGUI, UI Toolkit, lights, audio, sprites |
| `com.deusald.deusaldunitytools.extensions`        | `DeusaldUnityTools.Extensions`              | [String, Unity, uGUI and TMP extensions](#extensions), incl. Text Warp    |
| `com.deusald.deusaldunitytools.pooling`           | `DeusaldUnityTools.Pooling`                 | [GameObject pooling](#pooling)                                            |
| `com.deusald.deusaldunitytools.secureplayerprefs` | `DeusaldUnityTools.SecurePlayerPrefs`       | [Encrypted PlayerPrefs](#secure-player-prefs)                             |
| `com.deusald.deusaldunitytools.mobile`            | `DeusaldUnityTools.Mobile`                  | [Android/iOS tools, Haptic, Game Center Auth](#mobile) + native plugins   |
| `com.deusald.deusaldunitytools.fpscounter`        | `DeusaldUnityTools.FpsCounter`              | [Fps Counter](#fps-counter)                                               |
| `com.deusald.deusaldunitytools.inspector`         | `DeusaldUnityTools.Inspector` (+ `.Editor`) | [`[Button]` and `[ShowInInspector]`](#inspector)                          |
| `com.deusald.deusaldunitytools.sceneautoloader`   | `DeusaldUnityTools.SceneAutoLoader.Editor`  | [Scene Autoloader](#scene-autoloader) (editor only)                       |
| `com.deusald.deusaldunitytools.localpackages`     | `DeusaldUnityTools.LocalPackages.Editor`    | [Local Packages](#local-packages) (editor only)                           |

The assemblies are auto-referenced, so code in `Assembly-CSharp` sees them without any setup. If your code lives in
your own assembly definition, add the assemblies of the modules you use to its references.

---

## Tweens

`com.deusald.deusaldunitytools.tweens` · depends on `com.unity.ugui`, `com.unity.modules.audio`, `com.unity.modules.uielements`

A lightweight, strictly typed tweening engine for script-based animation of UI and world-space objects. Based on
[unity-tweens](https://github.com/jeffreylanters/unity-tweens) by Jeffrey Lanters.

### Running tweens

Tweens are run by a `TweenEngine` component (or your own `ITweenEngine`). Add `TweenEngine` to a GameObject in the
scene; it updates every running tween in `LateUpdate`.

```csharp
[SerializeField] private TweenEngine _TweenEngine;
[SerializeField] private Transform   _Target;

private void Start()
{
    // Extension method: from 0 to 3 on the X axis.
    Tween tween = _Target.TweenPositionX(0f, 3f)
                         .SetDuration(1.5f)
                         .SetEase(EaseType.BackOut)
                         .AddOnEndCallback(_ => Debug.Log("Finished"));

    _TweenEngine.RunTween(tween);

    // Same thing, built from the tween class. SetTarget(component, to) starts from the current value.
    _TweenEngine.RunTween(new PositionTween().SetTarget(_Target, Vector3.one).SetDuration(3f));
}
```

Each tween can be cancelled with `Cancel()` and awaited until it ends or is cancelled:

```csharp
_TweenEngine.RunTween(tween);
await tween.AwaitDecommissionAsync();   // Awaitable
yield return tween.AwaitDecommission(); // or in a coroutine
```

A tween whose target component is destroyed cancels itself.

### Options

All setters return the tween, so they chain.

| Method                                    | Effect                                                                                    |
|-------------------------------------------|-------------------------------------------------------------------------------------------|
| `SetTarget(component, from, to)`          | Target and values. Overloads without `from` start from the current value; overloads without `component` are for [value tweens](#value-tweens) |
| `SetDuration(float)`                      | Duration in seconds                                                                       |
| `AddDelay(float)`                         | Delay before the tween starts                                                             |
| `SetEase(EaseType)`                       | Easing function (default `Linear`)                                                        |
| `SetAnimationCurve(AnimationCurve)`       | Custom easing curve, used instead of `EaseType`                                           |
| `SetLoops(int)` / `Infinite()`            | Number of loops, or loop forever                                                          |
| `PingPong()`                              | Play forwards, then backwards                                                             |
| `SetPingPongInterval(float)`              | Pause at the turning point of a ping-pong                                                 |
| `SetRepeatInterval(float)`                | Pause between loops                                                                       |
| `SetOffset(float)`                        | Start from this time into the tween                                                       |
| `UnscaledTime()`                          | Use `Time.unscaledDeltaTime`, so the tween runs while `Time.timeScale` is 0               |
| `SetFillMode(FillMode)`                   | What is applied before the start and after the end (default `Backwards`, see below)      |
| `SetDontInvokeWhenDestroyed(bool)`        | Skip the cancel callbacks when the tween is cancelled because its target was destroyed   |

`FillMode` works like CSS `animation-fill-mode`:

| FillMode    | Applies `from` during the delay | Keeps the final value after the end |
|-------------|---------------------------------|-------------------------------------|
| `None`      | no                              | no, returns to the initial value    |
| `Forwards`  | yes                             | no, returns to the initial value    |
| `Backwards` | no                              | yes                                 |
| `Both`      | yes                             | yes                                 |

`EaseType` has `Linear` and the `In`, `Out` and `InOut` variants of `Sine`, `Quad`, `Cubic`, `Quart`, `Quint`, `Expo`,
`Circ`, `Back`, `Elastic` and `Bounce`.

### Callbacks

| Method                         | Invoked                                                    |
|--------------------------------|------------------------------------------------------------|
| `AddOnAddCallback`             | When the tween is handed to the engine                     |
| `AddOnStartCallback`           | On the first update after the delay                        |
| `AddOnUpdateCallback`          | Every update, with the current value                       |
| `AddOnEndCallback`             | When the tween finishes                                    |
| `AddOnCancelCallback`          | When the tween is cancelled                                |
| `AddOnFinallyCallback`         | After it either finishes or is cancelled                   |

### Sequences

`TweenSequence` runs tweens one after another, in parallel, or at set times:

```csharp
TweenSequence sequence = new TweenSequence()
    .Append(_Target.TweenPositionX(0f, 3f).SetDuration(1f))   // runs first
    .Join(_Target.TweenLocalScale(Vector3.one * 2f).SetDuration(1f)) // runs together with the previous step
    .AppendInterval(0.5f)                                     // waits
    .Append(_Target.TweenPositionX(0f).SetDuration(1f))       // then runs
    .AppendCallback(() => Debug.Log("Finished sequence"))
    .Insert(0.25f, _Light.TweenLightIntensity(0f, 2f).SetDuration(0.5f)) // starts 0.25 s after the sequence starts
    .InsertCallback(1f, () => Debug.Log("One second in"));

_TweenEngine.RunSequence(sequence);
await sequence.AwaitDecommissionAsync();
```

`Cancel()` cancels the sequence and the tweens it is currently running.

### Available tweens

Every tween has a class (e.g. `PositionXTween`) and an extension method with `(from, to)` and `(to)` overloads.

| Target           | Extension methods                                                                                                                      |
|------------------|----------------------------------------------------------------------------------------------------------------------------------------|
| `Transform`      | `TweenPosition`, `TweenPositionX/Y/Z`, `TweenLocalPosition`, `TweenLocalPositionX/Y/Z`, `TweenRotation`, `TweenLocalRotation`, `TweenEulerAngles`, `TweenEulerAnglesX/Y/Z`, `TweenLocalEulerAngles`, `TweenLocalEulerAnglesX/Y/Z`, `TweenLocalScale`, `TweenLocalScaleX/Y/Z` |
| `RectTransform`  | `TweenAnchoredPosition`, `TweenAnchoredPositionX/Y`, `TweenAnchorMin`, `TweenAnchorMax`, `TweenOffsetMin`, `TweenOffsetMax`            |
| uGUI             | `Graphic.TweenGraphicColor`, `Graphic.TweenGraphicAlpha`, `Image.TweenImageFillAmount`, `CanvasGroup.TweenAlpha`                       |
| UI Toolkit       | `VisualElement.TweenOpacity`, `TweenColor`, `TweenBackgroundColor`, `TweenBackgroundColorAlpha`, `TweenTranslate`, `TweenTranslateX/Y`, `TweenScale`, `TweenRotate` |
| `SpriteRenderer` | `TweenSpriteRendererColor`, `TweenSpriteRendererAlpha`                                                                                 |
| `Light`          | `TweenLightColor`, `TweenLightIntensity`, `TweenLightRange`, `TweenLightSpotAngle`                                                     |
| `AudioSource`    | `TweenAudioSourceVolume`, `TweenAudioSourcePitch`, `TweenAudioSourcePan`, `TweenAudioSourceSpatialBlend`, `TweenAudioSourceReverbZoneMix`, `TweenAudioSourcePriority` |

### Value tweens

`FloatTween`, `Vector2Tween`, `Vector3Tween`, `Vector4Tween`, `ColorTween`, `QuaternionTween` and `RectTween` animate a
value without a target. Read it in the update callback:

```csharp
Tween counter = new FloatTween().SetTarget(0f, 1000f)
                                .SetDuration(2f)
                                .SetEase(EaseType.QuadOut)
                                .AddOnUpdateCallback((_, value) => _ScoreLabel.text = Mathf.RoundToInt(value).ToString());

_TweenEngine.RunTween(counter);
```

### Custom tweens

Derive from `Tween<TComponent, TData>` and implement three methods:

```csharp
public sealed class MaterialFloatTween : Tween<Renderer, float>
{
    private static readonly int _Cutoff = Shader.PropertyToID("_Cutoff");

    protected override float CurrentFromComponent()                   => Component!.material.GetFloat(_Cutoff);
    protected override float Lerp(float from, float to, float time)   => Mathf.LerpUnclamped(from, to, time);
    protected override void  ApplyToComponent(float value)            => Component!.material.SetFloat(_Cutoff, value);
}
```

---

## Extensions

`com.deusald.deusaldunitytools.extensions` · depends on `com.unity.ugui` (uGUI and TextMesh Pro)

### Text Warp

`TMP_Text.WarpText(AnimationCurve vertexCurve, float yCurveScaling)` bends a TextMesh Pro text along a curve. Call it
again after the text changes.

```csharp
[SerializeField] private TMP_Text       _Text;
[SerializeField] private AnimationCurve _VertexCurve   = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
[SerializeField] private float          _YCurveScaling = 100f;

private void Start() => _Text.WarpText(_VertexCurve, _YCurveScaling);
```

![text-warp-screenshot](images/text-warp.png)

### String extensions

```csharp
"Copied!".SaveToClipboard();                              // GUIUtility.systemCopyBuffer

string label = "Warning".RichColor("#FF0000").RichBold(); // <b><color=#FF0000>Warning</color></b>
```

Rich text helpers: `RichColor`, `RichSize`, `RichBold`, `RichItalic`, `RichUnderline`, `RichStrikethrough`, `RichFont`,
`RichAlign`, `RichGradient`, `RichRotation`, `RichSpace`.

### Unity extensions

| Extension                                              | Effect                                                         |
|--------------------------------------------------------|----------------------------------------------------------------|
| `Color.SetAlpha(float)`                                | Returns the color with a new alpha                             |
| `Image.SetAlpha(float)`                                | Sets the alpha of an `Image`                                   |
| `Color.ToHex()` / `string.FromHex()`                   | `#RRGGBBAA` string to and from `Color` (`FromHex` throws on invalid input) |
| `Color.Invert()`                                       | Inverted RGB, same alpha                                       |
| `Camera.ClipPlaneScreenToWorld(Vector3)`               | `ScreenToWorldPoint` with `z` set to 0                         |
| `Vector2/Vector3.With(x, y, z)`                        | Copy with only the given components replaced                   |
| `Vector2/Vector3.Add(x, y, z)`                         | Copy with the given values added to the components             |
| `Transform.SetLocalX/Y/Z(float)`                       | Sets one component of `localPosition`                          |
| `RectTransform.SetSizeDeltaX/Y(float)`                 | Sets one component of `sizeDelta`                              |

```csharp
transform.position = transform.position.With(y: 0f);
_Image.color       = "#3FA9F5FF".FromHex().SetAlpha(0.5f);
```

---

## Pooling

`com.deusald.deusaldunitytools.pooling`

A pool of prefab instances, kept per prefab. Pooled components implement `IPooledObject`; the pool itself is a
`PoolManagerBase` subclass that decides how instances are created, e.g. through your DI container.

```csharp
public class Bullet : MonoBehaviour, IPooledObject
{
    public void OnGetFromPool()  { /* reset state */ }
    public void OnReturnToPool() { /* stop effects */ }
}

public class PoolManager : PoolManagerBase
{
    protected override GameObject InnerInstantiate(Object prefab, Transform parent)
    {
        return (GameObject)Instantiate(prefab, parent);
    }
}
```

```csharp
_PoolManager.InitPool(_BulletPrefab, 20);                    // optional pre-warm

Bullet bullet = _PoolManager.GetFromPool(_BulletPrefab, _BulletsParent);
// ...
_PoolManager.ReturnToPool(bullet);

_PoolManager.ClearPools();                                   // destroys the pooled (inactive) instances
```

- `GetFromPool` creates a pool on first use, pre-warmed with the `Pool Init Size` set in the inspector, and
  instantiates a new object when the pool is empty.
- Inactive instances are parented under the pool manager.
- `ReturnToPool` destroys an object that did not come from the pool.

---

## Secure Player Prefs

`com.deusald.deusaldunitytools.secureplayerprefs`

`PlayerPrefs` with AES-encrypted keys and values, so players can't read or edit them. The AES key is derived from
`Password` (PBKDF2-SHA512).

```csharp
SecurePlayerPrefs.Password = "your-password"; // set once, before the first call

await SecurePlayerPrefs.SetIntAsync("coins", 100);
await SecurePlayerPrefs.SetFloatAsync("volume", 0.8f);
await SecurePlayerPrefs.SetStringAsync("name", "Player");
SecurePlayerPrefs.Save();

int    coins = await SecurePlayerPrefs.GetIntAsync("coins", defaultValue: 0);
bool   has   = await SecurePlayerPrefs.HasKeyAsync("coins");
string name  = await SecurePlayerPrefs.GetStringAsync("name");

SecurePlayerPrefs.DeleteKey("coins");
SecurePlayerPrefs.DeleteAll();

// The same encryption for your own data, e.g. a save file:
string encrypted = await SecurePlayerPrefs.SymmetricEncryptAsync(json);
string decrypted = await SecurePlayerPrefs.SymmetricDecryptAsync(encrypted);
```

- The encryption key is derived on first use and kept for the rest of the session: changing `Password` later has no
  effect until the next run.
- `Set*Async` and `Get*Async` log an error instead of throwing (for example when `Password` is not set); `Get*Async`
  then returns the default value.
- `DeleteKey` takes the key you used to save the value.

---

## Mobile

`com.deusald.deusaldunitytools.mobile` · depends on `com.unity.modules.androidjni`

Native Android and iOS functionality, backed by the `DeusaldJavaTools.aar` (Android) and `DeusaldSwiftTools.framework`
(iOS) plugins shipped in the package. The native code runs only in device builds; in the editor and on other platforms
the methods log what they would do, or return a fallback value.

### Android Tools

`AndroidTools` (static).

| Method                                    | Effect                                                                                          |
|-------------------------------------------|-------------------------------------------------------------------------------------------------|
| `GetKeyboardHeight()`                     | Height of the on-screen keyboard in pixels, including suggestion/emoji bars                     |
| `LaunchOrOpenPlayStore(packageName)`      | Launches the app; if it is not installed, opens its Play Store page (browser fallback)          |
| `TryOpenPlayStorePage(packageName)`       | Opens the app's Play Store page, with a browser fallback                                        |
| `OpenAppSettings()`                       | Opens this app's system settings                                                                |
| `InitShare(providerName)`                 | Initializes the share module; call once before sharing                                          |
| `ShareText(message)`                      | Native share sheet with text                                                                    |
| `ShareFile(filePath, message)`            | Native share sheet with a file                                                                  |
| `ShareFiles(filesPaths, message)`         | Native share sheet with several files                                                           |
| `RefreshCurrentActivity()`                | Re-reads Unity's current activity; called automatically on first use                            |

**Keyboard height** is in screen pixels. To place UI above the keyboard, scale it to your canvas:

```csharp
float rate      = referenceCanvasHeight / Screen.height;
float positionY = AndroidTools.GetKeyboardHeight() * rate + margin;
```

**`LaunchOrOpenPlayStore`** can only see apps listed in `<queries>` in your `AndroidManifest.xml`:

```xml
<manifest>
    <queries>
        <package android:name="com.google.android.play.games" />
    </queries>
</manifest>
```

**Sharing** needs a `FileProvider` in your `AndroidManifest.xml`. Replace `com.yourcompany.yourapp` with your own
unique string:

```xml
<manifest>
    <application>
        <provider android:name="androidx.core.content.FileProvider"
                  android:authorities="com.yourcompany.yourapp"
                  android:exported="false"
                  android:grantUriPermissions="true">
            <meta-data android:name="android.support.FILE_PROVIDER_PATHS" android:resource="@xml/file_provider_paths" />
        </provider>
    </application>
</manifest>
```

Then initialize the share module with the same authority before sharing:

```csharp
AndroidTools.InitShare("com.yourcompany.yourapp");
AndroidTools.ShareFile(Path.Combine(Application.persistentDataPath, "screenshot.png"), "Look at this!");
```

### iOS Tools

`iOSTools` (static).

| Method                        | Effect                                                                         |
|-------------------------------|--------------------------------------------------------------------------------|
| `OpenAppStore(appId)`         | Opens the App Store page; `appId` is the numeric Apple ID, not the bundle ID   |
| `OpenAppSettings()`           | Opens this app's page in iOS Settings                                          |
| `ShareText(text)`             | Native share sheet with text                                                   |
| `ShareFile(path, text)`       | Native share sheet with a file                                                 |
| `ShareFiles(paths, text)`     | Native share sheet with several files                                          |

```csharp
iOSTools.OpenAppStore("123456789");
iOSTools.ShareText("Check out this game!");
```

### Haptic

Haptic feedback on iOS and Android. It uses the richest feedback the device supports and falls back to
`Handheld.Vibrate()`; `HapticType.Default` always uses `Handheld.Vibrate()`.

```csharp
Haptic.TurnedOn = playerSettings.HapticsEnabled; // true by default
Haptic.Perform(HapticType.Success);
```

`HapticType`: `Default`, `Light`, `Medium`, `Heavy`, `Selection`, `Success`, `Warning`, `Error`.

On Android, add the vibrate permission to your `AndroidManifest.xml`:

```xml
<manifest>
    <uses-permission android:name="android.permission.VIBRATE" />
</manifest>
```

### Game Center Auth

Signs the player in to Game Center and generates an identity verification signature, which your server can use to
verify the player (see Apple's `fetchItems(forIdentityVerificationSignature:)`).

```csharp
bool authenticated = await GameCenterAuth.AuthenticateGameCenterPlayerAsync();
if (!authenticated) throw new Exception("Failed to authenticate in Game Center!");

GameCenterAuth.GameCenterAuthResult result = await GameCenterAuth.GenerateIdentityVerificationSignatureAsync();
if (!result.Success) throw new Exception($"Failed to get Game Center auth data. {result.Error}");

// Send to your server for verification:
// result.PublicKeyUrl, result.Signature, result.Salt, result.TimeStamp, result.TeamPlayerId, result.BundleId
// Player info: result.GamePlayerId, result.DisplayName, result.Alias
```

---

## Fps Counter

`com.deusald.deusaldunitytools.fpscounter`

A component that averages unscaled frame times over the last `Samples` frames (set in the inspector, default 3).

```csharp
[SerializeField] private FpsCounter _FpsCounter;
[SerializeField] private TMP_Text   _Label;

private void Update()
{
    _Label.text = $"{_FpsCounter.FPS:0} FPS ({_FpsCounter.FrameTime * 1000f:0.0} ms)";
}
```

`FPS` is frames per second, `FrameTime` the average frame time in seconds, and `Clear()` resets the samples.

---

## Inspector

`com.deusald.deusaldunitytools.inspector`

Two attributes, drawn by an inspector that applies to every `MonoBehaviour` (a more specific custom editor for a type
still takes precedence). Without this package installed, MonoBehaviours keep Unity's default inspector.

### `[Button]`

Shows a method as a button below the default inspector. Clicking it calls the method on the selected object, in Edit
and Play Mode.

```csharp
public class Example : MonoBehaviour
{
    [Button]
    private void ResetCounter()
    {
        Debug.Log("Counter reset");
    }
}
```

- Works on public and private methods, instance or static.
- Only parameterless methods are supported. The button is labelled with the method name.

### `[ShowInInspector]`

Shows non-serialized fields and properties, for runtime debugging and live state inspection.

```csharp
public class Example : MonoBehaviour
{
    [ShowInInspector]
    private int _RuntimeCounter;              // editable

    [ShowInInspector(ReadOnly = true)]
    public float Health => _CurrentHealth;    // read-only

    [ShowInInspector(true)]                   // same as ReadOnly = true
    private Vector3 _CurrentVelocity;
}
```

- Works on public and private instance fields and readable properties. Editing a property without a setter does
  nothing, so mark those `ReadOnly`.
- Editable types: `int`, `float`, `bool`, `string`, enums, `Vector2`, `Vector3`. Other types show only their type name.
- Values refresh when the inspector repaints (on mouse move or selection change), not every frame.
- The values are not serialized, so they are not saved, are not covered by Undo, and reset on domain reload.

---

## Scene Autoloader

`com.deusald.deusaldunitytools.sceneautoloader` · editor only

Marks one scene as the master scene that always loads when you enter Play Mode, so the game starts from it. On stop,
the scene you were editing is reopened. Modified scenes are offered for saving first; cancelling that also cancels
Play Mode.

Use the **Tools > Scene Autoload** menu:

- **Select Master Scene...** picks the scene and turns autoloading on.
- **Load Master On Play** / **Don't Load Master On Play** toggles it.

The settings are stored in `EditorPrefs`, per machine. You can also set them from your own editor code:

```csharp
SceneAutoLoader.SetManually("Assets/Scenes/Master.unity", loadMasterOnPlay: true);
```

![scene-autoloader-screenshot](images/scene-autoloader.png)

---

## Local Packages

`com.deusald.deusaldunitytools.localpackages` · editor only

Keeps Asset Store and OpenUPM packages as embedded packages inside the project's `Packages` folder instead of
installing them from a registry. They are committed with the project, so nobody needs an Asset Store or registry
account to open it, and Unity shows no unsigned package warnings for them. The module remembers where each package
came from, tells you when a newer version is out and installs OpenUPM packages for you.

Open **Tools > Local Packages**. The list shows every local package with its source, installed version and status;
select one to see its details. Use **+** to add packages.

### Installing from OpenUPM

**+ > Install from OpenUPM** takes a package name (`com.cysharp.unitask`) or its OpenUPM page URL, and a version
(empty for the latest). The package is downloaded into `Packages/<name>` and marked as a local package. No scoped
registry or account is needed.

- **Dependencies** that come from OpenUPM are installed the same way. `com.unity.*` dependencies and packages already
  in the `Packages` folder are skipped; dependencies OpenUPM doesn't have are listed in the Console.
- **Downloads are verified** against OpenUPM's checksum, and everything is downloaded before the `Packages` folder is
  touched, so a failed install changes nothing.
- **A package still listed in `Packages/manifest.json`** is reported in the Console: the local copy takes
  precedence, so you can remove the manifest entry.

### Adding Asset Store packages

The Asset Store needs a signed-in Unity account to download, so these packages are added by hand:

1. Import the asset, then move its folder to `Packages/` and add a `package.json` with a 3 part version, e.g.
   `3.984.0` for the Asset Store's `3.984`.
2. Give its scripts assembly definitions. Scripts outside an assembly definition are not compiled in packages, and
   `Editor` folders have no special meaning there, so editor code needs its own Editor-only assembly definition.
3. In **+ > Mark a Package Already in the Packages Folder**, pick the package and paste its Asset Store page as the
   source.

Asset Store content is licensed per seat, so keep such a project in a private repository.

### The marker file

Every local package has a `localpackage.json` in its folder. The window writes it, and you can also edit it by hand:

```json
{
    "source": "https://assetstore.unity.com/packages/tools/utilities/editor-console-pro-11889",
    "version": "",
    "silenceWarnings": true
}
```

- **`source`**: the Asset Store page (it must end with the product id, `-11889`) or the OpenUPM package page
  (`https://openupm.com/packages/com.vendor.package/`).
- **`version`**: the upstream version kept in the folder. Leave it empty to use the `package.json` version.
- **`silenceWarnings`**: hides the package's compiler warnings (see below).

### Checking for updates

**Check for Updates** asks every source for its newest version. Packages are also checked when the window opens if
their last check is older than a day; results are cached in `Library/DeusaldUnityTools/LocalPackagesCache.json`.
Versions are compared part by part with missing parts counting as 0, so `3.984.0` matches `3.984`.

OpenUPM is asked through its registry API. The Asset Store has no public API, so its product page is read instead:
that works without signing in, but it can break if Unity changes the page.

When an update is out:

- **Install Latest** (OpenUPM packages) replaces the package files with the latest version and keeps the marker
  settings. The old folder is moved away as a whole first, so if a file is in use (a loaded native plugin, for
  example) nothing is changed.
- **Mark as Updated** is for packages you update by hand: replace the files, then click it to set the installed
  version to the latest one. When the marker has no version, it is written into `package.json` as a 3 part version
  (`3.985` becomes `3.985.0`).

### Silencing compiler warnings

Third-party code often triggers warnings your own settings add, such as nullable warnings. **Silence Warnings**
writes a `csc.rsp` with `-warn:0` next to every assembly definition of the package, which hides the compiler warnings
of those assemblies only. Options already in an existing `csc.rsp` are kept.

If the files go missing, for example after replacing the package files, the window says so and **Reapply** writes
them again. **Install Latest** and **Mark as Updated** reapply them on their own, and **Remove Marker** removes them.

---

## Upgrading to 4.0

4.0 splits the single `DeusaldUnityTools` assembly into one package and assembly per module.

- Namespaces, type names and script GUIDs are unchanged, so scenes, prefabs and code in `Assembly-CSharp` keep working.
- If your own assembly definitions referenced `DeusaldUnityTools` or `DeusaldUnityToolsEditor`, replace that reference
  with the assemblies of the modules you use (see [Modules](#modules)).
- Installing `com.deusald.deusaldunitytools` still gets you everything. Over git URLs, add each module package as well
  (see [Git URL](#git-url)).

---

## Native Swift/Java projects

The sources of the native plugins used by the [Mobile](#mobile) module are in this repository.

### DeusaldSwiftTools (iOS)

Build the `DeusaldSwiftTools` project in Xcode in the Release configuration. Then go to Derived Data
(Xcode Settings > Locations), take `Build/Products/Release-iphoneos/DeusaldSwiftTools.framework`, and replace
`Packages/com.deusald.deusaldunitytools.mobile/Plugins/iOS/DeusaldSwiftTools.framework` with it.

### DeusaldJavaTools (Android)

Build the release `.aar` of the `DeusaldJavaTools` module in Android Studio (or with `gradlew :DeusaldJavaTools:assembleRelease`),
take it from `DeusaldJavaTools/DeusaldJavaTools/build/outputs/aar/`, and replace
`Packages/com.deusald.deusaldunitytools.mobile/Plugins/Android/DeusaldJavaTools.aar` with it, keeping the file name.
