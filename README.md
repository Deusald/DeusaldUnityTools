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
private struct GameCenterAuth
{
    public bool   Success      { get; set; }
    public string Error        { get; set; }
    public string PublicKeyUrl { get; set; }
    public string Signature    { get; set; }
    public ulong  TimeStamp    { get; set; }
    public string Salt         { get; set; }
    public string GamePlayerId { get; set; }
    public string TeamPlayerId { get; set; }
    public string DisplayName  { get; set; }
    public string Alias        { get; set; }
    public string BundleId     { get; set; }
}

private static TaskCompletionSource<GameCenterAuth> _SignatureTask;

[MonoPInvokeCallback(typeof(GameCenterSignature.OnSucceeded))]
private static void OnSuccess(string publicKeyUrl, ulong timestamp, string signature, string salt,
                              string gamePlayerId, string teamPlayerId, string displayName, string alias, string bundleId)
{
    _SignatureTask.TrySetResult(new GameCenterAuth
    {
        Success      = true,
        Error        = "",
        PublicKeyUrl = publicKeyUrl,
        TimeStamp    = timestamp,
        Signature    = signature,
        Salt         = salt,
        GamePlayerId = gamePlayerId,
        TeamPlayerId = teamPlayerId,
        DisplayName  = displayName,
        Alias        = alias,
        BundleId     = bundleId
    });
}

[MonoPInvokeCallback(typeof(GameCenterSignature.OnFailed))]
private static void OnFailed(string reason)
{
    _SignatureTask.TrySetResult(new GameCenterAuth
    {
        Success      = false,
        Error        = reason,
        PublicKeyUrl = "",
        TimeStamp    = 0,
        Signature    = "",
        Salt         = "",
        GamePlayerId = "",
        TeamPlayerId = "",
        DisplayName  = "",
        Alias        = "",
        BundleId     = ""
    });
}

public async Task LoginAsync()
{
    _SignatureTask = new TaskCompletionSource<GameCenterAuth>();
    GameCenterSignature.Generate(OnSuccess, OnFailed);
    GameCenterAuth authResult = await _SignatureTask.Task;
}
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