# DeusaldUnityTools

Deusald Unity Tools are set of useful modules, classes, extensions and methods.

### Unity Install

You can add this library to Unity project in 2 ways:

1. In package manager add library as git repository using format:
   `https://github.com/Deusald/DeusaldUnityTools.git?path=/Packages/com.deusald.deusaldunitytools#vX.X.X`
2. Add package using Scoped Register: https://openupm.com/packages/com.deusald.deusaldunitytools/

## Features

* Scene Autoloader
* Fps Counter
* Object Pool System
* Text Warp
* Extension method to copy string into clipboard
* Some useful unity extension methods
* Secure Player Prefs
* Apple Game Center Auth Plugin
* Tweens System
* Android and iOS Tools

## Native Swift/Java Projects
### Deusald Swift Tools
To update the iOS Framework, after building it in XCode, go to Derived data (Xcode Preferences/Locations) -> Build/Products/Release-iphoneos/DeusaldSwiftTools.framework and replace it under Plugins/iOS folder.

## Editor Tools

### Scene Autoloader
With this tool you can mark one scene as a master scene that will always load on play to start game from it.

![scene-autoloader-screenshot](images/scene-autoloader.png)

## Extensions

### Text Warp
Text Mesh Pro extension method that lets you warp text.

![text-warp-screenshot](images/text-warp.png)

## Secure Player Prefs
Secure Player Prefs is a module that lets you encrypt player prefs so it can't be modified or read by players.

```csharp
SecurePlayerPrefs.Password = "password";
await SecurePlayerPrefs.SymmetricEncryptAsync(secretData);
string decryptedSecretData = await SecurePlayerPrefs.SymmetricDecryptAsync(encryptedSecretData);
await SecurePlayerPrefs.SetStringAsync(key, value);
await SecurePlayerPrefs.GetIntAsync(key);
// etc.
```

## Game Center Auth Plugin
Game Center Auth Plugin lets you get detailed data about player logged in using iOS Game Center. 
This data also contains fields that can be used to securely authenticate player using external server.

```csharp
if (Social.localUser.authenticated) return;
TaskCompletionSource<bool> authenticateTask = new TaskCompletionSource<bool>();
Social.localUser.Authenticate(result => { authenticateTask.TrySetResult(result); });

bool authenticateResult = await authenticateTask.Task;

if (!authenticateResult) throw new Exception("Failed to authenticate in game center!");

GameCenterAuth.GameCenterAuthResult authResult = await GameCenterAuth.GenerateIdentityVerificationSignatureAsync();

if (!authResult.Success) throw new Exception($"Failed to get game center auth data. {authResult.Error}");

Debug.Log(authResult.PublicKeyUrl);
Debug.Log(authResult.Signature);
Debug.Log(authResult.TimeStamp);
Debug.Log(authResult.Salt);
Debug.Log(authResult.GamePlayerId);
Debug.Log(authResult.TeamPlayerId);
Debug.Log(authResult.DisplayName);
Debug.Log(authResult.Alias);
Debug.Log(authResult.BundleId);
```

## Tweens System
An extremely light weight, extendable and customisable tweening engine made for strictly typed script-based animations for user-interfaces and world-space objects optimised for all platforms.

Tweens module based on unity-tweens by Jeffrey Lanters - https://github.com/jeffreylanters/unity-tweens

```csharp
Tween positionTween = new PositionTween().AddOnEndCallback(_ => Debug.Log("Finished")).SetTarget(_Target, Vector3.zero, Vector3.one).SetDuration(3f).SetEase(EaseType.BounceInOut);
_TweenEngine.RunTween(positionTween);

TweenSequence sequence = new TweenSequence().AppendInterval(2f).Append(_Target.TweenPositionX(0f, 3f).SetDuration(3f)).AppendInterval(2f).AppendCallback(() => Debug.Log("Finished Sequence"));
_TweenEngine.RunSequence(sequence);
```

## Android Tools
Set of methods that could be useful on Android devices:

### GetKeyboardHeight()
Use to get keyboard height to place elements above the keyboard. Includes decorative elements like suggestions, emojis, etc.
```csharp
float rate = referenceCanvasHeight / Screen.height;
positionY = AndroidAppLauncher.GetKeyboardHeight() * rate + margin
```

### LaunchOrOpenPlayStore(string packageName)
Opens app and if that app is not installed, then opens a Play Store page for a specific app, with fallback to browser if Play Store isn't available.
To be able to use this method, you need to specify which apps will be checked using this method in an Android manifest file:
```xml
<manifest>
   <application>
  </application>
    <queries>
        <package android:name="com.google.android.play.games" />
    </queries>
</manifest>
```

### TryOpenPlayStorePage(string packageName)
Opens a Play Store page for a specific app, with fallback to browser if Play Store isn't available.

### OpenAppSettings()
Opens app settings.

### ShareText(string text)
Uses native Android methods to share text via another app.
To be able to use this method and other Share Android method you must put this code in your AndroidManifest.xml:
```xml
<manifest>
   <application>
      <provider android:name="androidx.core.content.FileProvider" android:authorities="com.yourcompany.yourapp" android:exported="false" android:grantUriPermissions="true">
         <meta-data android:name="android.support.FILE_PROVIDER_PATHS" android:resource="@xml/file_provider_paths" />
      </provider>
   </application>
</manifest>
```
You must replace `com.yourcompany.yourapp` in `android:authorities` with your own unique string!
Before executing any Share methods on Android you must also initialize the Share module by calling (replace `com.yourcompany.yourapp` with the same string you've used in `android:authorities`:
```csharp
AndroidTools.InitShare("com.yourcompany.yourapp");
```
### ShareFile(string path, string text)
Uses native Android methods to share the file via another app.

### ShareFiles(string[] paths, string text)
Uses native Android methods to share the files via another app.

## iOS Tools
Set of methods that could be useful on iOS devices:

### OpenAppStore(string appId)
Opens the App Store page for the specified App ID. AppId -> The app’s numeric Apple ID (not the bundle ID).

### OpenAppSettings()
Opens the iOS settings screen from the app.

### ShareText(string text)
Uses native iOS methods to share text via another app.

### ShareFile(string path, string text)
Uses native iOS methods to share the file via another app.

### ShareFiles(string[] paths, string text)
Uses native iOS methods to share the files via another app.

## Haptic System
Haptic System lets you use haptic feedback on both iOS and Android systems. To use on Android, add this permission to the AndroidManifest file:
```xml
<manifest>
   <application>
   </application>
   <uses-permission android:name="android.permission.VIBRATE" />
</manifest>
```

```csharp
Haptic.TurnedOn = true;
Haptic.Perform(HapticType.Success);
```