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

using UnityEngine;

namespace DeusaldUnityTools
{
    /// <summary> Scene-scoped singleton for MonoBehaviour. </summary>
    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        #region Variables

        private static T _Instance;

        // ReSharper disable once StaticMemberInGenericType
        private static bool _Quiting;

        #endregion Variables

        #region Properties

        public static T Instance
        {
            get
            {
                if (_Instance != null) return _Instance;

                _Instance = FindObjectOfType<T>();
                if (_Instance != null) return _Instance;

                if (_Quiting) return null;

                var obj = new GameObject("MonoSingleton<" + typeof(T).Name + ">");

                _Instance = obj.AddComponent<T>();

                return _Instance;
            }
        }

        #endregion Properties

        #region Special Methods

        protected virtual void OnApplicationQuit()
        {
            _Quiting = true;
        }

        protected virtual void OnDestroy()
        {
            _Quiting = true;
        }

        #endregion Special Methods
    }
}