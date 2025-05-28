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

#if TEST_SCRIPT || (UNITY_IOS && !UNITY_EDITOR)
using System.Runtime.InteropServices;
#endif

namespace DeusaldGameCenterAuth
{
    public class GameCenterSignature
    {
        public delegate void OnSucceeded 
        ( 
            string publicKeyUrl, 
            ulong timestamp,
            string signature,
            string salt,
            string gamePlayerId,
            string teamPlayerId,
            string displayName,
            string alias,
            string bundleID
        );

        public delegate void OnFailed(string reason);

        #if TEST_SCRIPT || (UNITY_IOS && !UNITY_EDITOR)
        [DllImport("__Internal")]    
        private static extern void GenerateIdentityVerificationSignature(OnSucceeded onSucceeded, OnFailed onFailed); 
        #endif

        public static void Generate(OnSucceeded onSucceeded, OnFailed onFailed)
        {
            #if TEST_SCRIPT || (UNITY_IOS && !UNITY_EDITOR)
            GenerateIdentityVerificationSignature(onSucceeded, onFailed);
            #else
            onFailed.Invoke("GameCenter authentication is only available for iOS");
            #endif
        }
    }
}