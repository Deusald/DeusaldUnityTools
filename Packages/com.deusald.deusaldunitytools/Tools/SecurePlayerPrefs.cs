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

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeusaldUnityTools
{
    public static class SecurePlayerPrefs
    {
        public static string Password      { get; set; } = string.Empty;
        public static bool   IsPasswordSet => !string.IsNullOrEmpty(Password);

        private static byte[] _KeyBytes;
        private static byte[] _IVBytes;

        private const int _SYNC_PASSWORD_HASH_BYTE_SIZE = 32;
        private const int _SYNC_SALT_HASH_BYTE_SIZE     = 16;

        public static async Task SetIntAsync(string key, int value)
        {
            try
            {
                CheckPassword();
                await SetStringAsync(key, Convert.ToString(value));
            }
            catch (Exception e)
            {
                Debug.LogError($"Error while setting {key} to player prefs!: {e}.");
            }
        }

        public static async Task<int> GetIntAsync(string key, int defaultValue = 0)
        {
            try
            {
                CheckPassword();
                return int.TryParse(await GetStringAsync(key), out int retValue) ? retValue : defaultValue;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error while getting {key} from player prefs!: {e}.");
            }

            return defaultValue;
        }

        public static async Task SetFloatAsync(string key, float value)
        {
            try
            {
                CheckPassword();
                await SetStringAsync(key, Convert.ToString(value, CultureInfo.InvariantCulture));
            }
            catch (Exception e)
            {
                Debug.LogError($"Error while setting {key} to player prefs!: {e}.");
            }
        }

        public static async Task<float> GetFloatAsync(string key, float defaultValue = 0.0f)
        {
            try
            {
                CheckPassword();
                return float.TryParse(await GetStringAsync(key), out float retValue) ? retValue : defaultValue;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error while getting {key} from player prefs!: {e}.");
            }

            return defaultValue;
        }

        public static async Task SetStringAsync(string key, string value)
        {
            try
            {
                CheckPassword();
                PlayerPrefs.SetString(await SymmetricEncryptAsync(key), await SymmetricEncryptAsync(value));
            }
            catch (Exception e)
            {
                Debug.LogError($"Error while setting {key} to player prefs!: {e}.");
            }
        }

        public static async Task<string> GetStringAsync(string key, string defaultValue = "")
        {
            try
            {
                CheckPassword();
                return await SymmetricDecryptAsync(await GetRowStringAsync(key, defaultValue));
            }
            catch (Exception e)
            {
                Debug.LogError($"Error while getting {key} from player prefs!: {e}.");
            }

            return defaultValue;
        }

        public static async Task<bool> HasKeyAsync(string key)
        {
            try
            {
                CheckPassword();
                return PlayerPrefs.HasKey(await SymmetricEncryptAsync(key));
            }
            catch (Exception e)
            {
                Debug.LogError($"Error while checking {key} existance!: {e}.");
            }

            return false;
        }

        public static void Save()
        {
            CheckPassword();
            PlayerPrefs.Save();
        }

        public static void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
        }

        public static void DeleteKey(string key)
        {
            PlayerPrefs.DeleteKey(key);
        }

        public static async Task<string> SymmetricEncryptAsync(string text)
        {
            CheckKeyAndSaltBytes();
            using Aes aes = Aes.Create();
            aes.Key = _KeyBytes;
            aes.IV  = _IVBytes;
            using ICryptoTransform   encryptor       = aes.CreateEncryptor();
            using MemoryStream       encryptedStream = new MemoryStream();
            await using CryptoStream cryptoStream    = new CryptoStream(encryptedStream, encryptor, CryptoStreamMode.Write);
            await cryptoStream.WriteAsync(Encoding.UTF8.GetBytes(text));
            cryptoStream.FlushFinalBlock();
            byte[] encrypted = encryptedStream.ToArray();
            return Convert.ToBase64String(aes.IV.Concat(encrypted).ToArray());
        }

        public static async Task<string> SymmetricDecryptAsync(string encryptedText)
        {
            CheckKeyAndSaltBytes();
            byte[]    encryptedBytes = Convert.FromBase64String(encryptedText);
            using Aes aes            = Aes.Create();
            aes.Key = _KeyBytes;
            aes.IV  = encryptedBytes.Take(aes.IV.Length).ToArray();
            using MemoryStream       memoryStream    = new MemoryStream();
            using ICryptoTransform   cryptoTransform = aes.CreateDecryptor();
            await using CryptoStream cryptoStream    = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write);
            await cryptoStream.WriteAsync(encryptedBytes.Skip(aes.IV.Length).ToArray());
            cryptoStream.FlushFinalBlock();
            return Encoding.UTF8.GetString(memoryStream.ToArray());
        }

        private static void CheckKeyAndSaltBytes()
        {
            if (_KeyBytes != null && _IVBytes != null) return;
            _KeyBytes = CreateHashPassBytes(Encoding.UTF8.GetBytes(Password), _SYNC_PASSWORD_HASH_BYTE_SIZE);
            char[] charArray = Password.ToCharArray();
            Array.Reverse(charArray);
            _IVBytes = CreateHashPassBytes(Encoding.UTF8.GetBytes(new string(charArray)), _SYNC_SALT_HASH_BYTE_SIZE);
        }

        private static byte[] CreateHashPassBytes(byte[] keyBytes, int size)
        {
            using var                sha512        = SHA512.Create();
            byte[]                   saltBytes     = sha512.ComputeHash(keyBytes);
            using Rfc2898DeriveBytes hashPassBytes = new Rfc2898DeriveBytes(keyBytes, saltBytes, 555, HashAlgorithmName.SHA512);
            return hashPassBytes.GetBytes(size);
        }

        private static async Task<string> GetRowStringAsync(string key, string defaultValue = "")
        {
            CheckPassword();
            return PlayerPrefs.GetString(await SymmetricEncryptAsync(key), await SymmetricEncryptAsync(defaultValue));
        }

        private static void CheckPassword()
        {
            if (IsPasswordSet) return;
            throw new Exception("Password is not set for ZPlayerPrefs");
        }
    }
}