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

using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace DeusaldUnityTools
{
    [PublicAPI]
    public class FpsCounter : MonoBehaviour
    {
        #region Properties

        public float FPS
        {
            get
            {
                float result = 0.0f;
                for (int i = 0; i < _Samples; ++i)
                {
                    result += _Timing[i];
                }

                return _Samples / result;
            }
        }

        public float FrameTime
        {
            get
            {
                float result = 0.0f;
                for (int i = 0; i < _Samples; ++i)
                {
                    result += _Timing[i];
                }

                return result / _Samples;
            }
        }

        #endregion Properties
        
        #region Variables

        [SerializeField] private int _Samples = 3;

        private int _Index;

        private readonly List<float> _Timing = new();

        #endregion Variables

        #region Special Methods

        private void Awake()
        {
            for (int i = 0; i < _Samples; ++i) _Timing.Add(0.0f);
        }

        private void Update()
        {
            _Timing[_Index] = Time.unscaledDeltaTime;
            _Index          = (_Index + 1) % _Samples;
        }

        #endregion Special Methods

        #region Public Methods

        public void Clear()
        {
            for (int i = 0; i < _Samples; ++i)
            {
                _Timing[i] = 0.0f;
            }
        }

        #endregion Public Methods
    }
}