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

using JetBrains.Annotations;
using System.Threading.Tasks;
#if TEST_SCRIPT || (UNITY_IOS && !UNITY_EDITOR)
using System.Runtime.InteropServices;
#endif

namespace DeusaldUnityTools
{
    [PublicAPI]
    public static class GameCenterAuth
    {
        [PublicAPI]
        public struct GameCenterAuthResult
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
        
        #if TEST_SCRIPT || (UNITY_IOS && !UNITY_EDITOR)
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void GenerateSucceeded(
            string publicKeyUrl,
            ulong timestamp,
            string signature,
            string salt,
            string gamePlayerId,
            string teamPlayerId,
            string displayName,
            string alias,
            string bundleId
        );

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void GenerateFailed(string reason);
        
        [DllImport("__Internal")]
        private static extern void _GenerateIdentityVerificationSignature(
            GenerateSucceeded onSuccess,
            GenerateFailed onFailure
        );
        
        private static TaskCompletionSource<GameCenterAuthResult> _SignatureTask;
        
        #endif

        public static async Task<GameCenterAuthResult> GenerateIdentityVerificationSignatureAsync()
        {
            #if TEST_SCRIPT || (UNITY_IOS && !UNITY_EDITOR)
            _SignatureTask = new TaskCompletionSource<GameCenterAuthResult>();
            _GenerateIdentityVerificationSignature(OnSucceeded, OnFailed);
            return await _SignatureTask.Task;
            #else
            return await NonIOSPlatformResultAsync();
            #endif
        }
        
        #if TEST_SCRIPT || (UNITY_IOS && !UNITY_EDITOR)
        
        [AOT.MonoPInvokeCallback(typeof(GenerateSucceeded))]
        private static void OnSucceeded(
            string publicKeyUrl,
            ulong timestamp,
            string signature,
            string salt,
            string gamePlayerId,
            string teamPlayerId,
            string displayName,
            string alias,
            string bundleId)
        {
            _SignatureTask.TrySetResult(new GameCenterAuthResult
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
        
        [AOT.MonoPInvokeCallback(typeof(GenerateFailed))]
        private static void OnFailed(string reason)
        {
            _SignatureTask.TrySetResult(new GameCenterAuthResult
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
        
        #else

        private static Task<GameCenterAuthResult> NonIOSPlatformResultAsync()
        {
            return Task.FromResult(new GameCenterAuthResult
            {
                Success      = false,
                Error        = "GameCenter authentication is only available for iOS!",
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
        
        #endif
    }
}