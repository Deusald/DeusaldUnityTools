# DeusaldUnityTools

Deusald Unity Tools are set of useful modules, classes, extensions and methods.

### Unity Install

You can add this library to Unity project in 2 ways:

**Warning: Plugin require [Odin inspector](https://assetstore.unity.com/packages/tools/utilities/odin-inspector-and-serializer-89041) installed in project.**

1. In package manager add library as git repository using format:
   `https://github.com/Deusald/DeusaldUnityTools.git?path=/Packages/DeusaldUnityTools#vX.X.X`
2. Add package using Scoped Register: todo

## Features

* Scene Autoloader
* Fps Counter
* Object Pool System
* Text Warp
* Extension method to copy string into clipboard
* Some useful unity extension methods
* Secure Player Prefs

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